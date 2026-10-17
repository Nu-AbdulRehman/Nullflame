# AD-011: Consume detection, airborne flames, fire duration

**Date:** 2026-10-17 · **Closes:** OD-010 · **Decided by:** owner

## Decision
(1) Consume and rewind-channel detection use the §8.3 rule: view cone, line of sight and the 0.2 s grace. (2) Every `FlameVolume` ignores the entity while it is jumping. (3) Lantern fire grows for T-FIRE-GROW, burns at full radius for T-FIRE-LIFE, then shrinks over T-FIRE-SHRINK (9 s total by default).

## Rejected alternatives
Literal §7.3 detection without line of sight or grace: rejected as harsher and inconsistent. Only lantern fire ignoring airborne: rejected as more special cases. 6 s total including growth: rejected as a shorter fire than §7.9 describes.

## Consequences
Design doc §7.3 and §7.9 need a clarifying sentence each (owner edit).
