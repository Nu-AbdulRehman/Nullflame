# Asset manifest

Two lists (`CLAUDE.md` §4.1, §5.4): every builder or baker script and the content it constructs, and every third-party asset the game uses with its source and license. Rows are added in the same commit that adds the script or first uses the asset.

## Constructed content

| Script | Menu item | Builds | Owned root / output folder | Rebuild source |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Editor/FacilityBuilder.cs` | `Nullflame/Build Facility` | Floors, walls with door cuts, closed containment doors (Normal), show-only doors, invisible SCP doorway blockers, props, ceiling lights; Meltdown alarm lights (spot light + floor-circle FlameVolume) and Final Corridor wall fire lanterns (lights, lantern and flame visuals, one instant-kill box). Kit visuals from `AssetRegistry` slots, cubes when a slot is empty | `Level_Generated`, `State_Meltdown/Generated_Meltdown`, `State_Normal/Generated_Normal` | `Settings/FacilityLayout.asset`, `Settings/GameTuning.asset` (wall height), `Art/AssetRegistry.asset` |
| `Assets/_Project/Scripts/Editor/DecalBaker.cs` | `Nullflame/Bake Decals` | `PH_ShadowDecal.png/.mat`, `PH_EntityBlob.png/.mat`, `PH_Outline.mat`, `PH_Teardrop.mat`, `PH_ViewCone.asset/.mat`, `PH_Ring.png` (single sprite), `PH_OilDecal.png/.mat` (oil drop, queue 3001) (cone mesh from T-VIEW-ANGLE and T-VIEW-RANGE; URP Unlit transparent materials, render queues from IMPL §2.8) | `Assets/_Project/Placeholders/` | Code constants in `DecalBaker`; `Settings/GameTuning.asset` for the cone |
| `Assets/_Project/Scripts/Editor/PipeBuilder.cs` | `Nullflame/Build Pipes` | One `P_CrackablePipe` per Final Corridor crack point (8): soft strip sized from the layout, three drop anchors `T-PIPE-DROP-SPACING` apart, burst point on the wall face, drip and `AssetRegistry.oilPipe` visual 2.5 m up on the wall | `Shared/Pipes_Generated` | `Settings/FacilityLayout.asset` (`crackPoints`, `pipeHeight`, `pipeStripWidth`), `Settings/GameTuning.asset`, `Art/AssetRegistry.asset` |

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
