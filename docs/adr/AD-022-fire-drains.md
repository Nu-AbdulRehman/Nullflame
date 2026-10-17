# AD-022: Lantern fire drains 15 Essence per second

**Date:** 2026-10-17 · **Overrides:** design doc §7.9 rule 4 ("fire contact sets Essence to 0") and the §9 Beat 2 fail state wording · **Decided by:** owner ("Fire should drain, 15 per second").

## Decision
- New tunable **T-DMG-FIRE = 15 /s** (`GameTuning.fireDps`). A `LanternFire` volume is no longer an instant kill; it drains like a light while it covers the entity, in any shadow (an opaque shadow is cover from light, not from fire, AD-018).
- The Final Corridor wall fire (`WallFire`) stays an instant kill (§7.7); the owner did not mention it.
- Fire still destroys oil drops and void shadows it reaches (§7.9 rule 6), which removes the cover under the entity.
- One smash burst per landing: the pack #8 `SmallExplosion` loops, so `ThrownLantern.Smash` turns `loop` off on its particle systems before their first frame and destroys the object after 4 s.

## Consequences
- `ConsumeTests.Pedestal_InCone_LeadsToDeath_Beat2` is now `Pedestal_InCone_GetsBurned_Beat2`: being spotted mid-consume costs Essence rather than the run. Design doc §9 Beat 2 fail state and §13 need the owner's edit (add the T-DMG-FIRE row).
- `FlameTests.FireContact_Kills` is now `FireDrains_TDMGFIRE`.

## Rejected
Keep the instant kill and only fix the explosion: the owner asked for the drain in the same message.
