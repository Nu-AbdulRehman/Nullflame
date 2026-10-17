# Asset manifest

Two lists (`CLAUDE.md` §4.1, §5.4): every builder or baker script and the content it constructs, and every third-party asset the game uses with its source and license. Rows are added in the same commit that adds the script or first uses the asset.

## Constructed content

| Script | Menu item | Builds | Owned root / output folder | Rebuild source |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Editor/FacilityBuilder.cs` | `Nullflame/Build Facility` | Floors, walls with door cuts, closed containment doors (Normal), show-only doors, invisible SCP doorway blockers, props, ceiling lights. Kit visuals from `AssetRegistry` slots, cubes when a slot is empty | `Level_Generated`, `State_Meltdown/Generated_Meltdown`, `State_Normal/Generated_Normal` | `Settings/FacilityLayout.asset`, `Settings/GameTuning.asset` (wall height), `Art/AssetRegistry.asset` |

Planned (from `docs/implementation/nullflame-implementation.md` §25): `PipeBuilder` (`Nullflame/Build Pipes`), `DecalBaker` (`Nullflame/Bake Decals`), `ToneBaker` (`Nullflame/Bake Placeholder Tones`).

## Third-party assets

| AssetRegistry slot | Asset | Pack | Source | License | Credit required |
|---|---|---|---|---|---|

Known credit requirements before first use: the Vintage Pocket Watch (CC BY 4.0, "Vintage Pocket Watch by Mune (Sketchfab), CC BY 4.0") and Vefects' Free Fire VFX (license not stated; wording pending OD-015).
