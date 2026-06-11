#!/usr/bin/env python3
"""
fix_shaders.py - Replace AssetRipper's broken shader dumps with real shaders.

AssetRipper exports every shader referenced by the build into Assets/Shader/
as decompiled dumps that do not actually compile in the editor. Materials
referencing them render magenta ("missing" look), and dumps of internal
pipeline shaders (e.g. Hidden/Universal/CoreBlit) register the same shader
NAME as the real package shaders, causing runtime errors like
"invalid pass index 1 in DrawProcedural" and "BLIT_DECODE_HDR doesn't exist".

This script:
  1. Extracts the canonical TMP shaders (+ .cginc includes) from the TMP
     package's "TMP Essential Resources.unitypackage" straight onto disk at
     their canonical paths (Assets/TextMesh Pro/Shaders/...) with their
     canonical .meta GUIDs - no Editor import dialog, no GUID collisions.
  2. Indexes all real shaders by declared name: extracted TMP shaders +
     every .shader in Library/PackageCache.
  3. Indexes the dumps in Assets/Shader/ by declared name.
  4. Where a dump's name matches a real shader, rewrites every reference
     (materials, assets, scenes, prefabs) from the dump's GUID to the real
     shader's GUID.
  5. Moves remapped dumps (now unreferenced) to _removed_from_assets/Shader/.
     Dumps with no real counterpart are left in place and reported.

Usage:
  python fix_shaders.py            # dry run
  python fix_shaders.py --write    # apply
"""

import re
import sys
import tarfile
import argparse
import shutil
from pathlib import Path
from collections import defaultdict

REPO = Path(__file__).resolve().parent
ASSETS = REPO / 'Assets'
PKG_CACHE = REPO / 'Library' / 'PackageCache'
DUMP_DIR = ASSETS / 'Shader'
QUARANTINE = REPO / '_removed_from_assets' / 'Shader'

YAML_EXTS = {'.mat', '.asset', '.unity', '.prefab', '.preset'}

SHADER_NAME_PAT = re.compile(r'^\s*Shader\s+"([^"]+)"', re.MULTILINE)
GUID_PAT = re.compile(r'^guid:\s*([a-f0-9]{32})', re.MULTILINE)


def find_tmp_essentials():
    hits = list(PKG_CACHE.glob('com.unity.textmeshpro@*/Package Resources/TMP Essential Resources.unitypackage'))
    return hits[0] if hits else None


def extract_tmp_shaders(write):
    """Extract Assets/TextMesh Pro/Shaders/* from the essentials unitypackage.
       Returns {shader_name: guid} for extracted .shader files."""
    pkg = find_tmp_essentials()
    if pkg is None:
        print('  WARNING: TMP Essential Resources.unitypackage not found - skipping TMP extraction')
        return {}
    # unitypackage = tar.gz of <guid>/{asset, asset.meta, pathname}
    entries = {}  # guid -> {'pathname': str, 'asset': bytes, 'meta': bytes}
    with tarfile.open(pkg, 'r:gz') as tf:
        for member in tf.getmembers():
            parts = member.name.split('/')
            if len(parts) < 2: continue
            guid_dir, fname = parts[0], parts[-1]
            if fname not in ('pathname', 'asset', 'asset.meta'): continue
            f = tf.extractfile(member)
            if f is None: continue
            entries.setdefault(guid_dir, {})[fname] = f.read()

    extracted = {}
    count = 0
    for guid_dir, files in entries.items():
        pathname = files.get('pathname', b'').decode('utf-8', 'ignore').splitlines()
        if not pathname: continue
        rel = pathname[0].strip()
        if not rel.startswith('Assets/TextMesh Pro/Shaders/'): continue
        if 'asset' not in files: continue  # folder entries have no asset payload
        dest = REPO / rel
        meta_dest = REPO / (rel + '.meta')
        if write:
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_bytes(files['asset'])
            if 'asset.meta' in files:
                meta_dest.write_bytes(files['asset.meta'])
        count += 1
        if rel.endswith('.shader'):
            text = files['asset'].decode('utf-8', 'ignore')
            nm = SHADER_NAME_PAT.search(text)
            meta_text = files.get('asset.meta', b'').decode('utf-8', 'ignore')
            gm = GUID_PAT.search(meta_text)
            if nm and gm:
                extracted[nm.group(1)] = gm.group(1)
    print(f'  extracted {count} TMP shader files ({len(extracted)} .shader) '
          f'{"" if write else "[dry run - nothing written]"}')
    return extracted


