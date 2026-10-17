# AD-007: Facility in the build scene list

**Date:** 2026-10-17 · **Closes:** OD-014 · **Decided by:** owner

## Decision
Add `Assets/_Project/Scenes/Facility.unity` to the build scene list as scene 0, so `SceneManager.LoadScene` can reload it for restarts.

## Rejected alternatives
Reset the scene in place: rejected because `CLAUDE.md` §3 requires restart by scene reload and every system would need reset code.

## Consequences
TASK-B4-015 may change `EditorBuildSettings` through MCP for Unity.
