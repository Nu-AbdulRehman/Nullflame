# AD-027: G-TIME spawns beside the sleeping-quarters door

**Date:** 2026-10-18 · **Overrides:** design doc §9.1 ("Barracks (by the lantern rack)"), Appendix A.4 G-TIME spawn (29.5, 38.5) · **Decided by:** owner ("When we rewind the barracks guard back to their original position, it overlaps too much with the one guarding the lever there. Make it so the guard rewinds it back to near the sleeping quarters door, but not exactly in front of it").

## Decision
G-TIME's spawn and first travel waypoint move to (30, 38.9): 2.5 m west-north-west of D-SLEEP (32, 37.4), off the SP-SLEEP-M path by 1.7 m, 4.4 m from G-BARRACKS's nearest pacing point (was 3.5 m), and 5 m from the lever shadow's edge, inside T-JMP-RANGE for the Beat 3 jump. The rest of the travel route is unchanged, so Beat 0/1 timings move by under 0.3 s.

## Consequences
Design doc §9.1 and Appendix A.4 need the owner's edit. The Beat 3 reverse (history retrace) ends at the new spawn.
