# AD-023: The entity can walk on bare floor outside the Final Corridor

**Date:** 2026-10-17 · **Overrides:** design doc §2 rule 1 ("can only exist inside shadows"), §3 / §5.2 (W/A/S/D slides within the current shadow, clamped to its bounds) · **Decided by:** owner ("the entity can also move around on the floor in all areas other than the final corridor, as they are all quite dark, but when we are just on the floor and not in another entity's shadow we take a lot of damage per tick if we get into the light of the lanterns").

## Decision
1. W/A/S/D moves the entity freely. Leaving a shadow's footprint puts it **on the floor** (`ShadowEntity.OnFloor`: `InShadow` with no `Current`). Walking into a shadow's footprint attaches to it; a moving shadow carries it as before.
2. Floor is forbidden where `FacilityLayout.noFloorAreas` says so (default: `SemiFinalGate`, `FinalCorridor`, `WestSideCorridor`, `EastSideCorridor`, `FinalGate`) and in every area with `playerAccess` false (SCP rooms A to D, the sleeping alcove). A move that would end on forbidden floor is clamped inside the current shadow instead.
3. Walls and props block floor movement through the guard NavMesh (`NavMesh.SamplePosition`, 0.3 m), so the entity keeps the agent's half-metre from walls and never crosses a prop. No physics change.
4. On the floor, lantern and alarm light drain **T-DMG-FLOOR = 40 /s** (`GameTuning.floorLightDps`), still blocked by walls (EN-048). Fire drains T-DMG-FIRE as anywhere. Guards see the entity on the floor as they see it in a soft shadow.
5. Jumps, consumes, rides, the void after a consume and the pipe strips are unchanged. A shadow that vanishes under the entity still leaves a void (§7.5).

## Open for the owner
T-DMG-FLOOR = 40 is the agent's placeholder for "a lot" (2.5 s from 100 to 0 under one lantern). Give the number you want.

## Rejected
Physics collisions between the entity and `LevelSolid` (a collision-matrix change, and the entity would still slide through props, which have no colliders). A separate `OnFloor` entity state (every system that reads `InShadow` would need a second case; `Current == null` carries the same meaning).

## Consequences
Design doc §2, §3, §5.2 and §13 (T-DMG-FLOOR row) need the owner's edit. The "move" hint now says floor is allowed but burns under lanterns. `JumpTests.Slide_ClampedToBounds_TPLYSIZE` moves to a pipe strip, where clamping still applies.