def index_package_shaders():
    """{shader_name: guid} for every shader in PackageCache."""
    out = {}
    for shader in PKG_CACHE.rglob('*.shader'):
        meta = Path(str(shader) + '.meta')
        if not meta.exists(): continue
        try:
            nm = SHADER_NAME_PAT.search(shader.read_text(encoding='utf-8', errors='ignore'))
            gm = GUID_PAT.search(meta.read_text(encoding='utf-8', errors='ignore'))
        except OSError:
            continue
        if nm and gm:
            out.setdefault(nm.group(1), gm.group(1))
    return out


def index_dumps():
    """{dump_guid: (shader_name, path)} for Assets/Shader/*.shader."""
    out = {}
    if not DUMP_DIR.is_dir(): return out
    for shader in DUMP_DIR.glob('*.shader'):
        meta = Path(str(shader) + '.meta')
        if not meta.exists(): continue
        try:
            nm = SHADER_NAME_PAT.search(shader.read_text(encoding='utf-8', errors='ignore'))
            gm = GUID_PAT.search(meta.read_text(encoding='utf-8', errors='ignore'))
        except OSError:
            continue
        if nm and gm:
            out[gm.group(1)] = (nm.group(1), shader)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--write', action='store_true')
    args = ap.parse_args()

    print('1) Extracting canonical TMP shaders from essentials package...')
    tmp_shaders = extract_tmp_shaders(args.write)

    print('2) Indexing package shaders...')
    real = index_package_shaders()
    # extracted TMP shaders take precedence for their names
    real.update(tmp_shaders)
    print(f'   {len(real)} real shaders by name')

    print('3) Indexing dumps in Assets/Shader/ ...')
    dumps = index_dumps()
    print(f'   {len(dumps)} dump shaders')

    remap = {}      # dump_guid -> real_guid
    movable = []    # dump paths to quarantine
    unmatched = []
    for dguid, (name, path) in dumps.items():
        if name in real and real[name] != dguid:
            remap[dguid] = real[name]
            movable.append(path)
        else:
            unmatched.append((name, path))

    print(f'\n4) Remap plan: {len(remap)} dumps have a real counterpart')
    for dguid, rguid in sorted(remap.items()):
        name = dumps[dguid][0]
        print(f'   {dguid[:8]}.. -> {rguid[:8]}..  {name}')
    if unmatched:
        print(f'\n   {len(unmatched)} dumps have NO real counterpart (left untouched):')
        for name, path in sorted(unmatched):
            print(f'     {name}   ({path.name})')

    print('\n5) Rewriting references...')
    files_changed = 0
    total = 0
    for f in ASSETS.rglob('*'):
        if f.suffix.lower() not in YAML_EXTS: continue
        try:
            text = f.read_text(encoding='utf-8', errors='ignore')
        except OSError:
            continue
        new_text = text
        n = 0
        for dguid, rguid in remap.items():
            c = new_text.count(dguid)
            if c:
                new_text = new_text.replace(dguid, rguid)
                n += c
        if n:
            files_changed += 1
            total += n
            print(f'   {n:>3}  {f.relative_to(REPO)}')
            if args.write:
                f.write_text(new_text, encoding='utf-8', newline='\n')

    print(f'\n   {files_changed} files, {total} references rewritten'
          f'{"" if args.write else " [dry run]"}')

    print('\n6) Quarantining remapped dumps...')
    if args.write:
        QUARANTINE.mkdir(parents=True, exist_ok=True)
        for p in movable:
            for q in (p, Path(str(p) + '.meta')):
                if q.exists():
                    shutil.move(str(q), str(QUARANTINE / q.name))
        print(f'   moved {len(movable)} dump shaders (+metas) to {QUARANTINE.relative_to(REPO)}')
    else:
        print(f'   would move {len(movable)} dump shaders [dry run]')


if __name__ == '__main__':
    main()
