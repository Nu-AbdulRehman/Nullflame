# AD-005: New T-* rows

**Date:** 2026-10-17 · **Closes:** OD-008 · **Decided by:** owner

## Decision
Add every row in IMPL §3.2 with the listed values, including the new rows T-PLY-SLIDE 2.5 m/s, T-VIEW-EYE 1.6 m, T-GATE-SLIDE 0.5 s, T-INTRO-CAM 1.5 s, T-CAM-SENS 0.12, T-CAM-DAMP 0.12 s, T-CAM-RADIUS 0.3 m, and the `T-SPN-*` spawner structs.

## Rejected alternatives
Same IDs with different values: not chosen. Spawner numbers as Inspector fields: rejected because it breaks the rule that every number lives in `GameTuning`.

## Consequences
Design doc §13 needs these rows added (owner edit).
