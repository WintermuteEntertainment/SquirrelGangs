# Squirrel Gangs 🐿️

**2D squirrel fighting with acorns and stash bombs.**

A local-multiplayer arena brawler for up to 4 players by **Wintermute Entertainment**. Squirrels battle across treetop and city arenas — throwing acorns, sprinting, digging, hiding, and planting explosive nut stashes to take each other out.

## Gameplay

- **Up to 4 players** (local) — keyboard/mouse and gamepads supported, drop-in via player select
- **Characters:** FatCat, Fitz, NinjaSquirrel, Nutmeg
- **Actions:** Move · Fire (throw acorns) · Jump · Sprint · Hide · Dig · Trigger Nut Bomb
- Collect and stash nuts around the arena — stashes can be rigged to explode
- HP / ammo / deaths tracked per player

## Built with

- **Unity 2022.3.13f1** (open with this exact version)
- Universal Render Pipeline 14 (2D renderer, 2D lights)
- New Input System 1.7.0 (4 player maps + UI map in `SquirrelGangs.inputactions` asset)
- TextMeshPro, Cinemachine, Tilemap

## Getting started

```bash
git lfs install          # binaries are LFS-tracked
git clone https://github.com/WintermuteEntertainment/SquirrelGangs.git
```

Open the folder in Unity Hub with **2022.3.13f1** (first import rebuilds `Library/`, takes a few minutes). Scenes live in `Assets/Scenes/` — start from `MainMenu`, or open `TreeLevel_2` to jump straight into the arena.

## Project notes — the recovery story

In mid-2026 the original project files were lost; the only surviving artifact was a built alpha (`SquirrelGs-09-18-2025`). This entire repository was **reconstructed from that build** using [AssetRipper](https://github.com/AssetRipper/AssetRipper) plus a set of custom repair scripts (kept in the repo root):

- `fix_script_refs.py` — remapped every scene/prefab script reference from the build's baked DLLs back to real package scripts (serialized-field fingerprint matching)
- `fix_shaders.py` — replaced non-compiling shader dumps with canonical TMP/URP package shaders
- Tile assets, input wiring, and UI event hookups were rebuilt from data the scenes themselves had cached

Decompiled script names/structure may look machine-translated in places — logic is faithful to the shipped alpha, cleanup is ongoing.

**Repo lineage:**

| Ref | What it is |
|---|---|
| `main` | Active development (recovered project) |
| `demo-build-archive` branch | The pre-loss repo content (demo build only — source was never pushed 😅) |
| `recovery-baseline` tag | Raw AssetRipper export, untouched |
| `v0.1.0-playable` tag | First fully-working editor state after recovery |
| [SquirrelGangs_recovered](https://github.com/WintermuteEntertainment/SquirrelGangs_recovered) | Frozen backup of the recovery work |

*Moral of the story: push your source, not just your builds.*
