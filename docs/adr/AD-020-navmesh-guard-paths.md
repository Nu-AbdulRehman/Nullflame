# AD-020: Guard routes are threaded through a baked NavMesh

**Date:** 2026-10-17 · **Overrides:** design doc §8.1 `[Default]` "waypoint lerp, no NavMesh" · **Decided by:** owner ("solve this by using a NavMesh, and adding all walls, obstacles, items etc to it as obstacles. do not add the player in it").

## Decision
- `Nullflame/Bake NavMesh` (`Scripts/Editor/NavMeshBaker.cs`) puts a `NavMeshSurface` on `Level_Generated` (collect Children, render meshes, layers Default and LevelSolid) and bakes it to `Scenes/Facility/NavMesh-Level_Generated.asset`. Walls, doors, props and floors from the builder are the obstacles. Nothing under `Shared`, the state roots or the Player is collected, so the entity never touches it and the Semi-Final Gate doorway stays walkable. Re-run after `Build Facility`.
- `NavRoute.Expand` runs once per guard in `GuardController.Build`: each leg between two waypoints is replaced by the NavMesh path's corners (`NavMesh.CalculatePath`, default Humanoid agent, 0.5 m radius). A leg with no complete path stays straight.
- Movement is still `GuardMotion`'s deterministic lerp over the expanded list, so reverse (history retrace), the intro skip (`Tick` fast-forward) and every existing test keep working. No `NavMeshAgent` is used.

## Rejected alternatives
`NavMeshAgent` on every guard: steering and avoidance are frame-rate dependent, so the intro skip can't fast-forward it, reverse would retrace a path that differs run to run, and the Beat 2 window simulation would stop matching the scene. Moving props and waypoints by hand: the owner reported clipping after a read-only check found no route crossing a prop (EN-050), so a hand fix had nothing to aim at.

## Consequences
Design doc §8.1 needs the owner's edit (NavMesh paths over waypoint lerp). The `com.unity.ai.navigation` package (2.0.10) was already in the project; no package change. The NavMesh asset must be re-baked whenever the facility layout or the prop list changes; the baker is idempotent.
