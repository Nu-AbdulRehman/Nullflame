# AD-018: An opaque shadow is cover from flame light, and the entity merges into it

Date: 2026-10-17. Decided by the owner in play-testing ("when in something's shadow (strong shadow) the player should not take damage"). Fire is kept lethal by the agent, pending the owner's confirmation (see Open point).

## Decision

1. While the entity is in an **Opaque** `ShadowArea`, flame *light* does nothing to it: guard lanterns (T-DMG-LANTERN) and alarm lights (T-DMG-ALARM) no longer drain. Soft shadows are unchanged.
2. Fire still burns in an Opaque shadow: a thrown lantern's fire and the Final Corridor wall fire remain instant kills (§7.9 rule 4, §7.7), and fire still destroys oil drops and void shadows (§7.9 rule 6).
3. In an Opaque shadow the entity's blob is not drawn; instead every cast shadow goes darker (ambient light drops to 30%). The blob returns while consuming, where it spreads over the target, and in Soft shadows.

## Open point for the owner

The literal reading ("no damage") would also make fire harmless in Opaque shadows. That removes the Beat 2 fail state (consume in the cone, get spotted, burn: `ConsumeTests.Pedestal_InCone_LeadsToDeath_Beat2`) and makes a lantern throw useless against anyone in cover, so the agent kept fire lethal. Say "fire too" to extend the cover to fire; the test above then changes to assert survival.

## Overrides

- Design doc §2 "Flame" row ("damages or kills you, even inside a shadow") and §7.7 guard lantern row ("even opaque shadows"): lantern and alarm drain now applies only in Soft shadows. Needs the owner's edit.
- The Beat 1 ride and the Beat 3 Essence budget (AD-010) get cheaper: a ridden guard's shadow is Opaque, so other lanterns and alarm lights no longer drain the rider.

## Rejected

- Keep damage in Opaque shadows and only hide the blob: the owner wants a strong shadow to be real cover.
- Full immunity including fire: see the open point.
- Darkening only the owner's own cast shadow: URP has no way to deepen one shadow-map region without a Decal renderer feature (a pipeline change, CLAUDE.md §3); dropping ambient darkens every cast shadow and needs nothing new.
