#!/usr/bin/env python3
"""
fix_script_refs.py - Comprehensive remap of broken script GUIDs (post AssetRipper plugin removal).

When AssetRipper exports a Unity build, it dumps every referenced package as a DLL into
Assets/Plugins/. When we delete those DLLs and use the Package Manager versions instead,
the scene/prefab references break:
  - Each old DLL had its own GUID (stored in the .dll.meta we deleted).
  - Inside that DLL, many classes shared the same GUID with different fileIDs.
  - The Package Manager versions use one .cs file per class, each with its own GUID
    and a canonical fileID of 11500000.

This script:
  1. Walks Library/PackageCache to build a map of {class fingerprint -> (package guid, 11500000)}.
     Fingerprint = set of serialized field names recursively from class + base classes.
  2. Walks Assets/ to find every (guid, fileID) pair that's no longer resolvable, plus the
     field names following each occurrence (the actual instance data lets us identify class).
  3. For each unresolvable pair, finds the best-matching package class by field overlap.
  4. Either prints the proposed remap (default) or applies it (--write).

Usage:
  python fix_script_refs.py                # report-only dry run
  python fix_script_refs.py --write        # apply
"""

import os
import re
import sys
import argparse
from pathlib import Path
from collections import defaultdict

REPO_ROOT = Path(__file__).resolve().parent
ASSETS_DIR = REPO_ROOT / 'Assets'
PKG_DIR = REPO_ROOT / 'Library' / 'PackageCache'

YAML_EXTS = {'.unity', '.prefab', '.asset', '.controller', '.mat', '.anim',
             '.physicMaterial', '.physicsMaterial2D', '.lighting', '.preset',
             '.spriteatlas'}

# Pattern: m_Script: {fileID: X, guid: Y, type: 3}
SCRIPT_REF_PAT = re.compile(r'm_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]{32}),\s*type:\s*3\}')

# Manually identified (old_guid, fileID) -> package class name, for cases where the
# fingerprint matcher can't score confidently (classes whose serialized fields use
# attribute syntax the regex extractor misses, or single-field classes).
# The class GUID is resolved from PackageCache <ClassName>.cs.meta at runtime.
MANUAL_REMAP = {
    ('c88ab7b37c4f350242674d2efd621c19',  938447500): 'UniversalAdditionalCameraData',
    ('c88ab7b37c4f350242674d2efd621c19',  796348501): 'Light2D',
    ('c88ab7b37c4f350242674d2efd621c19', -1431003440): 'UniversalAdditionalLightData',
    ('c88ab7b37c4f350242674d2efd621c19',  474283971): 'UniversalRenderPipelineGlobalSettings',
    ('c88ab7b37c4f350242674d2efd621c19', -549186028): 'UniversalRenderPipelineAsset',
    ('67dfb1fdfb2b407222eda8e23ac8b724', -1936749209): 'TMP_StyleSheet',
    ('d3e719b59ab71ba3f6b398058c866280', -1200242548): 'Mask',
    ('d3e719b59ab71ba3f6b398058c866280', -1862395651): 'EventTrigger',
    ('57c9a3e5193e26c4b968cc86e528416d', -1244478167): 'DebugUIHandlerContainer',
}


def resolve_class_guid_by_filename(class_name):
    """Find <class_name>.cs.meta in PackageCache and return its guid."""
    hits = list(PKG_DIR.rglob(f'{class_name}.cs.meta'))
    for h in hits:
        m = re.search(r'^guid:\s*([a-f0-9]{32})', h.read_text(encoding='utf-8', errors='ignore'), re.MULTILINE)
        if m:
            return m.group(1)
    return None

