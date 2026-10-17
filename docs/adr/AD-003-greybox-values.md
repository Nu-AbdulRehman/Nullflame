# AD-003: Greybox values not in design doc §4

**Date:** 2026-10-17 · **Closes:** OD-001 · **Decided by:** owner

## Decision
Build IMPL Appendix A.1 and A.2 as written: 0.2 m walls centred on the area lines, the listed door widths, prop sizes and positions and static shadow rectangles, no ceiling geometry, invisible blockers only in D-SCP-A..D.

## Rejected alternatives
Owner edits Appendix A first: rejected to keep B1 moving; values stay tunable in `FacilityLayout`. Ceilings at 4 m: rejected because the §11 camera sits above the wall tops and a ceiling would hide the floor.

## Consequences
Appendix A.1 and A.2 are approved values.
