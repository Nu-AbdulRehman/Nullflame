# AD-017: G-TIME's Beat 2 corner stop faces north for 4 s

**Date:** 2026-10-17 · **Closes:** BLOCKED question in TASK-B3-015 · **Decided by:** owner

## Decision
G-TIME's post-loop stop at the north-east corner (6.25, 5) lasts 4 s and faces due north (look point (6.25, 7), the north wall), not the pedestal. Loop period is about 11.5 s at the Meltdown walk speed. `Beat2Tests.TimeLoop_HasFourSecondWindow_Beat2` measures the longest safe window (pedestal footprint and the cabinet-to-pedestal path outside the cone) at about 5.4 s.

## Rejected alternatives
Stop looking at the pedestal as §9 Beat 2 wrote it: the longest window is 1.52 s whatever the stop length, corner, loop direction or rectangle size (simulated, and confirmed by the PlayMode test), which is shorter than the 3 s pedestal consume, so Beat 2 would be unsolvable. Facing the Time Room door at (3.75, 6): also 1.52 s, because the cone then covers the cabinet-to-pedestal path. A 3 s stop facing north: 4.4 s window, too close to the minimum. A 5 s stop: 6.4 s window but a 12.5 s loop.

## Consequences
Design doc §9 Beat 2 setup needs "looking at the pedestal" changed to "facing north (away from the pedestal)" and the stop to 4 s (owner edit). The question offered to the owner said "facing the Time Room door"; the values the owner chose (4 s, 5.4 s window) are the due-north case, which is what was built.