# C# field declaration matching: covers public, [SerializeField] private/protected
# We use a forgiving regex; the goal is field NAMES, not perfect parse
CLASS_DECL_PAT = re.compile(
    r'(?:public\s+|internal\s+)?(?:abstract\s+|sealed\s+|static\s+|partial\s+)*class\s+(\w+)(?:\s*<[^>]*>)?(?:\s*:\s*([^\n{]+))?',
    re.MULTILINE
)
SERIALIZED_FIELD_PAT = re.compile(
    r'(?:^|\n)\s*(?:\[SerializeField[^\]]*\]\s*)?(?:public|private|protected|internal)\s+'
    r'(?:static\s+)?(?:readonly\s+)?'  # skip statics/readonly accidentally
    r'(?!const\b)(?!event\b)'  # exclude const, event
    r'(?:[\w\.<>,\[\]\s]+?)\s+(m_\w+|\w+)\s*(?:=|;|//)',
)

# Sometimes the field name appears as "private Type m_Name" without [SerializeField] —
# Unity serializes [SerializeField] private fields AND public fields by default. Be conservative.

def collect_class_fields_in_file(path: Path):
    """Return list of (class_name, base_name_or_None, field_set, is_abstract) tuples."""
    try:
        text = path.read_text(encoding='utf-8', errors='ignore')
    except OSError:
        return []
    text = re.sub(r'//[^\n]*', '', text)
    text = re.sub(r'/\*.*?\*/', '', text, flags=re.DOTALL)
    results = []
    for m in CLASS_DECL_PAT.finditer(text):
        cname = m.group(1)
        # abstract is matched as part of the modifier group - check the entire declaration line
        is_abstract = 'abstract' in m.group(0)
        base_clause = (m.group(2) or '').strip()
        base = base_clause.split(',')[0].split('<')[0].strip() if base_clause else None
        body_start = text.find('{', m.end())
        if body_start < 0: continue
        depth = 1
        i = body_start + 1
        while i < len(text) and depth > 0:
            if text[i] == '{': depth += 1
            elif text[i] == '}': depth -= 1
            i += 1
        body = text[body_start:i]
        fields = set()
        for fm in SERIALIZED_FIELD_PAT.finditer(body):
            fname = fm.group(1)
            if fname[0].islower() or fname.startswith('m_'):
                fields.add(fname)
        results.append((cname, base, fields, is_abstract))
    return results


def build_pkg_class_index():
    """Scan PackageCache for class definitions, skipping test/editor folders.
       Returns {class_name: (guid, base_or_None, field_set, is_abstract)}.
    """
    classes = {}
    meta_for_cs = {}
    if not PKG_DIR.is_dir():
        sys.exit(f"ERROR: Library/PackageCache not found at {PKG_DIR}")
    # filter out test/editor folders - we want runtime classes only
    SKIP_PARTS = ('Tests', 'Editor', 'Samples', 'Documentation~', 'EditorTests')
    def is_skipped(p):
        return any(part in SKIP_PARTS for part in p.parts)
    for meta_path in PKG_DIR.rglob('*.cs.meta'):
        if is_skipped(meta_path): continue
        try:
            text = meta_path.read_text(encoding='utf-8', errors='ignore')
        except OSError:
            continue
        gm = re.search(r'^guid:\s*([a-f0-9]{32})', text, re.MULTILINE)
        if gm:
            meta_for_cs[meta_path.with_suffix('')] = gm.group(1)
    for cs_path in PKG_DIR.rglob('*.cs'):
        if is_skipped(cs_path): continue
        guid = meta_for_cs.get(cs_path)
        if not guid: continue
        for cname, base, fields, is_abstract in collect_class_fields_in_file(cs_path):
            if cname not in classes:
                classes[cname] = (guid, base, fields, is_abstract)
    return classes


def resolve_inherited_fields(class_name, classes, _seen=None):
    """Walk base chain to accumulate field set."""
    if _seen is None: _seen = set()
    if class_name in _seen or class_name not in classes:
        return set()
    _seen.add(class_name)
    _, base, fields, _ = classes[class_name]
    total = set(fields)
    if base:
        base_simple = base.split('<')[0].strip()
        total |= resolve_inherited_fields(base_simple, classes, _seen)
    return total


