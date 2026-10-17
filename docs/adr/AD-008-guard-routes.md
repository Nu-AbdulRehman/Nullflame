# AD-008: Guard routes

**Date:** 2026-10-17 · **Closes:** OD-002 · **Decided by:** owner

## Decision
Build IMPL Appendix A.4 and A.5 as written; tune waypoints in the Scene view after tests and play-throughs.

## Rejected alternatives
Owner revises routes before B3: rejected to keep B3 moving; routes are scene objects and easy to drag later.

## Consequences
`Beat2Tests.TimeLoop_HasFourSecondWindow` failing means the G-TIME loop needs tuning, not that the test is wrong.
