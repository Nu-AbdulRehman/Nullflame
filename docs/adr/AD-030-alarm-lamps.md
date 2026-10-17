# AD-030: Nine wall-mounted alarm lamps

**Date:** 2026-10-18 · **Overrides:** design doc Appendix A.3 alarm positions (four floor-centred lights) · **Decided by:** owner ("add the red emergency lights, around 3 on each side of the main corridor, and 3 on the non-door side of the SCP containment units corridor, none in the final corridor, on in the meltdown state, off in the ... wound back state").

## Decision
`FacilityLayout.alarmLamps` (replaces `alarmLights`): Main Corridor west wall at Z 16, 24, 32 facing east; east wall at Z 16, 24, 32 facing west; Access Corridor north wall at X 8, 26, 34 facing south. Mounted at `alarmLampHeight` 2.6 m, just off the wall face. Each keeps the §7.7 behaviour: a red spot light aimed at its floor circle (T-ALARM-RADIUS) and an `Alarm` flame volume that drains T-DMG-FLOOR on bare floor. The fixture is pack #1 `light_wall_1` (`AssetRegistry.alarmLight`) with its own light disabled. All under `State_Meltdown/Generated_Meltdown/AlarmLights`, so the rewind removes them (§10).

## Consequences
Design doc §7.7 / Appendix A.3 need the owner's edit. `FlameTests.Alarm_OnFloor_Drains_TDMGFLOOR` stands under the lamp at (8, 12). The lamp mesh keeps the pack's white emissive glass; a red material copy is a later polish item.