def collect_asset_script_refs():
    """Walk Assets/ and return:
       refs: {(guid, fileID): {(file_path, line_no): [following_field_names]}}
    """
    refs = defaultdict(dict)
    for path in ASSETS_DIR.rglob('*'):
        if path.suffix.lower() not in YAML_EXTS: continue
        try:
            text = path.read_text(encoding='utf-8', errors='ignore')
        except OSError:
            continue
        lines = text.splitlines()
        for i, line in enumerate(lines):
            m = SCRIPT_REF_PAT.search(line)
            if not m: continue
            fid = int(m.group(1))
            guid = m.group(2)
            # Capture all top-level fields in this MonoBehaviour block until next --- !u! block.
            # Look at indent of first content line to identify "top-level" within the block.
            fields_after = []
            block_indent = None
            for j in range(i+1, len(lines)):
                l = lines[j]
                stripped = l.strip()
                if stripped.startswith('--- !u!') or stripped.startswith('MonoBehaviour:'): break
                indent = len(l) - len(l.lstrip())
                if block_indent is None and stripped:
                    block_indent = indent
                # only count top-level fields
                if indent == block_indent:
                    fmm = re.match(r'(m_\w+|[a-z]\w+)\s*:', stripped)
                    if fmm:
                        name = fmm.group(1)
                        if name not in ('m_Name','m_EditorClassIdentifier'):
                            fields_after.append(name)
            refs[(guid, fid)][(str(path), i)] = fields_after
    return refs


def is_mono_or_so_descendant(class_name, classes, _seen=None):
    """Check if a class derives from MonoBehaviour or ScriptableObject."""
    if _seen is None: _seen = set()
    if class_name in _seen: return False
    _seen.add(class_name)
    if class_name in ('MonoBehaviour', 'ScriptableObject'):
        return True
    if class_name not in classes: return False
    _, base, _, _ = classes[class_name]
    if base is None: return False
    base_simple = base.split('<')[0].strip()
    return is_mono_or_so_descendant(base_simple, classes, _seen)


