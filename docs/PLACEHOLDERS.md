# Placeholders

Every placeholder asset in the project and what the final asset must be (`CLAUDE.md` §5.3). A row is added in the same commit as the placeholder it describes. Placeholder files are named `PH_<Purpose>` and live under `Assets/_Project/Placeholders/`.

| ID | Placeholder | Used by | Final asset needed (format, size, count, style notes) | Status |
|---|---|---|---|---|

## Expected placeholders (not created yet)

The implementation spec (`docs/implementation/nullflame-implementation.md` §23, §25.2) expects these gaps. They get real rows above only when created.

- `PH_OilPipe`: wall pipe for `CrackablePipe` (S16). No free pipe pack found.
- `PH_LeverHandle`: breaker lever handle on the #1 panel. No free lever found.
- `PH_Locker`: Barracks lockers. No pack confirmed.
- `PH_OilDrip`: oil drip particle system using a Kenney Particle Pack sprite tinted black.
- `PH_EntityBlob`, `PH_ShadowDecal`, `PH_OilDrop`, `PH_ViewCone`, `PH_Teardrop`, `PH_Outline`, `PH_Scanlines`, `PH_EssenceGradient`, `PH_AlertIcon`: baked by `DecalBaker`.
- `PH_Tone_<Event>`: only for sound events with no fitting clip in the approved sources.
- Any pack prop that turns out to be missing after import (cart, cabinet, pedestal, gate panels, Final Gate, stopwatch, guard body), per the gap fallbacks in `docs/assets/free-assets.md` §2.
