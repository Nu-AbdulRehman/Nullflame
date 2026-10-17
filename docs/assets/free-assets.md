# Nullflame: free assets list

Checked 2026-10-17. Every pack below showed a price of **$0** on its live store page. Nothing has been downloaded or installed.

**What was checked and what wasn't.** For each Asset Store page the price, render pipeline table, size, version and license were read. The pages' Description and Package Content sections did not come through, so most of the "covers" notes come from reviews, tags, publisher pages or mirrors. Open the Package Content tab before relying on a pack for a specific prop.

**Render pipeline.** The pipeline isn't pinned yet (CLAUDE.md §3 leaves it to the owner). **Use URP**, which is the Unity 6 default template. The fire VFX packs only support URP on Unity 6, and Built-in packs convert to URP with *Window → Rendering → Render Pipeline Converter*.

**License.** Asset Store items use the Standard Unity Asset Store EULA: free to ship in a game, but you can't redistribute the raw files, so keep them out of a public repo. Licenses for items from other sites are noted next to each one.

---

## 1. Recommended install set (10 items)

| # | Pack | Source | Pipeline on Unity 6 | Size | Covers |
|---|---|---|---|---|---|
| 1 | [Sci-Fi Styled Modular Pack](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913) (karboosx) | Asset Store, free | Built-in (2018). Convert to URP. | 8.6 MB | **Main facility kit.** 202 meshes and 153 prefabs: walls, floors, corridors, columns, animated doors, desks, tables, **beds**, machines, computers, **lights**, signs. All prefabs share one material, so the URP conversion is a single change. |
| 2 | [3D Free Modular Kit](https://assetstore.unity.com/packages/3d/environments/3d-free-modular-kit-85732) (Barking Dog) | Asset Store, free | Built-in, URP and HDRP on 6000.3 | 10.3 MB | Backup shell kit, the only free one that officially supports Unity 6 and URP. Reviews describe many prefabs and dark indoor lighting. |
| 3 | [Sci Fi Doors](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-doors-162876) (MASH Virtual) | Asset Store, free | Not stated (2019.4). Convert. | 12.4 MB | Containment doors, and the Semi-Final and Final Gates (scale one up). Whether the doors are animated is unconfirmed. |
| 4 | [Free Laboratory Pack](https://assetstore.unity.com/packages/3d/props/tools/free-laboratory-pack-123782) (StormBringer) | Asset Store, free | Not stated (2017.4). Convert. | 4.9 MB | Lab benches and equipment (Lab is SHOULD). Contents unconfirmed. |
| 5 | [Low Poly Character Series 1 SWAT](https://assetstore.unity.com/packages/3d/characters/humanoids/low-poly-character-series-1-swat-404966) (Castle Bravo) | Asset Store, free | Built-in, URP, HDRP (6000.0) | 16.3 MB | **Guards.** Low-poly SWAT/security look, the closest theme fit. New pack with no reviews yet; check that the rig imports as Humanoid. |
| 6 | [Human Soldier Animations FREE](https://assetstore.unity.com/packages/3d/animations/human-soldier-animations-free-340690) (Kevin Iglesias) | Asset Store, free | All three (6000.0) | 17.3 MB | **Guard animations:** idle, military (alert) idle, 8-direction walk and run, **grenade throw** (works as the lantern throw), hit reactions, deaths. |
| 7 | [Starter Assets – ThirdPerson URP](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-urp-196526) (Unity) | Asset Store, free | URP | ~71–77 MB | Fallback guard body: a Humanoid mannequin with locomotion, and an orbit-camera reference rig. **Pulls in Cinemachine and the Input System**, which needs package approval (CLAUDE.md §3). Skip it if #5 works. |
| 8 | [Particle Pack](https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-particle-pack-127325) (Unity) | Asset Store, free | **URP only** | 160 MB | Explosions, fire, and probably smoke and sparks (contents not itemised). Use for the lantern smash burst and the pipe burst. |
| 9 | [Free Fire VFX – Unity URP](https://vefects.itch.io/free-fire-vfx-unity-urp) (Vefects) | itch.io, free (name your price). License not stated on the page. | **URP only** | 4.6 MB | **Lantern fire and wall fire lanterns.** 15 prefabs: Fire Small/Medium/Big and **Fire Floor** 01/02 (with and without smoke), plus looping fire sounds. Grow the fire disc by scaling Fire Floor from script. |
| 10 | [Modular Medieval Lanterns](https://marketplace.unity.com/packages/3d/environments/historic/modular-medieval-lanterns-85527) (Alexandr Voevodov) | Asset Store, free | Not stated (5.6). Convert. | 278.5 MB | Wall fire lanterns and rack spares, possibly also hand-held. The only free lantern on the store; contents unconfirmed. |

Two items aren't on the Asset Store and are added by hand (FBX/OBJ dropped into `Assets/`):

| Need | Item | License |
|---|---|---|
| Hand-held guard lantern (if #10 has no hand-held one) | [Low poly Lantern](https://opengameart.org/content/low-poly-lantern-game-asset) (PolygonDan, OpenGameArt), OBJ, 2.7k tris, untextured | CC0 |
| Time SCP stopwatch | [Vintage Pocket Watch](https://sketchfab.com/3d-models/vintage-pocket-watch-0d6052aae54e4c1c878a67b868672018) (Mune, Sketchfab), PBR, 23.8k tris | **CC BY 4.0: needs a credit line** on the win screen or in the credits |

There is no free stopwatch or pocket watch on the Asset Store; the free "Clock Free" pack is a wall clock.

---

## 2. Need → asset map

From `design/nullflame-design.md` §4, §7, §8 and §10.

| Need (design ref) | Asset | Gap / fallback |
|---|---|---|
| Rooms, corridors, walls, floors, ceilings (§4.1) | #1 Sci-Fi Styled Modular (or #2) | Keep the `FacilityLayout` builder; swap its cubes for kit prefabs through the registry. |
| Room/corridor doors, open frame (§4.2) | #1 animated doors, #3 | – |
| Containment doors D-SCP-T, A..D (§4.2) | #3 Sci Fi Doors | – |
| Semi-Final Gate, two-panel sliding (§7.8) | #3 (two doors side by side) | Primitive panels with a #1 wall material. |
| Final Gate (§9 Beat 5) | #3 scaled up | Primitive. |
| Equipment crates, Main Corridor (§4.3) | #1 machines/props | [Modular Sci-Fi Corridor](https://assetstore.unity.com/packages/3d/environments/sci-fi/modular-sci-fi-corridor-142811) (MagixBox, free, 299 MB) has 2 crates, but it's large for that. |
| Cart, Access Corridor | #1 props | Primitive. |
| Cabinet, Time SCP Room | #1 props | Primitive. |
| Pedestal, Time SCP Room | #1 columns/machines | Cylinder with a #1 material. |
| Bunks and lockers, Barracks | #1 **beds** | Lockers: no free pack confirmed. Use cubes with a #1 panel material. |
| Lantern rack (unlit spares) | #10 lanterns on a #1 shelf | – |
| **Breaker lever panel** (§9 Beat 3) | #1 machines/computers for the panel | **No free lever found.** Use a primitive handle on a #1 panel. |
| Lab benches (SHOULD) | #4 Free Laboratory Pack, #1 desks | – |
| **Oil pipes**, Final Corridor walls (§7.6) | – | **No free pipe pack found** (the pipe packs found all cost $5–15). Cylinders at 2.5 m with a metal material from #1. Easy to build and they split cleanly into crack points. |
| Ceiling lights (§7.7) | #1 lights | – |
| Alarm lights, Meltdown (§7.7) | #1 lights with a red emissive material and a rotating Spot Light | – |
| Wall fire lanterns, Final Corridor | #10 plus #9 Fire Small | – |
| Guards, several variants (§8.1) | #5 SWAT, tinted per guard; G-TIME gets the amber lantern | #7 mannequin, or [Low Poly Soldiers Demo](https://assetstore.unity.com/packages/3d/characters/low-poly-soldiers-demo-73611) (free, URP only, own animations, retargets poorly) |
| Guard animations: idle, walk, alert, throw (§8.2, §7.9) | #6 Human Soldier Animations FREE | [Human Basic Motions FREE](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271) for extra locomotion. For a look-around, use one [Mixamo](https://www.mixamo.com) clip (free with an Adobe ID, royalty-free in games, no credit needed) or a head-turn script. |
| Guard hand-held lantern | #10 or the CC0 OBJ, plus a Point Light and #9 Fire Small | – |
| Time SCP stopwatch (§9 Beat 2) | Sketchfab pocket watch (CC BY) | Cylinder plus a disc. |
| Lantern smash fire disc (§7.9) | #9 Fire Floor, scaled over `T-FIRE-GROW` | #8 |
| Pipe burst / oil fire (§7.6) | #9 Fire Floor Big plus #8 explosion | – |
| **Oil drip and oil drops** | – | **No free drip VFX found.** Build a small particle system with a [Kenney Particle Pack](https://kenney.nl/assets/particle-pack) sprite (CC0) tinted black. The drops stay as decals. |
| Glass smash on lantern impact | #8 small explosion | No free glass-shatter pack found. |
| Shadow entity blob, teardrop (§5.1) | Keep the baked decal | Optional: [Free Stylized Smoke Effects Pack](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/free-stylized-smoke-effects-pack-226406), recoloured black and alpha-blended, for a wispy edge. |
| Sounds: klaxon, alert sting, fire, pipe creak, footsteps, whoosh (§12) | [Sonniss GDC 2026 bundle](https://gdc.sonniss.com/) (free, royalty-free, no credit needed, 7.5 GB) as the library. [Footsteps – Essentials](https://assetstore.unity.com/packages/package/189879) (free) for footsteps. #9 for fire loops. | Klaxon not confirmed in a specific file; pick from Sonniss. [Kenney Sci-fi Sounds](https://kenney.nl/assets/sci-fi-sounds) (CC0) for whooshes. |

---

## 3. How to import

The Claude agent can't sign in to the Asset Store, so this part is manual, once, before the jam:

1. Sign in on each Asset Store link and click **Add to My Assets**.
2. In Unity 6: **Window → Package Manager → My Assets**, then download and import each one into the project.
3. Run **Window → Rendering → Render Pipeline Converter** (Built-in to URP, Material Upgrade) for #1, #3, #4 and #10.
4. Drop the two non-store files (lantern OBJ, pocket watch FBX) into `Assets/ThirdParty/` and add the CC BY credit for the watch.
5. Keep imported packs out of a public repo (EULA). A `.gitignore` entry for `Assets/ThirdParty/` or the packs' own folders works.

---

## 4. Packs checked and rejected

- **Paid:** Modular Sci-Fi Corridor Pack (Team Proton, $20), Sci-fi station modular (Mixall, $14.99), Sci-Fi Modular Interior PBR ($9.50), Futuristic Modular Corridor ($21.99), Scifi Station Bedroom ($9.99), Sci-Fi Crates and Barrels ($14), Sci-Fi Modular Pack 64442 ($50), every lantern on the store except #10 ($4.99–15), every pocket watch or stopwatch ($8–20), pipe packs ($5–15), Security Guards character pack ($14.90).
- **Gone:** Snaps Prototype Sci-Fi/Industrial is deprecated, and the "Guard" character pack (30629) is no longer sold.
- **Poor fit:** Industrial Set v2.0 (free, but exterior hangars and tanks), Industrial Props Pack 45195 (free, but from 2015 with unknown contents).
- **CC0 alternative to the whole environment set:** [Quaternius Modular Sci-Fi MegaKit](https://quaternius.itch.io/modular-sci-fi-megakit), with a free 190-model tier (walls, floors, doors, props). Not on the Asset Store, but it's the cleanest modular kit if #1 disappoints.