def best_class_match(field_signature, classes, mono_classes):
    """Find non-abstract Mono/SO class with maximum overlap.
       Tiebreaker prefers subclasses whose OWN fields explain the signature
       (vs inherited fields shared by a base). Returns (class_name, score, overlap)."""
    sig = set(field_signature)
    if not sig: return (None, 0, 0)
    candidates = []
    for cname in mono_classes:
        if cname not in classes: continue
        _, _, own, is_abstract = classes[cname]
        if is_abstract: continue
        total_fields = resolve_inherited_fields(cname, classes)
        if not total_fields: continue
        overlap = len(sig & total_fields)
        if overlap == 0: continue
        own_overlap = len(sig & own)
        union = len(sig | total_fields)
        score = overlap / union
        candidates.append((cname, overlap, own_overlap, score))
    if not candidates: return (None, 0, 0)
    # Sort: prefer (overlap, own_overlap, score) — all descending
    candidates.sort(key=lambda x: (-x[1], -x[2], -x[3]))
    best = candidates[0]
    return (best[0], best[3], best[1])


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--write', action='store_true', help='apply changes (else dry-run)')
    ap.add_argument('--missing-only', action='store_true',
                    help='only remap (guid,fileID) pairs whose guid is missing from any .meta')
    args = ap.parse_args()

    print('Building package class index...')
    classes = build_pkg_class_index()
    print(f'  found {len(classes)} unique class definitions in PackageCache')
    mono_classes = {c for c in classes if is_mono_or_so_descendant(c, classes)}
    print(f'  of which {len(mono_classes)} are MonoBehaviour/ScriptableObject descendants')

    print('Scanning Assets/ for m_Script references...')
    refs = collect_asset_script_refs()
    print(f'  found {sum(len(occs) for occs in refs.values())} m_Script occurrences')
    print(f'  ({len(refs)} distinct (guid, fileID) pairs)')

    # Determine which guids are "missing" (not in any .meta of project + packages)
    known = set()
    for meta in list(ASSETS_DIR.rglob('*.meta')) + list(PKG_DIR.rglob('*.meta')):
        try:
            t = meta.read_text(encoding='utf-8', errors='ignore')
        except OSError: continue
        gm = re.search(r'^guid:\s*([a-f0-9]{32})', t, re.MULTILINE)
        if gm: known.add(gm.group(1))

    # Build remap
    remap = {}  # (old_guid, old_fid) -> (new_guid, 11500000, class_name, score)
    unresolved_targets = []  # debugging
    for (guid, fid), occs in refs.items():
        if guid in known and not args.write and not args.missing_only:
            # Skip already-resolvable guid groups silently
            pass
        if args.missing_only and guid in known:
            continue
        if guid == '0' * 31 + 'e' + '0' * 16 or guid == '0' * 32:
            continue  # sentinel
        # Use union of all observed fingerprints
        sig = set()
        for fields in occs.values():
            sig.update(fields)
        # Manual pin takes precedence over fingerprint matching
        if (guid, fid) in MANUAL_REMAP:
            cname = MANUAL_REMAP[(guid, fid)]
            new_guid = resolve_class_guid_by_filename(cname)
            if new_guid:
                remap[(guid, fid)] = (new_guid, 11500000, cname, 1.0, -1)
                continue
        cname, score, overlap = best_class_match(sig, classes, mono_classes)
        # Confidence rule: relaxed since we now filter to concrete Mono/SO classes only
        #   - overlap >= 2 with high coverage (small but specific classes like ContentSizeFitter)
        #   - OR overlap >= 4 (substantial absolute match)
        sig_coverage = overlap / max(len(sig), 1)
        confident = (overlap >= 2 and sig_coverage >= 0.6) or overlap >= 4
        if cname is None or not confident:
            unresolved_targets.append((guid, fid, sig, cname, score, overlap))
            continue
        new_guid = classes[cname][0]
        remap[(guid, fid)] = (new_guid, 11500000, cname, score, overlap)

    if not remap:
        print('Nothing to remap.')
        return

    # Sort remap for stable output
    print(f'\nProposed remap ({len(remap)} (guid,fileID) pairs):')
    by_guid = defaultdict(list)
    for (og, of), v in remap.items():
        by_guid[og].append((of, v))
    for og, items in sorted(by_guid.items()):
        print(f"\n  old guid {og}:")
        for of, (ng, nfid, cname, score, ov) in sorted(items):
            print(f"    fileID {of:>12} -> {cname:<35} new guid {ng[:8]}.. (overlap={ov} score={score:.2f})")

    if unresolved_targets:
        print(f"\n  {len(unresolved_targets)} pair(s) could not be matched confidently:")
        for guid, fid, sig, cname, score, ov in unresolved_targets[:20]:
            sig_str = ', '.join(sorted(sig)[:5]) or '(no fields)'
            best_str = f"best={cname}/ov{ov}/sc{score:.2f}" if cname else 'best=(none)'
            print(f"    guid {guid[:8]}.. fileID {fid:>12}  fingerprint: {sig_str}  {best_str}")

    if not args.write:
        print('\n(dry run — re-run with --write to apply)')
        return

    # Apply remap. We need a fileID-aware substitution: only replace m_Script: {fileID: X, guid: Y, type: 3}
    # entries where (Y, X) is in remap.
    print('\nApplying remap...')
    files_changed = 0
    total_subs = 0
    for path in ASSETS_DIR.rglob('*'):
        if path.suffix.lower() not in YAML_EXTS: continue
        try:
            text = path.read_text(encoding='utf-8', errors='ignore')
        except OSError:
            continue
        new_text = text
        local_count = 0

        def sub(m):
            nonlocal local_count
            fid = int(m.group(1))
            guid = m.group(2)
            key = (guid, fid)
            if key in remap:
                ng, nfid, _, _, _ = remap[key]
                local_count += 1
                return f'm_Script: {{fileID: {nfid}, guid: {ng}, type: 3}}'
            return m.group(0)

        new_text = SCRIPT_REF_PAT.sub(sub, text)
        if new_text != text:
            path.write_text(new_text, encoding='utf-8', newline='\n')
            files_changed += 1
            total_subs += local_count
            print(f"  {local_count:>3}  {path.relative_to(REPO_ROOT)}")

    print(f"\nDone. {files_changed} files, {total_subs} substitutions.")


if __name__ == '__main__':
    main()
