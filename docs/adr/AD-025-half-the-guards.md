# AD-025: About half as many guards

**Date:** 2026-10-17 · **Overrides:** design doc §9.1 wave table, §13 T-SPN-* rows, Appendix A.4/A.5 · **Decided by:** owner ("Reduce the number of guards, to around half as many as currently are there").

## Decision
- Meltdown named guards 7 → 4: `G-B`, `G-C`, `G-D` (the wave decoys) and their routes are deleted. The wave is G-GLANCE, G-TIME, then G-MAIN. `IntroSequence` keeps the empty wave slots, so G-TIME still leaves at 9 s and the Beat 0/1 timings are unchanged.
- Normal named guards 3 → 2: `G-ROUTINE-2` and its routes are deleted.
- Spawner caps halved: T-SPN-SLEEP-M 2 → 1, T-SPN-SLEEP-N 3 → 2 (one routine guard plus one spawned), T-SPN-SIDE-N 2 → 1, T-SPN-FINAL-N 2 → 1, T-SPN-FINAL-M 2 → 1. Intervals unchanged.

## Consequences
- Beat 5 has one Final Corridor patroller instead of two offset by half a loop; `PipeTests.Burst_AllCorridorGuardsThrow` now reads the count from T-SPN-FINAL-N.
- Beat 1 (AD-009): no wrong guards to ride by mistake.
- Design doc §9.1, §9 Beat 5 setup, §13 and Appendix A need the owner's edit.
