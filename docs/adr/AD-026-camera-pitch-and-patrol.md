# AD-026: Wider camera pitch; G-PATROL walks into the Barracks

**Date:** 2026-10-18 · **Overrides:** design doc §13 T-CAM-PITCH-MIN / T-CAM-PITCH-MAX, Appendix A.5 G-PATROL loop · **Decided by:** owner ("the clamping values feel too restrictive as I can't aim high enough"; "make the patrolling person from the SCP rooms access corridor patrol all the way to the barracks ... so there is someone's shadow to grab on to to get out of the barracks").

## Decision
- T-CAM-PITCH-MIN 25 → 5 degrees, T-CAM-PITCH-MAX 70 → 80 degrees. A flatter camera puts the reticle further away, so long jumps can be aimed; a steeper one shows the floor round the entity.
- G-PATROL's Normal post loop (`State_Normal/Routes/R_Patrol_Post`) is (19.5, 14) → (19.5, 38) → (19.5, 35) → through D-BARRACKS to (27, 35), 2 s dwell → (19.5, 35) → back to (19.5, 14). Its opaque shadow passes through the Barracks door once per loop, giving a ride out after the rewind.

## Consequences
Design doc §13 and Appendix A.5 need the owner's edit. The loop is about 14 s longer (16 m extra at 2 m/s plus the dwell). `PipeTests` still disables G-PATROL in its setup.
