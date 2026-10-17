# Asset manifest

Two lists (`CLAUDE.md` §4.1, §5.4): every builder or baker script and the content it constructs, and every third-party asset the game uses with its source and license. Rows are added in the same commit that adds the script or first uses the asset.

## Constructed content

| Script | Menu item | Builds | Owned root / output folder | Rebuild source |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Editor/FacilityBuilder.cs` | `Nullflame/Build Facility` | Floors, walls with door cuts, closed containment doors (Normal), show-only doors, invisible SCP doorway blockers, props, ceiling lights; Meltdown alarm lights (spot light + floor-circle FlameVolume) and Final Corridor wall fire lanterns (lights, lantern and flame visuals, one instant-kill box). Kit visuals from `AssetRegistry` slots, cubes when a slot is empty | `Level_Generated`, `State_Meltdown/Generated_Meltdown`, `State_Normal/Generated_Normal` | `Settings/FacilityLayout.asset`, `Settings/GameTuning.asset` (wall height), `Art/AssetRegistry.asset` |
| `Assets/_Project/Scripts/Editor/DecalBaker.cs` | `Nullflame/Bake Decals` | `PH_ShadowDecal.png/.mat`, `PH_EntityBlob.png/.mat`, `PH_Outline.mat`, `PH_Teardrop.mat`, `PH_ViewCone.asset/.mat`, `PH_Ring.png` (single sprite), `PH_OilDecal.png/.mat` (oil drop, queue 3001) (cone mesh from T-VIEW-ANGLE and T-VIEW-RANGE; URP Unlit transparent materials, render queues from IMPL §2.8) | `Assets/_Project/Placeholders/` | Code constants in `DecalBaker`; `Settings/GameTuning.asset` for the cone |
| `Assets/_Project/Scripts/Editor/DecalBaker.cs` | `Nullflame/Bake Badge Material` | `Art/M_ScpBadge.mat`: URP Unlit, alpha blended, `Art/scp_logo.png` | `Assets/_Project/Art/` | `Art/scp_logo.png` |
| `Assets/_Project/Scripts/Editor/DecalBaker.cs` | `Nullflame/Bake Door Material` | `Art/M_ScpDoor.mat`: the `wallSegment` kit material copied, base colour at 55% (AD-031) | `Assets/_Project/Art/` | `Art/AssetRegistry.asset` (`wallSegment`) |
| `Assets/_Project/Scripts/Editor/PipeBuilder.cs` | `Nullflame/Build Pipes` | One `P_CrackablePipe` per Final Corridor crack point (8): soft strip sized from the layout, three drop anchors `T-PIPE-DROP-SPACING` apart, burst point on the wall face, drip and `AssetRegistry.oilPipe` visual 2.5 m up on the wall | `Shared/Pipes_Generated` | `Settings/FacilityLayout.asset` (`crackPoints`, `pipeHeight`, `pipeStripWidth`), `Settings/GameTuning.asset`, `Art/AssetRegistry.asset` |
| `Assets/_Project/Scripts/Editor/NavMeshBaker.cs` | `Nullflame/Bake NavMesh` | `NavMeshSurface` on `Level_Generated` (children, render meshes, Default + LevelSolid) baked to `Scenes/Facility/NavMesh-Level_Generated.asset`; guards thread their routes through it (AD-020) | `Level_Generated` (component), `Scenes/Facility/` | `Level_Generated` contents (run after `Build Facility`) |

Planned (from `docs/implementation/nullflame-implementation.md` §25): `PipeBuilder` (`Nullflame/Build Pipes`), `ToneBaker` (`Nullflame/Bake Placeholder Tones`).

## Third-party assets

| AssetRegistry slot | Asset | Pack | Source | License | Credit required |
|---|---|---|---|---|---|
| `lanternFireVfx` | `MediumFlames.prefab` | #8 Particle Pack (Unity) | https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-particle-pack-127325 | Standard Unity Asset Store EULA | No |
| `fireSmallVfx` | `TinyFlames.prefab` | #8 Particle Pack (Unity) | same as above | Standard Unity Asset Store EULA | No |
| `wallFireLantern` | `Latern1.prefab` | #10 Modular Medieval Lanterns (Alexandr Voevodov) | https://marketplace.unity.com/packages/3d/environments/historic/modular-medieval-lanterns-85527 | Standard Unity Asset Store EULA | No |

Known credit requirements before first use: the Vintage Pocket Watch (CC BY 4.0, "Vintage Pocket Watch by Mune (Sketchfab), CC BY 4.0") and Vefects' Free Fire VFX (license not stated; wording pending OD-015).
| `oilPipe` (via `PH_OilPipe`), `P_FinalGate` visual (`PH_FinalGate`) | `Materials/Palletes/dark color.mat` | #1 Sci-Fi Styled Modular Pack (karboosx) | https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913 | Standard Unity Asset Store EULA | No |
| `sounds[FireLoop]` (via `RegistryAudio` on `P_LanternFire`) | `Audio/SFX_FireMedium_L.wav` | #9 Free Fire VFX (Vefects) | https://vefects.itch.io/free-fire-vfx-unity-urp | Not stated (credited on the win screen, AD-015) | Yes |
| `P_Guard/Badge` (prefab child, `Art/M_ScpBadge.mat` baked by `Nullflame/Bake Badge Material`) | `Art/scp_logo.png` (owner-supplied) | SCP Foundation logo (Aelanna) | https://scp-wiki.wikidot.com/licensing-guide | CC BY-SA 3.0 | Yes: "SCP Foundation logo by Aelanna, CC BY-SA 3.0" on the win screen |
| `lanternSmashVfx` | `SmallExplosion.prefab` | #8 Particle Pack (Unity) | https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-particle-pack-127325 | Standard Unity Asset Store EULA | No |
| `P_Guard/Lantern/LanternModel`, `P_ThrownLantern`, `rackLantern` (rack spares) | `Assets/polygondan_lantern.obj` | Low poly Lantern (PolygonDan, OpenGameArt) | https://opengameart.org/content/low-poly-lantern-game-asset | CC0 | No |
| `floorTile` (floors and ceilings), `wallSegment`, `crate`, `cart`, `cabinet`, `bunk`, `lanternRack`, `labBench`, `ceilingLight`, `alarmLight`, `locker`, `P_LeverPanel` console | `floor_2_blank`, `blank_wall_A`, `storage_container_small`, `container_small`, `vending_machine` (cabinet, fits the 1.2 x 2 x 0.6 m box uniformly)`, `bunk_bed`, `shelf`, `desk_no_computer`, `light_celing_1`, `light_wall_1` (alarm lamps), `computer_station` (lockers), `console` (`P_LeverPanel/Console`) | #1 Sci-Fi Styled Modular Pack (karboosx) | https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913 | Standard Unity Asset Store EULA | No |
| `containmentDoor` (D-SCP-T in Normal) | `Gate_Small.prefab` | #3 Sci Fi Doors (MASH Virtual) | https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-doors-162876 | Standard Unity Asset Store EULA | No |
| `P_Guard/Visual/Body` (prefab child) | `Character_SWAT_A_1.prefab` | #5 Low Poly Character Series 1 SWAT (Castle Bravo) | https://assetstore.unity.com/packages/3d/characters/humanoids/low-poly-character-series-1-swat-404966 | Standard Unity Asset Store EULA | No |
| `Art/Guard.controller` clips | `HumanM@Idle01`, `Walk01_Forward`, `Walk01_Backward`, `MilitaryIdle01`, `ThrowGrenade01_L`, `Death01` | #6 Human Soldier Animations FREE (Kevin Iglesias) | https://assetstore.unity.com/packages/3d/animations/human-soldier-animations-free-340690 | Standard Unity Asset Store EULA | No |
