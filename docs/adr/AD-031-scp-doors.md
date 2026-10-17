# AD-031: Closed SCP doors on every sealed opening

**Date:** 2026-10-18 · **Overrides:** design doc §4.2 (D-SCP-A..D "open in Meltdown"), Appendix A.1 show-only doors · **Decided by:** owner ("add closed doors with SCP logos on them to all the blocked off areas ... same texture / mats as the walls but a bit darker, with the SCP logo, and a bit inset from the walls").

## Decision
- New `OpeningKind.ScpDoor`. D-SCP-A, B, C, D use it: a closed door in both world states (the invisible blocker stays). D-SCP-T keeps `ContainmentDoor` (open in Meltdown, closed in Normal).
- `ShowOnlyDoor` openings (D-SLEEP-SHOW, D-SIDE-W, D-SIDE-E) now build the same door instead of the pack #3 gate.
- The door is built by `FacilityBuilder.ScpDoor`: a solid 0.12 m slab centred in the 0.2 m wall (0.04 m inside each face), `Art/M_ScpDoor.mat` (the wall kit material with its base colour at 55%, baked by `Nullflame/Bake Door Material`), and the SCP badge (`Art/M_ScpBadge.mat`) on both faces at 45% of the door width. Solid: blocks jumps, sight, floor steps and the guard NavMesh like a wall.

## Consequences
Design doc §4.2 and Appendix A.1 need the owner's edit. `AssetRegistry.showOnlyDoor` is no longer read. Re-run `Bake Door Material` after changing the wall kit, then `Build Facility` and `Bake NavMesh`.
