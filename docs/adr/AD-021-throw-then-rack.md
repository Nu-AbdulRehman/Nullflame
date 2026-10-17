# AD-021: A guard stands through its throw, then fetches a new lantern from the Barracks rack

**Date:** 2026-10-17 · **Overrides:** design doc §7.9 / §8.2 lanternless rule (relight in place after T-LANTERN-RELIGHT) · **Decided by:** owner ("They should throw the actual lantern and it should shatter, and burst the floor into flames ... After this, the guard returns to the barracks and gets a new lantern.")

## Decision
1. On the throw the guard stays paused through the throw animation (`LanternAttack.throwAnimTime`, 1.2 s, look only), then resumes.
2. The thrown object is the hand lantern model (CC0 PolygonDan, glow material) on `P_ThrownLantern`; on landing it spawns the pack #8 `SmallExplosion` (`AssetRegistry.lanternSmashVfx`) and the fire as before.
3. Lanternless, the guard leaves its route by NavMesh path to the point 0.6 m south of the Barracks lantern rack (from `FacilityLayout.props`, kind `LanternRack`), waits `rackPickupTime` (1 s), relights, walks the same path back and resumes its route from where it left (`GuardMotion.Detour`). A travel detour is recorded in history, so Beat 3's reverse retraces it.
4. With no NavMesh path to the rack the old rule applies: relight in place after T-LANTERN-RELIGHT.

## Rejected alternatives
Relight in place (the design doc rule): the owner wants the trip. A `NavMeshAgent` for the errand: breaks the deterministic motion and the history retrace (AD-020). Despawning the guard and respawning at the rack: loses the ridden-guard case and the history.

## Consequences
Design doc §7.9 ("lanternless for T-LANTERN-RELIGHT") needs the owner's edit. A Final Corridor guard's trip to the Barracks is about 30 m each way (about 30 s round trip in Normal), which leaves the corridor unguarded longer than the 10 s rule did. T-LANTERN-RELIGHT stays as the fallback.

## Open
The owner wrote "from which we lose health". Fire contact is still an instant kill (§7.9 rule 4, AD-018). Say "fire drains instead" with a per-second value for a T-DMG-FIRE row.
