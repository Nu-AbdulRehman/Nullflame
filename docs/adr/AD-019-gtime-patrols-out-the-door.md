# AD-019: G-TIME patrols in and out of the Time SCP Room

**Date:** 2026-10-17 · **Supersedes:** AD-017 · **Decided by:** owner ("make it patrol around the room, inside and out, so that when it goes outside the door for a bit we can consume").

## Decision
G-TIME's post loop (`State_Meltdown/Routes/R_GTime_Post`) is now: (4, 5) → (6.25, 5) → (6.25, 1) → (1.25, 1) → (1.25, 5) → (3.5, 5) → out through D-SCP-T to (3.75, 9) in the Access Corridor, 3 s dwell facing north (look point (3.75, 12)) → back in to (4, 5). The AD-017 corner stop facing the north wall is removed (dwell 0). Loop ≈ 25.5 m at 2.4 m/s plus 3 s ≈ 13.6 s. The Beat 2 window is the walk out plus the dwell, with the guard's back to the pedestal (`Beat2Tests.TimeLoop_HasFourSecondWindow_Beat2` measures it).

## Rejected alternatives
Keep the corner stop and add the door leg: longer loop (about 17.6 s), two safe windows, slower Beat 2. Dwell outside at the door jamb (3.75, 6.5): the cone still reaches the cabinet-to-pedestal path on the way back in. Looping the other way round (anticlockwise): the walk back in faces the pedestal for longer.

## Consequences
Design doc §9 Beat 2 ("loops around the pedestal", "3 s corner stop") and Appendix A.4 need the owner's edit. G-TIME's Beat 3 reverse history now includes the door leg; the reverse retraces it. The outside dwell point is 4.4 m from the Access Corridor alarm light at (8, 8); G-TIME takes no alarm damage there (guards don't take damage anyway) and the player riding it would be outside the 2 m alarm circle.
