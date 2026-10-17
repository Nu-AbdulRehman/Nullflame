# AD-024: The Beat 1 "missed your chance" fail is removed

**Date:** 2026-10-17 · **Overrides:** design doc §9 Beat 1 fail state and the §12 "Missed" screen · **Decided by:** owner ("there shouldn't be a Missed Your Chance end, as the entity can go around on the dark floor").

## Decision
With floor movement (AD-023) the player can leave the Player Room on foot, so missing the G-TIME ride no longer strands them. `FlowState.Missed`, `GameFlow.CheckMissedChance`, the `waveGuards`, `layout` and `timePowers` fields on `GameFlow`, the `Canvas/MissedScreen` object and `FlowTests.AllWaveGuardsPosted_PlayerOutside_Missed_Beat1` are deleted. Death remains the only fail; T-FAIL-RESTART now covers death only.

## Consequences
Design doc §9 Beat 1 ("fail: the wave reaches its posts, Missed screen"), §12 Missed row and §14 need the owner's edit. EN-026 (the Beat 1 test boards at D-PLAYER) is unaffected.
