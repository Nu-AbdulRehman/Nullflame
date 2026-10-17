# AD-028: Soft shadows hide the entity and drain 2 per second

**Date:** 2026-10-18 · **Overrides:** design doc §2 rule 3 ("soft shadows don't hide you"), §7.1 opacity rule, §7.7 drain table, §9 Beat 5 ("hide in oil while a guard passes") · **Decided by:** owner ("keep a small amount of per tick damage ~2/s even after rewind in the weak shadows of the pipe, but not the strong shadows of the guards. Also, make it so the guards can't see us in the pipe's shadow, so the incentive for cracking the pipe becomes to stop the per tick damage").

## Decision
1. Any shadow is cover from flame light and from guard eyes. Guards see the entity only while it jumps, consumes, or stands on bare floor.
2. A **Soft** shadow (the pipe strips) drains **T-DMG-SOFT = 2 /s** (`GameTuning.softShadowDps`) all the time, in both world states. An Opaque shadow (guards, props, oil drops, voids, the Player Room) drains nothing. Cracking a pipe for oil drops is how the drain is stopped.
3. Fire still drains T-DMG-FIRE in any shadow; bare floor still drains T-DMG-FLOOR under light.
4. T-DMG-LANTERN and T-DMG-ALARM no longer apply anywhere (light either reaches bare floor at T-DMG-FLOOR, or is blocked by a shadow); the fields are removed from `GameTuning`.

## Consequences
- Design doc §2, §7.1, §7.7, §9 Beat 5 and §13 need the owner's edit (remove T-DMG-LANTERN/ALARM, add T-DMG-SOFT).
- `VisionTests.SoftShadow_Spotted_AfterGrace_TVIEWGRACE` becomes `SoftShadow_NotSpotted` plus `Floor_Spotted_AfterGrace_TVIEWGRACE`; the lantern/alarm drain tests become floor tests; `FlameTests.SoftShadow_Drains_TDMGSOFT` is new.
