# AD-002: Physics layers

**Date:** 2026-10-17 · **Closes:** OD-007 · **Decided by:** owner

## Decision
Add user layers `LevelSolid` (6), `Entity` (7) and `Hazard` (8), and enable only `Entity` × `Hazard` in the collision matrix.

## Rejected alternatives
No new layers, with non-allocating raycasts filtered by a marker component: rejected as more code and slower queries.

## Consequences
TASK-B1-011 may change `TagManager` and the physics matrix through MCP for Unity.
