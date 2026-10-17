# Placeholders

Every placeholder asset in the project and what the final asset must be (`CLAUDE.md` §5.3). A row is added in the same commit as the placeholder it describes. Placeholder files are named `PH_<Purpose>` and live under `Assets/_Project/Placeholders/`.

| ID | Placeholder | Used by | Final asset needed (format, size, count, style notes) | Status |
|---|---|---|---|---|
| PH-001 | `PH_ShadowDecal` (64 px grey square, soft edge, baked) + `.mat` (URP Unlit transparent, queue 3000) | `P_ShadowArea`, `P_VoidShadow` (S3, §7.1) | Tileable soft-edged grey shadow decal, 256 px PNG with alpha, slightly noisy edge so shadows read as cast shade, not tiles | Open |
| PH-002 | `PH_EntityBlob` (64 px black radial blob, baked) + `.mat` (queue 3004) | `Player/Blob` (S4, §5.1) | Animated pure-black blob: 256 px PNG or a small flipbook (4-8 frames) with a wispy edge; must stay pure black against grey shadows | Open |
| PH-003 | `PH_Teardrop` material (black unlit) on Unity's built-in sphere, scaled 0.35 × 0.35 × 0.7 | `Player/Teardrop` (S4, §5.1) | Black teardrop mesh, under 500 tris, pointed tail along +Z | Open |
| PH-004 | `PH_Outline` material (white unlit, tinted by the LineRenderer) | `Player/TargetOutline` (S4, §12) | Fine as a material; final look may add a dashed or glowing line texture | Open |
| PH-005 | `PH_ViewCone` fan mesh (baked from T-VIEW-ANGLE / T-VIEW-RANGE) + `.mat` (white, alpha 0.12, queue 3002) | `P_Guard/ViewCone` (S8, §8.3 MUST) | Fine as a mesh; final look may add a radial fade texture so the cone edge reads softer. Rebake after changing either tunable | Open |
| PH-006 | Unity capsule (default material) as the guard body | `P_Guard/Visual` (S7, §8.1) | Pack #5 Low Poly SWAT body as a child visual through `AssetRegistry`, Humanoid rig driven by `Art/Guard.controller` (B10) | Open |
| PH-007 | Red unlit sphere `PH_AlertIcon.mat` above the guard's head | `P_Guard/AlertIcon` (S10, §12 "!" over the guard) | Camera-facing "!" icon sprite, 128 px PNG, red/orange, readable at 15 m | Open |
| PH-008 | Small Unity sphere + orange point light | `P_ThrownLantern/Body` (S10, §7.9) | The guard lantern model (pack #10 hand-held lantern or the CC0 Low poly Lantern OBJ) as the thrown visual | Open |

## Expected placeholders (not created yet)

The implementation spec (`docs/implementation/nullflame-implementation.md` §23, §25.2) expects these gaps. They get real rows above only when created.

- `PH_OilPipe`: wall pipe for `CrackablePipe` (S16). No free pipe pack found.
- `PH_LeverHandle`: breaker lever handle on the #1 panel. No free lever found.
- `PH_Locker`: Barracks lockers. No pack confirmed.
- `PH_OilDrip`: oil drip particle system using a Kenney Particle Pack sprite tinted black.
- `PH_OilDrop`, `PH_EssenceGradient`, `PH_AlertIcon`: baked by `DecalBaker` in later steps.
- `PH_Tone_<Event>`: only for sound events with no fitting clip in the approved sources.
- Any pack prop that turns out to be missing after import (cart, cabinet, pedestal, gate panels, Final Gate, stopwatch, guard body), per the gap fallbacks in `docs/assets/free-assets.md` §2.
