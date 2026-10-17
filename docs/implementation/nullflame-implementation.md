# Nullflame: Implementation Specification

**Version:** 0.1 draft (2026-10-17)
**Derived from:** `docs/design/nullflame-design.md` v1.0 (the design doc), `docs/assets/free-assets.md`, `CLAUDE.md`, `AGENTS.md`, `SKILLS.md`.
**Status:** Approved for build (2026-10-17). Every `[OD-nnn]` tag below was decided by the owner on 2026-10-17; the decision is in `docs/adr/` (OD-001 → AD-003, OD-002 → AD-008, OD-003 → AD-012, OD-004 → AD-010, OD-005 → AD-004, OD-006 → AD-006, OD-007 → AD-002, OD-008 → AD-005, OD-009 → AD-013, OD-010 → AD-011, OD-011 → AD-014, OD-012 → AD-001, OD-013 → AD-009, OD-014 → AD-007, OD-015 → AD-015). Values marked **PROPOSED** are approved as written. OD-009 is the one answer that differs from this document's original default; the sections it touches are updated.

How this document relates to the others:
- The design doc says **what** the game does. If this document disagrees with it, the design doc wins; report the conflict.
- `CLAUDE.md` says **how** the agent works. This document applies those rules to each system; it does not relax any of them.
- `docs/tasks/B{n}.md` turns this document into 30-minute tasks per build step. Each task cites the section of this document it implements as `IMPL §n`.

Section numbers in this document are written `IMPL §n`. Design doc sections are written `§n`.

---

## 1. Project baseline (checked 2026-10-17, read-only)

| Item | Value | Source |
|---|---|---|
| Unity version | 6000.3.9f1 | `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP 17.3.0, `PC_RPAsset` with `PC_Renderer` (Forward+, one existing renderer feature) | `Packages/manifest.json`, `Assets/Settings/` |
| Active input handling | Input System package only (`activeInputHandler: 1`), `com.unity.inputsystem` 1.18.0 | `ProjectSettings/ProjectSettings.asset` |
| UI | `com.unity.ugui` 2.0.0 (includes TextMesh Pro; TMP Essential Resources are not imported) | manifest |
| Tests | `com.unity.test-framework` 1.6.0 | manifest |
| Packages in the template commit | AI Navigation, Input System, Test Framework, Timeline, uGUI, URP, Visual Scripting, IDE packages, Multiplayer Center, Collab Proxy | staged `Packages/manifest.json` |
| Packages added after the template commit (unstaged) | MCP for Unity, **Cinemachine 3.1.7, ProBuilder 6.1.3, Visual Effect Graph 17.3.0, glTFast 6.20.0** | `git diff Packages/manifest.json` |
| Imported asset packs | None yet (only the URP template content and `Assets/Plugins/Roslyn`) | `Assets/` listing |

The four packages added after the template commit have no approval record. This document does not use them until `[OD-005]` is answered: the camera is a custom rig, the greybox uses primitives and kit prefabs, VFX use the Particle System, and the pocket watch is imported as FBX.

---

## 2. Conventions

### 2.1 Folders `[OD-012]`

`CLAUDE.md` fixes the first four rows. The rest are PROPOSED.

| Path | Contents |
|---|---|
| `Assets/_Project/Scripts/` | Runtime MonoBehaviours, grouped by system folder (`Shadows/`, `Entity/`, `Guards/`, ...) |
| `Assets/_Project/Scripts/Logic/` | Plain C# rules (IMPL §4) |
| `Assets/_Project/Scripts/Editor/` | Builder scripts, bakers, custom inspectors |
| `Assets/_Project/Tests/EditMode/`, `Assets/_Project/Tests/PlayMode/` | Tests |
| `Assets/_Project/Art/` | `AssetRegistry.asset`, prefab variants of pack assets, material copies, `Guard.controller` |
| `Assets/_Project/Placeholders/` | `PH_*` assets |
| `Assets/_Project/Scenes/Facility.unity` | The one game scene (PROPOSED) |
| `Assets/_Project/Prefabs/` | Gameplay prefabs `P_*` (PROPOSED) |
| `Assets/_Project/Settings/` | `GameTuning.asset`, `FacilityLayout.asset` (PROPOSED) |
| `Assets/_Project/Materials/` | Project materials that are not copies of pack materials (PROPOSED) |

### 2.2 Assemblies and namespaces `[OD-012]`

PROPOSED names:

| Assembly definition | Folder | References | Namespace |
|---|---|---|---|
| `Nullflame.Runtime` | `Scripts/` (including `Logic/`) | `Unity.InputSystem`, `UnityEngine.UI`, `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Core.Runtime` | `Nullflame`, `Nullflame.Logic` |
| `Nullflame.Editor` | `Scripts/Editor/` (Editor platform only) | `Nullflame.Runtime` | `Nullflame.EditorTools` |
| `Nullflame.Tests.EditMode` | `Tests/EditMode/` (Editor platform only) | `Nullflame.Runtime`, `Nullflame.Editor`, NUnit, `UnityEngine.TestRunner`, `UnityEditor.TestRunner` | `Nullflame.Tests` |
| `Nullflame.Tests.PlayMode` | `Tests/PlayMode/` (all platforms) | `Nullflame.Runtime`, NUnit, `UnityEngine.TestRunner`, `UnityEditor.TestRunner` | `Nullflame.Tests` |

Two test assemblies, because EditMode tests need an Editor-only assembly and PlayMode tests an all-platforms one (AD-016).

The `Logic` folder stays inside the runtime assembly. A rule enforced by review (not by a separate assembly): files under `Logic/` may use only `System`, `System.Collections.Generic` and the UnityEngine value types `Vector2`, `Vector3`, `Quaternion`, `Mathf`, `Rect`. No `MonoBehaviour`, `GameObject`, `Transform`, `Time`, `Physics` or `Debug`.

### 2.3 Physics layers `[OD-007]`

User layers (AD-002). `add_layer` assigned slots 8 to 10. Code looks layers up by name, never by index:

| Layer | Index | Used by | Hit by |
|---|---|---|---|
| `LevelSolid` | 8 | Walls, door panels, closed containment doors, Semi-Final Gate panels and its doorway blocker, invisible doorway blockers | Jump line-of-sight raycast (IMPL §7.5), guard view raycast (IMPL §10.4), camera collision spherecast (IMPL §8) |
| `Entity` | 9 | The entity's trigger capsule | `FlameVolume` triggers |
| `Hazard` | 10 | `FlameVolume` trigger colliders | The entity capsule |

The physics matrix needs `Entity` × `Hazard` on. Nothing else needs to collide; gameplay never uses non-trigger rigidbody contacts. Props from packs keep whatever colliders they ship with on `Default`, which no gameplay query hits.

Fallback if layers are refused: every query uses `Physics.RaycastNonAlloc` into a preallocated buffer and filters for a `LevelSolid` marker component. This is slower and more code.

### 2.4 Finding managers at runtime `[OD-012]`

PROPOSED: one `GameContext` MonoBehaviour on the `_Managers/Game` object holds serialized references to every manager (`GameFlow`, `Essence`, `ShadowRegistry`, `WorldStateController`, `ShadowEntity`, `TimePowers`, `Hud`, `ScreenFx`, `SfxPlayer`, `OrbitCamera`, `GameTuning`, `AssetRegistry`, `FacilityLayout`). It sets `public static GameContext Current` in `Awake` and clears it in `OnDestroy`, so nothing survives a scene reload. Scene objects use serialized references where the reference is known at edit time. Prefabs instantiated at runtime (spawned guards, fires, oil drops, void shadows, thrown lanterns) read `GameContext.Current`.

Rejected alternative: inject every reference through spawner `Init` chains. Every runtime prefab would need two or three hops of injected references, which is more code for the same result.

### 2.5 Update order

Set with `[DefaultExecutionOrder]`, which is a code attribute, not a project setting:

| Order | Component | Why |
|---|---|---|
| −100 | `GameFlow` | Applies pause and state before anything ticks |
| −50 | `GuardController` | Guards move first, so ridden shadows have their final position for the frame |
| −40 | `GuardGate`, `GuardSpawner` | Read guard positions after the move |
| 0 | `ShadowEntity`, `JumpTargeting`, `ConsumeAction`, `TimePowers` | Read input, follow the ridden shadow |
| 10 | `GuardVision` | Checks the entity's final position and state for this frame |
| 20 | `LanternFire`, `FlameReceiver` | Applies damage after positions settle |
| LateUpdate | `OrbitCamera`, `Hud` | Camera and UI after gameplay |

### 2.6 Time and pause

- Logic classes take `float dt` and positions as arguments. MonoBehaviours pass `Time.deltaTime`.
- Pause sets `Time.timeScale = 0`. All gameplay uses scaled time; UI animation uses `Time.unscaledDeltaTime`.
- The intro skip fast-forwards guards by calling their `Tick` in fixed steps (IMPL §21).

### 2.7 Allocation rules

- No LINQ, `new List`, `foreach` over interfaces, string concatenation or `GetComponent` in `Update`.
- Registries hand out `IReadOnlyList<T>` backed by a `List<T>`; queries iterate by index.
- Raycasts use the non-allocating overloads into buffers owned by the caller.
- Prompt text is rebuilt only when the prompt key changes (IMPL §20.1).

### 2.8 Floor-decal rendering

Shadows, the entity blob, view cones, fire scorch and target outlines are flat quads with unlit transparent materials, not URP Decal Projectors. Decal Projectors need the Decal renderer feature, which is a pipeline asset change (`CLAUDE.md` §3). Draw order is fixed by height and render queue:

| Layer | Height above floor (m) | Render queue |
|---|---|---|
| Shadow quads (grey) | 0.010 | 3000 |
| Oil drops | 0.012 | 3001 |
| View-cone decals | 0.015 | 3002 |
| Target outline | 0.018 | 3003 |
| Entity blob | 0.020 | 3004 |

Recorded in `docs/ENGINEERING-NOTES.md` (EN-001).

---

## 3. GameTuning (S2)

**File:** `Scripts/Core/GameTuning.cs`. **Asset:** `Settings/GameTuning.asset`, `[CreateAssetMenu(menuName = "Nullflame/Game Tuning")]`.

One ScriptableObject with public serialized fields grouped by `[Header]`. Each field has a `[Tooltip]` that starts with its `T-*` id. Code reads `GameContext.Current.Tuning.<field>` or a serialized `GameTuning` reference. No other numeric gameplay constant may exist in code.

### 3.1 Rows from design doc §13

| T-* | Field | Type | Default |
|---|---|---|---|
| T-LVL-CEIL | `ceilingHeight` | float | 4 |
| T-PLY-SIZE | `entitySize` | float | 1.2 |
| T-JMP-RANGE | `jumpRange` | float | 6 |
| T-JMP-ARC | `jumpArcHeight` | float | 1.5 |
| T-JMP-TIME | `jumpTime` | float | 0.45 |
| T-ESS-MAX | `essenceMax` | float | 100 |
| T-ESS-START | `essenceStart` | float | 100 |
| T-ESS-INTRO | `introGrazeDamage` | float | 10 |
| T-ESS-FLOOR | `essenceFloor` | float | 10 |
| T-DMG-LANTERN | `lanternDps` | float | 8 |
| T-DMG-ALARM | `alarmDps` | float | 5 |
| T-LANTERN-RADIUS | `lanternRadius` | float | 3 |
| T-THROW-TIME | `throwTime` | float | 0.6 |
| T-FIRE-START | `fireStartRadius` | float | 1 |
| T-FIRE-RADIUS | `fireMaxRadius` | float | 4 |
| T-FIRE-RADIUS-OIL | `fireOilRadius` | float | 5 |
| T-FIRE-GROW | `fireGrowTime` | float | 2 |
| T-FIRE-LIFE | `fireLifeTime` | float | 6 |
| T-LANTERN-RELIGHT | `lanternRelightTime` | float | 10 |
| T-COST-PIPE | `pipeCost` | float | 5 |
| T-COST-PEDESTAL | `pedestalCost` | float | 20 |
| T-COST-GATE | `gateCost` | float | 80 |
| T-COST-REVERSE | `reverseCostPerSecond` | float | 5 |
| T-COST-REWIND | `rewindCost` | float | 30 |
| T-GAIN-GUARD | `guardGain` | float | 25 |
| T-CON-GUARD | `guardConsumeTime` | float | 2 |
| T-CON-PEDESTAL | `pedestalConsumeTime` | float | 3 |
| T-CON-GATE | `gateConsumeTime` | float | 5 |
| T-REWIND-CHANNEL | `rewindChannelTime` | float | 2 |
| T-REV-SPEED | `reverseSpeedMultiplier` | float | 2 |
| T-PIPE-BURST | `pipeBurstTime` | float | 1.5 |
| T-GRD-WALK | `guardWalkSpeedNormal` | float | 2 |
| T-GRD-ALERT | `alertTime` | float | 0.6 |
| T-VIEW-ANGLE | `viewAngle` | float (degrees, total) | 70 |
| T-VIEW-RANGE | `viewRange` | float | 8 |
| T-VIEW-GRACE | `viewGrace` | float | 0.2 |

### 3.2 New rows `[OD-008]`

These numbers appear in the design doc's prose or tables but have no `T-*` row, or don't appear at all. `CLAUDE.md` §3 requires a `T-*` row before code uses them. Values in the "Source" column marked §n are written in the design doc; values marked **new** are PROPOSED.

| Proposed T-* | Field | Default | Source |
|---|---|---|---|
| T-GRD-WALK-MELT | `guardWalkSpeedMeltdown` | 2.4 | §10, §13 (split from the T-GRD-WALK row) |
| T-PLY-SLIDE | `slideSpeed` | 2.5 m/s | **new** (§3 gives no slide speed) |
| T-JMP-SNAP | `reticleSnapDistance` | 2 m | §5.3 |
| T-JMP-RAY-Y | `jumpRayHeight` | 0.5 m | §5.3 |
| T-ESS-LOW | `essenceLowThreshold` | 25 | §6 |
| T-ALARM-RADIUS | `alarmFloorRadius` | 2 m | §7.7 |
| T-FIRE-SHRINK | `fireShrinkTime` | 1 s | §7.9 |
| T-CON-SINK | `consumeSinkFraction` | 0.4 | §7.3 (last 40%) |
| T-REWIND-FX | `rewindEffectTime` | 1.5 s | §10 |
| T-PIPE-SAFE-START | `pipeSafeStart` | 0.6 s | §7.6 |
| T-PIPE-SAFE-END | `pipeSafeEnd` | 1.1 s | §7.6 |
| T-PIPE-DROPS | `pipeDropCount` | 3 | §7.6 |
| T-PIPE-DROP-TIME | `pipeDropDuration` | 2 s | §7.6 |
| T-PIPE-DROP-SPACING | `pipeDropSpacing` | 1.5 m | §7.6 |
| T-PIPE-BURST-COST | `pipeBurstCost` | 5 | **new**, `[OD-011]` |
| T-SHD-OIL | `oilDropSize` | 1.2 m | §7.6 |
| T-SHD-VOID | `voidShadowSize` | 1.5 m | §7.1, §7.5 |
| T-SHD-GUARD-OFFSET | `guardShadowOffset` | 0.6 m | §7.1 |
| T-SHD-GUARD-W | `guardShadowWidth` | 1.5 m | §7.1 (1.5 × 1 m; 1.5 read as across the guard) |
| T-SHD-GUARD-D | `guardShadowDepth` | 1.0 m | §7.1 |
| T-GRD-HISTORY | `historyStep` | 0.25 s | §8.2, C5 |
| T-VIEW-EYE | `viewEyeHeight` | 1.6 m | **new** (§8.3 says "head height") |
| T-GATE-ZONE | `gateTriggerDepth` | 2 m | §7.8 |
| T-GATE-CLOSE | `gateCloseDelay` | 1.5 s | §7.8 |
| T-GATE-SLIDE | `gatePanelSlideTime` | 0.5 s | **new** (visual only, gameplay state switches instantly) |
| T-INTRO-WAVE | `introWaveSpacing` | 3 s | §9 Beat 0 |
| T-INTRO-GLANCE | `introGlanceTime` | 2 s | §9 Beat 0 |
| T-INTRO-CAM | `introCameraPullTime` | 1.5 s | **new** |
| T-FAIL-RESTART | `autoRestartDelay` | 3 s | §9 Beat 1, §12 |
| T-CAM-DIST | `cameraDistance` | 6 m | §11 |
| T-CAM-PITCH-MIN | `cameraPitchMin` | 25° | §11 |
| T-CAM-PITCH-MAX | `cameraPitchMax` | 70° | §11 |
| T-CAM-PITCH-START | `cameraPitchStart` | 45° | §11 |
| T-CAM-SENS | `mouseSensitivity` | 0.12 °/count | **new** |
| T-CAM-DAMP | `cameraFollowDamping` | 0.12 s | **new** (§11 says "slight damping") |
| T-CAM-RADIUS | `cameraCollisionRadius` | 0.3 m | **new** |
| T-SPN-SLEEP-M | `spawnSleepMeltdown` | interval 20 s, max 2 | §8.4 |
| T-SPN-SLEEP-N | `spawnSleepNormal` | interval 25 s, max 3 (counts the 2 routine guards) | §8.4, §10 |
| T-SPN-SIDE-N | `spawnSideNormal` | interval 15 s, max 2 in transit | §8.4 |
| T-SPN-FINAL-N | `spawnFinalNormal` | refill delay 10 s, max 2 | §8.4 |
| T-SPN-FINAL-M | `spawnFinalMeltdown` | max 2, no respawn | §8.4 |

`T-SPN-*` rows are one serializable struct each:

```csharp
[System.Serializable]
public struct SpawnerTuning { public float interval; public int maxAlive; public bool refillOnly; }
```

`GameTuning.GetSpawner(SpawnerId id)` returns the struct for `SpawnerId { SleepMeltdown, SleepNormal, SideNormal, FinalNormal, FinalMeltdown }`.

### 3.3 Data that is not a tunable

Route waypoints, dwell times on waypoints (G-GLANCE's 2 s pause uses `T-INTRO-GLANCE`; G-TIME's 3 s corner stop and G-BARRACKS's lever look are waypoint data), pipe crack-point positions and all coordinates are **level data**. They live in the scene (waypoint components) or in `FacilityLayout`, not in `GameTuning`. Recorded as EN-002. Cosmetic values (pulse rate, flash alpha, wobble amplitude) are serialized fields on the visual components that use them, never read by gameplay.

### 3.4 Tests

- `GameTuningTests.Defaults_MatchDesignTable`: creates the asset with `ScriptableObject.CreateInstance` and asserts every §13 default. One assert per row, each with the `T-*` id in its message.

---

## 4. Logic classes (plain C#)

All in `Scripts/Logic/`, namespace `Nullflame.Logic`. Each has an EditMode test fixture `Tests/EditMode/<Class>Tests.cs`. Signatures are the contract; bodies are the implementer's.

### 4.1 `EssenceLogic` (S6, §6)

```csharp
public sealed class EssenceLogic {
    public EssenceLogic(float max, float start, float floor);
    public float Value { get; }
    public float Max { get; }
    public bool IsDead { get; }                       // Value <= 0
    public bool CanAfford(float cost);                // Value > cost (strict, §6)
    public bool TrySpend(float cost);                 // false and no change if !CanAfford
    public bool SpendHeld(float amount);              // clamps at floor; returns false once Value == floor
    public void Damage(float amount);                 // may reach 0
    public void Kill();                               // Value = 0
    public void Gain(float amount);                   // clamped to Max
    public void Set(float value);                     // intro skip sets 90
}
```

Tests (EditMode): `TrySpend_RefusesWhenValueEqualsCost_TCOSTGATE` (80 at Value 80 is refused, Beat 5), `TrySpend_SucceedsAbove`, `SpendHeld_StopsAtFloor_TESSFLOOR`, `SpendHeld_NeverKills`, `Damage_CanKill`, `Gain_ClampsToMax_TGAINGUARD`, `Kill_SetsZero`.

### 4.2 `GuardMotion` (S7, S12, §8.1, §8.2, §7.4)

Deterministic guard movement and history. Owns no transforms.

```csharp
public enum GuardMoveState { Idle, Travel, Post, Reversing, Hold }

public struct RoutePoint { public Vector3 Position; public float Dwell; public bool HasLook; public Vector3 LookAt; }

public sealed class GuardMotion {
    public GuardMotion(IReadOnlyList<RoutePoint> travel, IReadOnlyList<RoutePoint> post,
                       float walkSpeed, float reverseMultiplier, float historyStep);
    public GuardMoveState State { get; }
    public Vector3 Position { get; }
    public Vector3 Facing { get; }                    // unit XZ vector
    public bool Paused { get; set; }                  // Alert/Throw/BeingConsumed freeze motion
    public int HistoryCount { get; }
    public bool ReachedEnd { get; }                   // Travel finished (Post started or route ended)
    public event System.Action ArrivedAtEnd;
    public void Start();                              // Idle -> Travel, records first history point
    public void Tick(float dt);
    public void BeginReverse();                       // from Travel/Post/Hold -> Reversing (no-op if history empty)
    public void EndReverse();                         // Reversing -> Hold
}
```

Rules:
1. **Travel:** move along `travel` points at `walkSpeed`, honouring each point's `Dwell` (stand still, face `LookAt` if set). Facing = current segment direction.
2. **History:** while in Travel and moving, append `Position` every `historyStep` seconds of travel time. Also append every waypoint position at the moment it is reached, so the recorded polyline contains every corner and reverse retraces the path exactly (EN-003). Never append a point within 0.001 m of the last one (dwells add nothing).
3. **End of travel:** if `post` has points, switch to Post at `post[0]` (the first post point is where the travel route ends); otherwise stay at the last point and raise `ArrivedAtEnd` (the controller despawns or idles). `ArrivedAtEnd` fires in both cases.
4. **Post:** loop `post` points forever with dwell and look-at. No history.
5. **Reversing:** first walk straight to the last history point, then walk backwards through history at `walkSpeed × reverseMultiplier`, removing each point when it is passed. Facing stays the original walking direction (the direction from the previous history point to the current one), so the guard walks backwards. When history is empty, switch to Hold.
6. **Hold:** stand still, facing the original walking direction of the last segment traversed.
7. `Paused` stops all position changes and dwell timers but keeps state.
8. While Reversing, `Paused` is never set by the ridden guard's own logic (the ridden guard doesn't notice the player, §7.2).

Tests: `Travel_RecordsHistoryEveryStep_TGRDHISTORY`, `Travel_RecordsCorners`, `Post_DoesNotRecordHistory`, `Reverse_RetracesExactPath` (every reverse position lies on the forward polyline within 0.001 m; Beat 3), `Reverse_RunsAtDoubleSpeed_TREVSPEED`, `Reverse_EmptyHistory_GoesToHold`, `EndReverse_HoldsFacingOriginalDirection`, `Hold_ThenReverseAgain_Continues`, `Dwell_AddsNoHistory`, `Paused_FreezesEverything`.

### 4.3 `SpotTimer` (S8, §8.3)

```csharp
public sealed class SpotTimer {
    public SpotTimer(float grace);
    public bool Tick(bool conditionHolds, float dt); // true on the frame the condition has held continuously for >= grace
    public void Reset();
}
```

Tests: `Spot_RequiresContinuousGrace_TVIEWGRACE`, `Spot_ResetsWhenConditionBreaks`, `Spot_FiresOnce_UntilReset`.

### 4.4 `ViewConeMath` (S8)

```csharp
public static class ViewConeMath {
    public static bool Contains(Vector3 eye, Vector3 forwardXZ, float totalAngleDeg, float range, Vector3 point);
}
```

Uses XZ distance for range and the XZ angle between `forwardXZ` and `point − eye`. Tests: `Contains_EdgeOfAngle_TVIEWANGLE`, `Contains_BeyondRange_TVIEWRANGE`, `Contains_Behind_IsFalse`.

### 4.5 `FireRadius` (S10, §7.9)

```csharp
public sealed class FireRadius {
    public FireRadius(float start, float max, float grow, float life, float shrink);
    public float Radius { get; }
    public bool Done { get; }
    public void Tick(float dt);
}
```

Profile: `start → max` linearly over `grow`, hold `max` for `life`, `max → 0` over `shrink`, then `Done`. (§7.9 says the fire grows over 2 s and burns for 6 s. This reads "burns for 6 s" as after growing; total 9 s. `[OD-010]` item 3.)

Tests: `Radius_AtImpact_TFIRESTART`, `Radius_AfterGrow_TFIRERADIUS`, `Radius_OilFire_TFIRERADIUSOIL`, `Radius_ShrinksToZero_TFIRESHRINK`, `Done_AfterTotal_TFIRELIFE`.

### 4.6 `PipeCrackMeter` (S16, §7.6)

```csharp
public enum PipeZone { Grey, Safe, Warning, Burst }
public enum PipeResult { None, Crack, Burst }

public sealed class PipeCrackMeter {
    public PipeCrackMeter(float safeStart, float safeEnd, float burstTime);
    public float Elapsed { get; }
    public PipeZone Zone { get; }
    public PipeResult Hold(float dt);   // returns Burst on the frame Elapsed reaches burstTime, else None
    public PipeResult Release();        // Grey -> None, Safe/Warning -> Crack; resets Elapsed
}
```

Tests: `Release_InGrey_DoesNothing`, `Release_InSafe_Cracks_TPIPESAFE`, `Release_InWarning_Cracks`, `Hold_ToBurstTime_Bursts_TPIPEBURST` (Beat 5).

### 4.7 `HoldProgress` (S11, S12)

```csharp
public sealed class HoldProgress {
    public HoldProgress(float duration);
    public float Normalized { get; }
    public bool Tick(float dt);   // true on the completing frame
    public void Cancel();         // resets to 0
}
```

Used for guard/pedestal/gate consume and the facility-rewind channel. Tests: `Completes_AtDuration_TCONGUARD`, `Cancel_Resets`.

### 4.8 `JumpArc` (S4, §5.3)

```csharp
public static class JumpArc {
    // t in [0,1]. Target is re-evaluated each frame by the caller (homing on moving shadows).
    public static Vector3 Evaluate(Vector3 from, Vector3 to, float apexHeight, float t);
}
```

Parabola `y = 4·apex·t·(1−t)` on top of the XZ lerp. Tests: `Evaluate_ApexAtHalf_TJMPARC`, `Evaluate_Endpoints`.

### 4.9 `ShadowMath` (S3)

```csharp
public static class ShadowMath {
    // Rect is the shadow's local XZ footprint centred on `center`, rotated by `yawDeg`.
    public static bool Contains(Vector3 center, float yawDeg, Vector2 size, Vector3 point);
    public static Vector3 ClosestPoint(Vector3 center, float yawDeg, Vector2 size, Vector3 point);
    public static Vector3 ClampInside(Vector3 center, float yawDeg, Vector2 size, float margin, Vector3 point); // returns center if size <= 2*margin on an axis
}
```

Tests: `Contains_RotatedRect`, `ClosestPoint_OutsideCorner`, `ClampInside_SmallerThanEntity_ReturnsCenter` (§3: sliding only in shadows larger than the entity).

### 4.10 `GateLogic` (S14, §7.8)

```csharp
public sealed class GateLogic {
    public GateLogic(float closeDelay);
    public bool IsOpen { get; }
    public void Tick(int guardsInZone, float dt);  // open while >0; closes closeDelay after it drops to 0
}
```

Tests: `Opens_WhenGuardInZone`, `StaysClosed_ForPlayerOnly` (zero guards; Beat 4), `Closes_AfterDelay_TGATECLOSE`, `ReopenWithinDelay_CancelsClose`.

### 4.11 `SpawnTimer` (S15, §8.4)

```csharp
public sealed class SpawnTimer {
    public SpawnTimer(float interval, int maxAlive, bool refillOnly);
    public bool Tick(float dt, int aliveCount);   // true when a spawn should happen now
    public void NotifyRemoved();                  // refillOnly: starts the refill delay
}
```

Interval mode: first spawn at `interval` after enable, then every `interval` while `aliveCount < maxAlive` (timer pauses at the cap). Refill mode (SP-FINAL-N): spawns up to `maxAlive` immediately on enable, then one spawn `interval` seconds after each `NotifyRemoved`. Tests: `Interval_RespectsCap_TSPNSLEEPM`, `Refill_AfterRemoval_TSPNFINALN`.

---

## 5. Scene structure

`Assets/_Project/Scenes/Facility.unity` `[OD-012]`. Hierarchy (bold = must exist by name, other names PROPOSED):

```
Facility
├─ _Managers
│   ├─ Game            GameContext
│   ├─ GameFlow        GameFlow, IntroSequence
│   ├─ WorldState      WorldStateController
│   ├─ Shadows         ShadowRegistry
│   └─ Audio           SfxPlayer (+ child AudioSources)
├─ Level_Generated     (owned by FacilityBuilder; rebuilt, never hand-edited)
│   ├─ Floors, Walls, Doorways, Blockers, Props, CeilingLights
├─ Shared              (hand-placed, both states)
│   ├─ StaticShadows   ShadowArea instances for crates, cart, cabinet, bunks, benches, rack
│   ├─ Pedestal        P_Pedestal (Consumable + ShadowArea + stopwatch visual)
│   ├─ LeverPanel      P_LeverPanel (panel visual + ShadowArea + RewindSpot)
│   ├─ SemiFinalGate   P_GuardGate
│   ├─ FinalGate       P_FinalGate (Consumable + ShadowArea)
│   ├─ Pipes_Generated (owned by PipeBuilder)
│   └─ TimeRoomArea    BoxArea marker used by the Beat 1 fail check
├─ **State_Meltdown**
│   ├─ Generated_Meltdown  (owned by FacilityBuilder: open containment doors)
│   ├─ AlarmLights         4 × P_AlarmLight
│   ├─ WallFireLanterns    6 lantern visuals + Light, 1 FlameVolume box (instant kill)
│   ├─ LeverHandle_Up
│   ├─ Routes              WaypointRoute objects
│   ├─ Guards              G-GLANCE, G-B, G-TIME, G-C, G-D, G-MAIN, G-BARRACKS
│   ├─ Spawners            SP-SLEEP-M, SP-FINAL-M
│   └─ Klaxon              AudioSource (loop)
├─ **State_Normal**   (inactive at scene start)
│   ├─ Generated_Normal    (owned by FacilityBuilder: closed containment doors)
│   ├─ LeverHandle_Down
│   ├─ Routes
│   ├─ Guards              G-ROUTINE-1, G-ROUTINE-2, G-PATROL
│   └─ Spawners            SP-SLEEP-N, SP-SIDE-N, SP-FINAL-N
├─ Player              ShadowEntity, JumpTargeting, ConsumeAction, TimePowers, FlameReceiver, EntityVisual
├─ CameraRig           OrbitCamera → Main Camera (AudioListener)
├─ PostFx              Global Volume (Vignette, Color Adjustments)
├─ UI                  Canvas (Screen Space Overlay) + EventSystem (InputSystemUIInputModule)
└─ Lighting            ambient settings, Player Room rim light
```

Shared-versus-state placement follows §10: anything that differs between states lives under a state root; everything else lives outside both. The lever **panel** and its shadow are shared so the entity stays in a valid shadow through the swap; only the handle pose differs per state.

The template's `SampleScene` stays untouched. Never delete or rename it without asking.

---

## 6. S1 Level greybox

### 6.1 `FacilityLayout` (ScriptableObject, runtime assembly)

`Scripts/Level/FacilityLayout.cs`, asset `Settings/FacilityLayout.asset`. Runtime code reads it only for named areas (the Final Corridor rect for the pipe-burst alert, the Time Room rect for the Beat 1 check).

```csharp
[System.Serializable] public struct AreaSpec   { public string id; public Rect xz; public bool playerAccess; }
[System.Serializable] public struct WallSpec   { public Vector2 a, b; }                  // centreline, metres
[System.Serializable] public struct OpeningSpec{ public string id; public Vector2 center; public float width; public OpeningKind kind; }
public enum OpeningKind { Open, OpenFrame, Door, ContainmentDoor, ShowOnlyDoor, GateSlot }
[System.Serializable] public struct PropSpec   { public string id; public PropKind kind; public Vector3 center; public Vector3 size; public float yaw; }
public enum PropKind { Crate, Cart, Cabinet, Bunk, Locker, LanternRack, LabBench }
[System.Serializable] public struct BlockerSpec{ public string id; public Vector2 center; public Vector2 size; }

public sealed class FacilityLayout : ScriptableObject {
    public float wallThickness;     // PROPOSED 0.2
    public float wallHeight;        // = T-LVL-CEIL, copied by the builder from GameTuning
    public AreaSpec[] areas;
    public WallSpec[] walls;
    public OpeningSpec[] openings;
    public PropSpec[] props;
    public BlockerSpec[] blockers;
    public Vector2[] ceilingLights;
    public bool TryGetArea(string id, out Rect xz);
}
```

All values come from design doc §4 plus Appendix A (PROPOSED, `[OD-001]`).

### 6.2 `FacilityBuilder` (Editor)

`Scripts/Editor/FacilityBuilder.cs`, menu `Nullflame/Build Facility`, run with `execute_menu_item`.

1. Load `FacilityLayout`, `GameTuning`, `AssetRegistry`.
2. Find or create `Level_Generated`, `State_Meltdown/Generated_Meltdown`, `State_Normal/Generated_Normal`. Destroy all their children (idempotent rebuild). Never touch any other object.
3. **Floors:** one floor per area, as a kit floor prefab from `AssetRegistry.FloorTile` tiled to the rect, or a scaled cube 0.1 m thick with its top at Y = 0 when the slot is empty.
4. **Walls:** for each `WallSpec`, cut the gaps of every `OpeningSpec` lying on it, then emit one segment per remaining span: a `BoxCollider` on `LevelSolid` sized `length × wallHeight × wallThickness`, plus the visual from `AssetRegistry.WallSegment` (kit modules tiled and scaled to fit the span; kit pieces fit the coordinates, never the reverse) or a cube if the slot is empty.
5. **Openings:** `Open` emits nothing. `OpenFrame` and `Door` emit a frame visual (`AssetRegistry.DoorFrame`), no collider. `ContainmentDoor` emits an open-pose door under `Generated_Meltdown` and a closed-pose door with a `LevelSolid` collider under `Generated_Normal` (`AssetRegistry.ContainmentDoor`). `ShowOnlyDoor` emits a closed door visual with a collider. `GateSlot` emits nothing (the Semi-Final and Final gates are hand-placed prefabs).
6. **Blockers:** invisible `BoxCollider` on `LevelSolid`, full wall height, no renderer.
7. **Props:** visual from the matching registry slot (or a cube), no gameplay collider. Props do **not** create shadows; their `ShadowArea` instances are hand-placed under `Shared/StaticShadows` so the owner can tune them in the Inspector, and the builder never deletes them.
8. **Ceiling lights:** a Point Light (shadows off) plus `AssetRegistry.CeilingLight` visual at each `ceilingLights` entry, height `wallHeight − 0.2`.
9. **No ceiling geometry** (`[OD-001]` item 6): at 6 m distance and 45° pitch the camera sits about 4.7 m up, above the 4 m wall tops, and a ceiling would hide the floor.
10. Mark the scene dirty and save it with `EditorSceneManager.SaveScene`.

Recorded in `docs/ASSET-MANIFEST.md` under Constructed content.

### 6.3 Tests and checks

- EditMode `FacilityBuilderTests.Build_TwiceProducesSameChildCount` (idempotency, runs the builder on a temporary scene).
- EditMode `FacilityLayoutTests.Areas_MatchDesignTable` asserts every §4.1 rect.
- Manual: open `Facility`, run `Nullflame/Build Facility`, fly the Scene view through every room.

---

## 7. S3 and S4: Shadows and the entity

### 7.1 `ShadowArea` (S3, §7.1)

`Scripts/Shadows/ShadowArea.cs`. Prefab `P_ShadowArea`: the component plus a child quad (`MeshRenderer`, shadow material from `AssetRegistry.ShadowDecalMaterial`) rotated flat at Y = 0.010. No collider is needed; queries are pure math (IMPL §4.9).

| Field | Type | Notes |
|---|---|---|
| `owner` | `GameObject` | Null for the Player Room floor |
| `opacity` | `ShadowOpacity { Opaque, Soft }` | |
| `size` | `Vector2` | Local XZ footprint in metres |
| `consumable` | `Consumable` | Optional |
| `isRewindSpot` | bool | True only on the lever panel shadow |
| `pipe` | `CrackablePipe` | Optional, set on pipe segment shadows |
| `destroyedByFire` | bool | True for oil drops and void shadows (§7.9 rule 6) |

- `IsMoving` is computed: true when a `GuardController` is in the parents.
- Centre and yaw come from the transform. `OnValidate` scales the quad to `size`. `OnDrawGizmos` draws the footprint (white for opaque, cyan for soft) so the owner sees it in Edit mode.
- `OnEnable` registers with `ShadowRegistry`; `OnDisable` unregisters.
- `public event Action<ShadowArea> Removed` fires in `OnDisable`, so the entity can react when its shadow disappears.

### 7.2 `ShadowRegistry` (S3)

`Scripts/Shadows/ShadowRegistry.cs`, on `_Managers/Shadows`. Also tracks guards and active fires, because every query that needs them is a shadow query (jump landing blocked by fire, fire destroying shadows, gate counting guards).

```csharp
public sealed class ShadowRegistry : MonoBehaviour {
    public IReadOnlyList<ShadowArea> Shadows { get; }
    public IReadOnlyList<GuardController> Guards { get; }
    public IReadOnlyList<LanternFire> Fires { get; }
    public void Register(ShadowArea s);    public void Unregister(ShadowArea s);
    public void Register(GuardController g); public void Unregister(GuardController g);
    public void Register(LanternFire f);   public void Unregister(LanternFire f);
    public ShadowArea FindAt(Vector3 point, ShadowArea exclude);           // smallest-area containing shadow
    public ShadowArea FindNearest(Vector3 point, float maxDistance, ShadowArea exclude);
    public bool IsInsideAnyFire(Vector3 point, float extraRadius);
}
```

`FindAt` prefers the smallest containing shadow, so an oil drop on a pipe strip, or a guard's shadow crossing the Player Room floor, wins over the larger one.

### 7.3 `ShadowEntity` (S4, §5)

`Scripts/Entity/ShadowEntity.cs` on `Player`. Also on `Player`: a kinematic `Rigidbody` and a trigger `CapsuleCollider` (radius `entitySize/2`, height 1, centre Y 0.5) on layer `Entity`.

```csharp
public enum EntityState { Inactive, InShadow, Jumping, Consuming, Dead }

public sealed class ShadowEntity : MonoBehaviour {
    public EntityState State { get; }
    public ShadowArea Current { get; }
    public bool IsRiding => Current != null && Current.IsMoving;
    public GuardController RiddenGuard { get; }
    public event Action<EntityState> StateChanged;
    public void Place(ShadowArea shadow, Vector3 position);  // initial placement, intro, tests
    public void BeginJump(ShadowArea target, Vector3 landingPoint);
    public void BeginConsume();  public void EndConsume();    // called by ConsumeAction and TimePowers (rewind channel)
    public void Die();
    public void SetControl(bool enabled);                     // intro, death, pause
}
```

Behaviour:
- **InShadow, static:** W/A/S/D moves camera-relative on XZ at `slideSpeed`, clamped with `ShadowMath.ClampInside(margin = entitySize/2)`.
- **InShadow, riding:** W/A/S/D disabled (§5.2 `[Default]`); position = shadow centre each frame.
- **Shadow removed** (`Current.Removed` fires, from a consume, a despawn, fire or an oil drop expiring): instantiate `P_VoidShadow` at the entity position and move into it (§5.2, §7.5). This includes the case where a ridden spawned guard despawns at a show-only door (EN-004).
- **Jumping:** position = `JumpArc.Evaluate(start, target.landing, jumpArcHeight, t / jumpTime)`, where the landing point is re-read from the target each frame if the target is moving (homing). On arrival: InShadow in the target. If the target was removed mid-flight, land in a void shadow at the landing point.
- **Dead:** stop input, play the burn/drain visual, tell `GameFlow`.
- `Essence` reaching 0 calls `Die()` (subscribe in `OnEnable`, unsubscribe in `OnDisable`).

### 7.4 `InputReader` `[OD-006]`

PROPOSED: a small `InputReader` component on `Player` reading devices directly through the Input System (`Mouse.current.delta`, `Mouse.current.leftButton.wasPressedThisFrame`, `Keyboard.current.eKey.isPressed`, ...). It exposes `Look`, `Move`, `JumpPressed`, `ConsumeHeld`, `ReverseHeld`, `RewindHeld`, `PausePressed`, `SkipPressed`. Every gameplay component reads `InputReader`, never devices. Alternative: an Input Actions asset (the template's `InputSystem_Actions` or a new one); more setup, gives rebinding the jam doesn't need.

### 7.5 `JumpTargeting` (S4, §5.3, §12)

`Scripts/Entity/JumpTargeting.cs` on `Player`.

Each frame while the entity is InShadow and has control:
1. Ray from the camera through the screen centre. Intersect with the floor plane Y = 0 → point `P`. No hit (looking up) → no target.
2. `target = registry.FindAt(P, exclude: Current) ?? registry.FindNearest(P, reticleSnapDistance, exclude: Current)`.
3. Validity:
   - in range: `ShadowMath.ClosestPoint(target, entityPos)` within `jumpRange` (XZ);
   - line of sight: `Physics.Raycast` at `jumpRayHeight` from the entity to that closest point, mask `LevelSolid`, `QueryTriggerInteraction.Ignore`, nothing hit;
   - landing point not inside any fire: `!registry.IsInsideAnyFire(landing, entitySize/2)` (§7.9 rule 5).
4. Landing point: moving target → its centre; static target → `ClampInside(P, margin = entitySize/2)`.
5. Highlight: `TargetOutline` (a `LineRenderer` loop of the target footprint, 4 corners, at Y = 0.018), white when valid, red when not (EN-005).
6. Left mouse on a valid target → `entity.BeginJump(target, landing)`.

### 7.6 `EntityVisual` (S4, §5.1)

- **Blob:** child quad, material `PH_EntityBlob` (black radial texture, baked), diameter `entitySize`, Y = 0.020, slow scale wobble (cosmetic sine).
- **Teardrop:** child `PH_Teardrop` (black stretched sphere, the placeholder), shown only while Jumping, oriented along the arc velocity.
- **Consuming:** a second black quad scales from `entitySize` to cover the target shadow's footprint over the consume progress (§7.3 step 2) and shrinks back on cancel.
- **Death:** blob shrinks and tints orange over 0.6 s (cosmetic), then `GameFlow` shows the death screen.

### 7.7 Tests

- PlayMode `JumpTests.Jump_OutOfRange_IsRed_TJMPRANGE`.
- PlayMode `JumpTests.Jump_ThroughWall_IsBlocked`.
- PlayMode `JumpTests.Jump_HomesOnMovingTarget` (Beat 1 basis).
- PlayMode `EntityTests.ShadowRemoved_LeavesVoid` (§7.5).
- PlayMode `EntityTests.Slide_ClampedToBounds`.

---

## 8. S5 Camera

`Scripts/Camera/OrbitCamera.cs` on `CameraRig`. Custom rig (no Cinemachine until `[OD-005]`).

- Yaw and pitch from `InputReader.Look × mouseSensitivity`. Pitch clamped to `cameraPitchMin..cameraPitchMax`, starts at `cameraPitchStart`.
- Pivot follows the entity position + 0.5 m Y with `Vector3.SmoothDamp` (`cameraFollowDamping`). The pivot follows the jump arc too (§11).
- Desired camera position = pivot − forward × `cameraDistance`. `Physics.SphereCast` from pivot toward it (radius `cameraCollisionRadius`, mask `LevelSolid`) pulls the camera in on a hit.
- Cursor locked and hidden during play; unlocked on pause, death, win and title screens.
- `SetScripted(Transform from, float blendTime)` lets `IntroSequence` drive the camera and blend back.
- `Reticle`: a centred dot `Image` in the HUD canvas.

Test: PlayMode `CameraTests.Pitch_IsClamped_TCAMPITCH`. Manual: the camera pulls in against a corridor wall.

---

## 9. S6 Essence

`Scripts/Core/Essence.cs` on `_Managers/Game`. Wraps `EssenceLogic`.

```csharp
public sealed class Essence : MonoBehaviour {
    public float Value { get; }   public float Max { get; }
    public float PreviewCost { get; set; }        // ghost segment on the bar; 0 = none
    public event Action<float> Changed;           // new value
    public event Action Died;
    public event Action FloorReached;             // warning pulse
    public bool CanAfford(float cost); public bool TrySpend(float cost);
    public bool SpendHeld(float amount);
    public void Damage(float amount); public void Kill(); public void Gain(float amount); public void Set(float v);
}
```

`Awake` builds `EssenceLogic(essenceMax, essenceStart, essenceFloor)`. Retries call `Set(essenceStart − introGrazeDamage)` (= 90, §14). Tests are on `EssenceLogic` (IMPL §4.1) plus PlayMode `EssenceTests.Died_FiresOnZero`.

---

## 10. S7 and S8: Guards

### 10.1 `WaypointRoute` and `Waypoint` (S7)

- `Waypoint` (on each child transform): `float dwell`, `Transform lookAt` (optional). Gizmo: sphere plus the dwell value as a label.
- `WaypointRoute` (on the parent): `bool loop` (true for post loops). `OnDrawGizmos` draws lines between children (yellow travel, green post). `ToRoutePoints(List<RoutePoint> buffer)` fills a list for `GuardMotion`.
- Routes are scene objects under each state root's `Routes` so the owner can drag waypoints and replay a beat (`CLAUDE.md` §4.1).

### 10.2 Guard prefab `P_Guard`

```
P_Guard                   GuardController, GuardVision, LanternAttack, Consumable(kind = Guard), GuardAnimation
├─ Visual                 AssetRegistry.GuardBody instance (Animator, root motion off), tinted per guard
├─ Lantern                held front-right: AssetRegistry.GuardLantern visual + Point Light (shadows off)
│   └─ LanternFlame       FlameVolume (sphere, radius = lanternRadius, layer Hazard, sourceGuard = this)
├─ Shadow                 ShadowArea (Opaque, owner = guard, consumable = guard), local (0, 0, −guardShadowOffset), size (guardShadowWidth, guardShadowDepth)
├─ ViewCone               ViewConeDecal quad (PH_ViewCone mesh), Y = 0.015
└─ AlertIcon              "!" billboard quad (PH_AlertIcon), hidden by default
```

The guard has no colliders except the lantern's trigger. Movement never uses physics.

### 10.3 `GuardController` (S7, §8.2)

Serialized fields: `WaypointRoute travelRoute`, `WaypointRoute postRoute` (optional), `GuardMood mood` (`Meltdown` uses `guardWalkSpeedMeltdown`, `Normal` uses `guardWalkSpeedNormal`), `Color lanternColor` (G-TIME amber), `bool startOnEnable` (false for intro wave guards, which `IntroSequence` starts), `bool despawnAtEnd` (spawned transit guards).

```csharp
public sealed class GuardController : MonoBehaviour {
    public GuardMoveState MoveState { get; }
    public bool HasLantern { get; }                       // false while lanternless
    public bool IsBeingConsumed { get; }
    public bool ReachedPost { get; }
    public event Action<GuardController> ArrivedAtEnd;   // Beat 1 fail check, spawner despawn
    public event Action<GuardController> Removed;         // destroyed (consumed, despawned, state swap)
    public void Init(WaypointRoute travel, WaypointRoute post, GuardMood mood, bool despawnAtEnd); // spawners
    public void StartRoute();
    public void Tick(float dt);                           // called from Update; also from IntroSequence fast-forward
    public void BeginReverse(); public void EndReverse(); // TimePowers
    public void SetPaused(bool paused);                   // LanternAttack, Consumable
    public void BeginConsumed(); public void SinkProgress(float t01);
}
```

- `Update` calls `Tick(Time.deltaTime)`, which ticks `GuardMotion` and writes position and rotation to the transform. Rotation slerps toward `Facing` for looks; gameplay uses `Facing` directly.
- The walk speed is read once when the route starts. A Meltdown guard keeps Meltdown speed for its whole life.
- `despawnAtEnd` guards destroy themselves on `ArrivedAtEnd`.
- Registers with `ShadowRegistry` in `OnEnable`, unregisters in `OnDisable`.

### 10.4 `GuardVision` (S8, §8.3)

Each frame (order 10), if the guard has a lit lantern, is not being consumed, and is not the guard the entity is riding:
1. `eye = position + up × viewEyeHeight`.
2. Point tested = the entity position (the airborne position while jumping).
3. `inCone = ViewConeMath.Contains(eye, Facing, viewAngle, viewRange, point)`.
4. Line of sight: `Physics.Linecast(eye, point + up × 0.3, LevelSolid)` hits nothing.
5. `acting = entity.State == Jumping || entity.State == Consuming || (entity.State == InShadow && entity.Current.opacity == Soft)`.
6. `if (spotTimer.Tick(inCone && los && acting, dt)) Spotted()`.

`Spotted()`: `ConsumeAction.CancelBySpotting()` and `TimePowers.CancelChannelBySpotting()` (the consume or rewind is cancelled at no cost, §7.3 step 5), then `LanternAttack.Begin(entityPosition)`.

Being consumed, the guard doesn't count as a watcher for its own consume (§7.3 step 5). `[OD-010]` item 1: the consume detection uses this same rule (cone + line of sight + 0.2 s grace) rather than §7.3's "inside any other guard's view cone at any moment".

### 10.5 `ViewConeDecal` (S8)

The cone mesh `PH_ViewCone` is a fan mesh baked by `DecalBaker` (IMPL §25.2) from `viewAngle` and `viewRange`, with a translucent white material. The decal stays visible while the guard is lanternless (§8.2: the cone stays active), tinted grey to show that spotting does nothing. Re-run the baker after changing `viewAngle` or `viewRange`.

### 10.6 `GuardAnimation` (S7, S20)

Drives `Assets/_Project/Art/Guard.controller` (built on pack #6 clips): float `Speed` (signed; negative while reversing, which plays the walk backwards through a state speed multiplier parameter), triggers `Alert` and `Throw`, bool `Consumed`. Root motion off (`CLAUDE.md` §5.2). If pack #5 or #6 isn't imported, the visual is the capsule placeholder and this component does nothing.

### 10.7 Tests

- EditMode tests on `GuardMotion`, `SpotTimer`, `ViewConeMath` (IMPL §4).
- PlayMode `GuardTests.RideGuard_EntityMovesWithShadow` (§7.2).
- PlayMode `VisionTests.OpaqueIdle_NeverSpotted`, `VisionTests.SoftShadow_Spotted_AfterGrace_TVIEWGRACE`, `VisionTests.Jump_InCone_Spotted`, `VisionTests.Lanternless_DoesNotSpot`.
- EditMode `Beat2Tests.TimeLoop_HasFourSecondWindow`: simulates G-TIME's post loop with `GuardMotion` (Appendix A route) and asserts at least one window of ≥ 4 s per cycle in which the cabinet-to-pedestal segment and the pedestal footprint are both outside the cone (§9 Beat 2 design check).

---

## 11. S9 Flame volumes

### 11.1 `FlameVolume` (§7.7)

`Scripts/Hazards/FlameVolume.cs`. Requires a trigger collider on layer `Hazard`.

| Field | Notes |
|---|---|
| `FlameKind kind` | `Lantern`, `Alarm`, `WallFire`, `LanternFire` |
| `bool instantKill` | WallFire and LanternFire |
| `GuardController sourceGuard` | Lantern only |
| `Light visual` | Optional; `SetLit(bool)` toggles the light and the collider together (§7.7) |

`DamagePerSecond` reads `lanternDps` or `alarmDps` from tuning by `kind`; it is never a serialized number.

### 11.2 `FlameReceiver` (on Player)

- `OnTriggerEnter`/`OnTriggerExit` keep a fixed-capacity list (16) of overlapping `FlameVolume`s.
- Each frame, drop entries that are null, disabled, or whose collider is disabled (a disabled collider may not raise `OnTriggerExit`; the per-frame check makes it safe either way).
- Skip all volumes while the entity is Jumping `[OD-010]` item 2 (§7.9 rule 4 exempts airborne only from lantern fire; the default extends it to every flame).
- Skip a `Lantern` volume whose `sourceGuard` is the guard the entity is riding (§7.2 `[Default]`, O1).
- `instantKill` → `Essence.Kill()`. Otherwise sum DPS × dt → `Essence.Damage`.
- `IsInFlame` (true while any damaging volume applies) drives the orange vignette (IMPL §20.3).

### 11.3 Placement

- Guard lantern: on `P_Guard` (IMPL §10.2).
- Alarm light `P_AlarmLight`: red emissive fixture from `AssetRegistry.AlarmLight`, a rotating Spot Light (cosmetic), and a `FlameVolume` with a vertical `CapsuleCollider` of radius `alarmFloorRadius` from floor to ceiling (the light is drawn as a cone; the damage shape is the floor circle, §7.7). Four instances in `State_Meltdown/AlarmLights` at Appendix A positions `[OD-003]`.
- Wall fire lanterns: six visuals (`AssetRegistry.WallFireLantern` + Fire Small VFX + Point Light) plus one `FlameVolume` `BoxCollider` filling the Final Corridor (X 15.75–21.75, Z 41.5–56, Y 0–4), `instantKill`, in `State_Meltdown/WallFireLanterns` (§7.7).

### 11.4 Tests

PlayMode `FlameTests.LanternDrains_TDMGLANTERN` (8 ± 0.5 over 1 s), `FlameTests.OwnLantern_DoesNotHurtRider`, `FlameTests.Alarm_TDMGALARM`, `FlameTests.WallFire_Kills`, `FlameTests.LightOff_StopsDamage`.

---

## 12. S10 Alert and lantern throw

### 12.1 `LanternAttack` (on `P_Guard`, §7.9, §8.2)

States: `Ready`, `Alert`, `Throwing`, `Lanternless`.

1. `Begin(Vector3 targetPoint)` (only from `Ready` with a lit lantern): `guard.SetPaused(true)`, turn to the target, show `AlertIcon`, play `AlertSting`, `GuardAnimation.Alert`. Wait `alertTime`.
2. **Throw:** capture the target point now ("where your shadow was when the throw started", §7.9 rule 2; this design reads "when the throw started" as the end of the alert). Instantiate `P_ThrownLantern` with `(from: lantern position, to: targetPoint, time: throwTime, oil: false)`. `lanternFlame.SetLit(false)`, hide the lantern visual. `guard.SetPaused(false)`; the guard resumes its previous movement state.
3. **Lanternless** for `lanternRelightTime`, then relight. Vision ignores the guard meanwhile (`HasLantern == false`).
4. `ThrowAt(Vector3 point, bool oil)`: the pipe-burst entry point (§8.3 lists the burst as an alert trigger). Same flow as `Begin`, including the alert wait, but the target is the fixed burst point and `oil` is passed to the projectile.

### 12.2 `ThrownLantern` (prefab `P_ThrownLantern`)

Lantern visual + Point Light. Flies along `JumpArc.Evaluate(from, to, apex 1.5 m, t)` over `throwTime` (it does not home). On arrival: spawn `AssetRegistry.LanternSmashVfx`, instantiate `P_LanternFire` at `to` with `oil`, destroy self.

### 12.3 `LanternFire` (prefab `P_LanternFire`)

- Wraps `FireRadius(fireStartRadius, oil ? fireOilRadius : fireMaxRadius, fireGrowTime, fireLifeTime, fireShrinkTime)`.
- `SphereCollider` trigger on `Hazard` with `FlameVolume(kind = LanternFire, instantKill)`; radius set every frame from `FireRadius.Radius`. The entity's 0.6 m capsule makes overlap equal "the fire's edge touches the entity's shadow" (§7.9 rule 4).
- Visual: `AssetRegistry.LanternFireVfx` (pack #9 Fire Floor) scaled every frame so its footprint matches the gameplay radius (`CLAUDE.md` §5.2: the particles follow the value, never decide contact). Plus `FireLoop` audio.
- Each frame: every registered shadow with `destroyedByFire` whose footprint centre is within the radius is destroyed (§7.9 rule 6, §7.6 burst rule).
- Registers with `ShadowRegistry.Fires` for the jump-landing check.
- Destroys itself when `Done`.

### 12.4 Tests

- EditMode `FireRadius` tests (IMPL §4.5).
- PlayMode `AttackTests.Spotted_ThrowsAfterAlert_TGRDALERT_TTHROWTIME`, `AttackTests.FireContact_Kills`, `AttackTests.FireDoesNotKillAirborne`, `AttackTests.EscapeBeyondRadius_Survives_TFIRERADIUS`, `AttackTests.FireDestroysOilAndVoid`, `AttackTests.Relight_AfterTLANTERNRELIGHT`.

---

## 13. S11 Consume

### 13.1 `Consumable`

On guards, the pedestal and the Final Gate.

| Field | Notes |
|---|---|
| `ConsumableKind kind` | `Guard`, `Pedestal`, `FinalGate`; cost/gain and duration come from tuning by kind |
| `string displayName` | Prompt text: "Guard", "Time SCP", "Final Gate" |
| `Transform sinkRoot` | What sinks into the floor |
| `UnityEvent onConsumed` | Inspector-wired: pedestal → `TimePowers.Grant()`; gate → `GameFlow.Win()` |

`EssenceDelta`: Guard `+guardGain`, Pedestal `−pedestalCost`, FinalGate `−gateCost`. `Duration`: `guardConsumeTime`, `pedestalConsumeTime`, `gateConsumeTime`.

### 13.2 `ConsumeAction` (on Player)

1. Prompt context: entity InShadow, `Current.consumable != null`. Cost shown red and E ignored when `!Essence.CanAfford(cost)` (§6). Guards always affordable.
2. Hold E → `entity.BeginConsume()`, `HoldProgress(Duration)`; `Essence.PreviewCost = cost`. Guard target: `guard.BeginConsumed()` (frozen); its lantern stays lit but the guard's vision stops.
3. Sink: in the last `consumeSinkFraction` of the duration, lower `sinkRoot` below the floor.
4. Complete: apply Essence (`TrySpend` or `Gain`), invoke `onConsumed`, destroy the owner (the shadow's `Removed` fires; the entity moves into a void shadow), play `ConsumeDone`.
5. Release early or `CancelBySpotting()`: `HoldProgress.Cancel()`, restore the sink and the guard's movement, no Essence change.
6. The pedestal and the stopwatch are one consume (§7.3 `[Default]`): the stopwatch is a child of the pedestal's `sinkRoot`.

### 13.3 `VoidShadow` (prefab `P_VoidShadow`)

`ShadowArea` (Opaque, size `voidShadowSize`², `destroyedByFire = true`) + `VoidShadow` component that destroys the object when the entity's `Current` is no longer this shadow (§7.5).

### 13.4 Tests

PlayMode `ConsumeTests.Guard_GainsTGAINGUARD_AfterTCONGUARD`, `ConsumeTests.ReleaseEarly_NoCost`, `ConsumeTests.Spotted_CancelsAtNoCost`, `ConsumeTests.Pedestal_Unseen_CostsAndGrantsPower` (Beat 2), `ConsumeTests.Pedestal_InCone_LeadsToDeath` (Beat 2: spotted → fire at the pedestal, the entity stays put and dies), `ConsumeTests.Gate_RefusedAtEightyOrLess_TCOSTGATE` (Beat 5), `ConsumeTests.LeavesVoidShadow`.

---

## 14. S12 Time powers

`Scripts/Entity/TimePowers.cs` on Player.

```csharp
public sealed class TimePowers : MonoBehaviour {
    public bool HasTimePower { get; }
    public bool IsReversing { get; }
    public bool IsChanneling { get; }
    public event Action Granted;
    public void Grant();                       // wired from the pedestal's onConsumed
    public void CancelChannelBySpotting();
}
```

**Local reverse (§7.4):**
- Available when `HasTimePower`, `entity.IsRiding`, the ridden guard's history isn't empty, and `Essence.Value > essenceFloor`.
- R pressed → `guard.BeginReverse()`. While held: `Essence.SpendHeld(reverseCostPerSecond × dt)`; if it returns false (floor reached), stop.
- R released, history empty (guard state turns Hold), or floor → `guard.EndReverse()`; the guard holds. Fire `FloorReached` for the bar pulse.
- `ScreenFx.SetTimeEffect(true)` and the whoosh loop while reversing.

**Facility rewind (§7.4, §10):**
- Available when `HasTimePower`, `Current.isRewindSpot`, `WorldStateController.State == Meltdown`, and `Essence.CanAfford(rewindCost)`.
- Hold Q → `entity.BeginConsume()` (so vision treats it as consuming, §7.4), `HoldProgress(rewindChannelTime)`, `Essence.PreviewCost = rewindCost`. The Meltdown lever handle rotates down with progress (cosmetic, EN-006).
- Complete → `Essence.TrySpend(rewindCost)`, `WorldStateController.Rewind()`.
- Release early or spotted → cancel, no cost.

Tests: PlayMode `TimeTests.Reverse_CostsTCOSTREVERSE_PerSecond`, `TimeTests.Reverse_StopsAtFloor_TESSFLOOR`, `TimeTests.Release_LeavesGuardOnHold` (Beat 3), `TimeTests.Rewind_OnlyFromLever_InMeltdown_WithPower` (Beat 3), `TimeTests.Rewind_CostsTCOSTREWIND`, `TimeTests.NoPower_RQDoNothing`.

---

## 15. S13 World states

`Scripts/World/WorldStateController.cs` on `_Managers/WorldState`.

```csharp
public enum WorldState { Meltdown, Normal }
public sealed class WorldStateController : MonoBehaviour {
    [SerializeField] GameObject meltdownRoot, normalRoot;
    public WorldState State { get; }
    public event Action<WorldState> Changed;
    public void Rewind();   // one-way Meltdown -> Normal
}
```

`Rewind()`:
1. `ScreenFx.PlayRewind(rewindEffectTime)` (white-out and scanline sweep).
2. At the midpoint: `normalRoot.SetActive(true)`, `Destroy(meltdownRoot)`. Destroying the root destroys every Meltdown guard, spawned guard, alarm light and the wall fire volume in one call, which satisfies "guards from the old state are destroyed" (§10). The player stays where they are; the lever panel shadow is shared.
3. Switch music (`MusicMeltdown` → `MusicNormal`), stop the klaxon (it is under the Meltdown root).
4. Raise `Changed`.

`Awake` asserts `meltdownRoot` active and `normalRoot` inactive. Nothing else may toggle the roots (`CLAUDE.md` §3).

Tests: PlayMode `WorldStateTests.Rewind_SwapsRoots`, `WorldStateTests.Rewind_TurnsOffWallFire` (Beat 3 result), `WorldStateTests.Rewind_IsOneWay`, `WorldStateTests.Rewind_DestroysMeltdownGuards`.

---

## 16. S14 Guard-only gate

Prefab `P_GuardGate` at G-SEMI (X 15.75–21.75, Z 40–41.5):

```
P_GuardGate          GuardGate
├─ PanelWest, PanelEast   AssetRegistry.GatePanel visuals with LevelSolid BoxColliders
└─ DoorwayBlocker         LevelSolid BoxCollider filling the 6 × 4 m doorway, enabled while closed
```

`GuardGate` each frame counts registered guards whose position is inside the zone rect X 15.75–21.75, Z (40 − gateTriggerDepth) to (41.5 + gateTriggerDepth), feeds `GateLogic.Tick`, sets `DoorwayBlocker.enabled = !IsOpen`, and slides the panels over `gatePanelSlideTime` (visual). It counts any guard in any movement state, reversing included (§7.4). The entity never counts. Panels and the blocker block jump raycasts and guard sight (§7.8).

Tests: EditMode `GateLogic` tests. PlayMode `GateTests.PlayerAlone_GateStaysClosed` (Beat 4), `GateTests.ReversedGuard_OpensGate` (Beat 4), `GateTests.ClosedGate_BlocksJump`.

---

## 17. S15 Spawners

Prefab `P_GuardSpawner`:

| Field | Notes |
|---|---|
| `SpawnerId id` | Selects the `T-SPN-*` struct |
| `GuardController guardPrefab` | `P_Guard` |
| `Transform[] spawnPoints` | Alternated in order (SP-SIDE-N uses W then E) |
| `WaypointRoute[] travelRoutes` | One per spawn point, same index |
| `WaypointRoute postRoute` | Optional (SP-FINAL-N patrol loop, SP-SLEEP-N routine loop) |
| `GuardMood mood` | |
| `bool despawnAtEnd` | True for SP-SLEEP-M and SP-SIDE-N |
| `GuardController[] countsExisting` | SP-SLEEP-N counts the two routine guards toward its cap of 3 |

Uses `SpawnTimer`. Instantiates guards as children of the spawner (so a state swap destroys them), calls `Init`, then `StartRoute`. Tracks alive guards through `Removed`; calls `NotifyRemoved` for refill mode. "In transit" caps (SP-SIDE-N) count only guards whose state is not Hold, so a guard the player parks on Hold doesn't stall the flow (EN-007).

Tests: EditMode `SpawnTimer` tests. PlayMode `SpawnerTests.CapRespected`, `SpawnerTests.SpawnedGuardDespawnsAtEnd`.

---

## 18. S16 Oil pipes

### 18.1 `PipeBuilder` (Editor)

Menu `Nullflame/Build Pipes`. Owns `Shared/Pipes_Generated`, idempotent. For each crack point in `FacilityLayout` (Appendix A.6): a `P_CrackablePipe` segment, 3 m long, at height 2.5 m on the wall, visual `AssetRegistry.OilPipe` (`PH_OilPipe` until a pack covers it), with a soft `ShadowArea` strip on the floor and three drop anchor transforms. Listed in `docs/ASSET-MANIFEST.md`.

### 18.2 `CrackablePipe`

| Field | Notes |
|---|---|
| `ShadowArea strip` | Soft, `pipe = this` |
| `Transform[] dropAnchors` | 3 positions on the floor, `pipeDropSpacing` apart, offset into the corridor |
| `Transform burstPoint` | Floor point below the pipe |
| `ParticleSystem drip` | `PH_OilDrip` (Kenney sprite, tinted black) |
| `bool cracked` | Read-only at runtime |

Flow (from `ConsumeAction` when `Current.pipe != null && !pipe.cracked`):
- Hold E ticks `PipeCrackMeter`; the HUD shows the zone ring. The warning zone plays `PipeCreak`.
- `Release()` → `Crack`: `Essence.TrySpend(pipeCost)` (refused and no crack if unaffordable), `cracked = true`, start the drip, spawn `P_OilDrop` at each anchor spread over `pipeDropDuration`.
- `Hold` → `Burst`: `Essence.TrySpend(pipeBurstCost)` `[OD-011]`, `cracked = true`, spawn `PipeBurstVfx`, and for every registered guard with a lantern whose position is inside the `FinalCorridor` area: `LanternAttack.ThrowAt(burstPoint, oil: true)` (§7.6).
- `OilDrop` (prefab `P_OilDrop`): `ShadowArea` (Opaque, size `oilDropSize`², `destroyedByFire = true`), black oil quad at Y = 0.012. Permanent (§7.6 `[Default]`).

Oil hides from guards but not from lantern light: no special code, since `FlameReceiver` already ignores opacity.

### 18.3 Tests

EditMode `PipeCrackMeter` tests. PlayMode `PipeTests.SafeRelease_SpawnsThreeOpaqueDrops_TPIPEDROPS` (Beat 5), `PipeTests.Burst_AllCorridorGuardsThrow` (Beat 5), `PipeTests.CrackedPipe_CannotCrackAgain`, `PipeTests.OilFire_DestroysDrops_TFIRERADIUSOIL`.

---

## 19. S17 Game flow

`Scripts/Core/GameFlow.cs` on `_Managers/GameFlow`.

```csharp
public enum FlowState { Title, Intro, Playing, Paused, Dead, Won, Missed }
public sealed class GameFlow : MonoBehaviour {
    public static bool HasSeenIntro;          // the only static that survives a reload (CLAUDE.md §3)
    public FlowState State { get; }
    public float RunTime { get; }             // win screen "time taken"
    public void Play();  public void Win();  public void Restart();  public void Quit();
}
```

Start-up (`Start`):
- `HasSeenIntro == false` → Title screen, gameplay inactive, cursor free. **Play** → `IntroSequence.Run()`.
- `HasSeenIntro == true` → skip the title and the intro: `IntroSequence.Skip()` (IMPL §21), Essence 90, state Playing.

Transitions:
- Death (`Essence.Died`): state Dead, death screen "Burned away.", auto `Restart()` after `autoRestartDelay`, or the Restart button.
- `Restart()`: `HasSeenIntro = true`, `SceneManager.LoadScene(active scene)`. This needs `Facility` in the build scene list `[OD-014]`.
- Win (Final Gate `onConsumed`): state Won, cursor free, win screen with run time and credits. **Return to title** sets `HasSeenIntro = false` and reloads, so the title and intro show again (EN-008).
- Esc: Pause (`Time.timeScale = 0`) with Resume, Restart, Quit. Quit = `Application.Quit()` (in the Editor, `EditorApplication.isPlaying = false` behind `#if UNITY_EDITOR`).
- **Beat 1 soft fail (§9 Beat 1):** subscribe to `ArrivedAtEnd` on the five wave guards (serialized array). When all five have arrived, the entity is not inside the `TimeRoomArea` rect, and the time power isn't granted: state Missed, show "You missed your chance", auto `Restart()` after `autoRestartDelay`.
- `RunTime` counts scaled time from the moment control is given (EN-009).

Tests: PlayMode `FlowTests.Death_ReloadsAndSkipsIntro` (§14), `FlowTests.Retry_StartsAtNinety` (Beat 0 acceptance on retries), `FlowTests.AllWaveGuardsPosted_PlayerOutside_Missed` (Beat 1 fail).

---

## 20. S19 UI

All UI is authored GameObjects under `UI/Canvas` (Screen Space Overlay, `CanvasScaler` scale with screen size 1920 × 1080). Text uses uGUI `Text` with the built-in `LegacyRuntime.ttf` (EN-010; TMP needs its Essential Resources imported, which is a change the owner can choose later). Every panel is a prefab under `Prefabs/UI/`.

| Element | Object | Driven by |
|---|---|---|
| Essence bar (§12) | `HUD/EssenceBar`: background, fill `Image` (filled, black-to-violet gradient sprite `PH_EssenceGradient`), ghost `Image` (cost preview), value `Text` | `EssenceBarView` subscribes to `Essence.Changed`; ghost from `PreviewCost`; pulse below `essenceLowThreshold` and on `FloorReached` |
| Reticle | `HUD/Reticle` dot | static |
| Context prompt | `HUD/Prompt` `Text` bottom-centre | `PromptPresenter` (IMPL §20.1) |
| Hold ring | `HUD/HoldRing`: radial `Image` around the reticle | `HoldRingView` reads the active `HoldProgress` or `PipeCrackMeter`; pipe mode shows three background arcs (grey 0–0.6, green 0.6–1.1, red 1.1–1.5 of 1.5 s) |
| Hints | `HUD/Hint` `Text` | `HintPresenter` (IMPL §20.2) |
| Alert "!" | world-space quad on `P_Guard` | `LanternAttack` |
| Damage feedback | Volume `Vignette` (orange) + `HUD/HitFlash` full-screen `Image` | `ScreenFx` |
| Time effect | Volume `ColorAdjustments.saturation` + Full Screen Pass renderer feature with `M_Scanlines` (AD-013) | `ScreenFx` |
| Rewind white-out | `HUD/WhiteOut` full-screen `Image` | `ScreenFx` |
| Title | `Screens/Title`: "Nullflame", Play, Quit | `GameFlow` |
| Death | `Screens/Death`: "Burned away.", Restart | `GameFlow` |
| Missed | `Screens/Missed`: "You missed your chance", Restart | `GameFlow` |
| Pause | `Screens/Pause`: Resume, Restart, Quit | `GameFlow` |
| Win | `Screens/Win`: "NULLFLAME — Contained: no", time taken, credits, Return to title | `GameFlow` |

Win screen credits, copied exactly:
- "Vintage Pocket Watch by Mune (Sketchfab), CC BY 4.0"
- Vefects credit for the fire VFX (the exact wording needs the owner; `[OD-015]`)

### 20.1 `PromptPresenter`

Picks the first matching context, builds the string only when the context key or affordability changes:

| Context | Text |
|---|---|
| Consumable guard | `[E] Consume Guard (+25)` |
| Consumable object | `[E] Consume <name> (<cost>)`, red when unaffordable |
| Uncracked pipe | `[E] Crack pipe (5)` |
| Lever, Meltdown, has power | `[Q] Rewind facility (30)`, red when unaffordable |
| Riding, has power, history left | `[R] Reverse (5/s)` |

Numbers come from `GameTuning`. R and Q prompts are hidden until the time power is granted (§7.4).

### 20.2 `HintPresenter`

One-shot hints per run (they reset on reload, which is fine because they're cheap to re-show): move (control given), jump (first valid target), consume (first consumable shadow), reverse (time power granted: "Hold R while riding a guard to reverse their time.", §9 Beat 2), rewind (first time in the lever shadow), crack pipe (first pipe shadow).

### 20.3 `ScreenFx`

On `PostFx`. References the Global Volume profile overrides, the HUD images and the scanline material `Materials/M_Scanlines.mat` (Shader Graph `Materials/SG_Scanlines.shadergraph`, Fullscreen target, an `_Intensity` float). `SetInFlame(bool)`, `HitFlash()`, `SetTimeEffect(bool)`, `PlayRewind(float duration)`. `SetTimeEffect` lerps saturation and the material's `_Intensity`; at intensity 0 the pass draws nothing visible. The Volume profile is a scene asset. The only renderer-asset change is the Full Screen Pass Renderer Feature on `PC_Renderer` (Injection Point: After Rendering Post Processing, material `M_Scanlines`), approved in AD-013. Verify the feature's name and runtime toggling with `unity_docs` for URP 17.3 before building; ask before adding it to `Mobile_Renderer`.

### 20.4 Tests

PlayMode `UiTests.Bar_ShowsGhostCost`, `UiTests.Prompt_RedWhenUnaffordable`. Manual: every screen appears and its buttons work.

---

## 21. S18 Intro sequence

`Scripts/Core/IntroSequence.cs` on `_Managers/GameFlow`. A coroutine (Timeline is installed but adds authoring work the coroutine doesn't need).

Serialized references: the wave guards in order (G-GLANCE, G-B, G-TIME, G-C, G-D), G-MAIN, the five Meltdown containment door visuals, the close-up camera transform, the klaxon, the Player Room `ShadowArea` and the entity spawn point.

`Run()`:
1. t = 0: black screen (`WhiteOut` image tinted black), entity `Place()`d in the Player Room shadow, control off.
2. t = 1 s: fade in. Alarm lights already on (Meltdown root). Klaxon starts. Containment doors animate from closed pose to open over 1.5 s.
3. Wave release: `StartRoute()` on wave guard *i* at t = 3 + `introWaveSpacing` × *i*; G-MAIN after G-D.
4. When G-GLANCE begins its dwell at D-PLAYER (its route waypoint with `dwell = introGlanceTime` and `lookAt` inside the room): `Essence.Damage(introGrazeDamage)`, `ScreenFx.HitFlash()`. This is scripted (§6 "scripted"), not lantern-volume damage.
5. When G-GLANCE leaves the door: `OrbitCamera.SetScripted(closeUp, introCameraPullTime)` blends back to the gameplay camera; then control on, `HasSeenIntro = true`, `RunTime` starts, the "move" hint shows.
6. Space skips from any point after the first play (§9 Beat 0), using the same path as `Skip()`.

`Skip()` (retries, and Space): set the doors open, `Essence.Set(essenceStart − introGrazeDamage)`, then fast-forward deterministically: for every wave guard and G-MAIN, compute its release time from step 3 and call `Tick(1/60)` until the control moment `t_control` (the time step 5 would give control in a full run). Guard positions and histories come out identical to a full run (EN-011). Then control on.

Acceptance (Beat 0): after the intro, Essence = 90, the entity is in the Player Room shadow with control, and G-TIME has not yet passed D-PLAYER.

Tests: PlayMode `IntroTests.AfterIntro_EssenceNinety_ControlOn_Beat0`, `IntroTests.Skip_MatchesFullRun_GuardPositions` (compares G-TIME's position and history count at `t_control` from `Run()` and `Skip()`).

---

## 22. S20 Audio and VFX

### 22.1 `SfxPlayer`

On `_Managers/Audio`. A small pool of 2D `AudioSource`s for one-shots, and named loops (`MusicMeltdown`, `MusicNormal`, `ReverseWhoosh`, `Heartbeat`). Clips come only from `AssetRegistry` audio slots. Calls: `PlayOneShot(SfxId)`, `SetLoop(SfxId, bool)`. `Heartbeat` loops while Essence < `essenceLowThreshold`.

3D sounds live on their objects: guard footsteps (`GuardFootsteps`, one `AudioSource`, step interval from walk speed), fire loop on `P_LanternFire`, klaxon on `State_Meltdown/Klaxon`, gate slide on `P_GuardGate`, drip on `CrackablePipe`.

### 22.2 Sound events

Each event has its own distinct sound (`CLAUDE.md` §5.3):

| SfxId | Source (asset list §2) |
|---|---|
| Klaxon | Sonniss GDC bundle |
| AlertSting | Sonniss |
| JumpWhoosh, ReverseWhoosh, RewindSweep | Kenney Sci-fi Sounds |
| ConsumeLoop, ConsumeDone, PowerGranted | Sonniss or Kenney |
| FireLoop | Pack #9 |
| LanternSmash | Sonniss |
| PipeCreak, PipeCrack, OilDrip, PipeBurst | Sonniss |
| GateSlide, DoorSlide | Sonniss |
| Footstep | Footsteps – Essentials |
| Heartbeat | Sonniss |
| MusicMeltdown, MusicNormal | Sonniss |
| DeathSting, WinSting, UiClick | Kenney or Sonniss |

Any event with no fitting clip gets a baked `PH_Tone_<Event>.wav` from `ToneBaker` (IMPL §25.2) and a `docs/PLACEHOLDERS.md` row.

### 22.3 VFX

| Effect | Asset |
|---|---|
| Lantern fire disc | Pack #9 Fire Floor, scaled to the gameplay radius |
| Pipe-burst oil fire | Pack #9 Fire Floor Big + pack #8 explosion |
| Lantern smash | Pack #8 small explosion |
| Wall fire lanterns | Pack #10 lantern + pack #9 Fire Small |
| Oil drip | `PH_OilDrip` particle system, Kenney Particle Pack sprite tinted black |
| Consume, rewind | Scaling black decal + screen filter (the cut-list version is the default build; fancier VFX only in B10 if time allows) |

---

## 23. AssetRegistry

`Scripts/Core/AssetRegistry.cs`, asset `Art/AssetRegistry.asset`. One field per purpose; each points at a pack prefab, a prefab variant under `Art/`, or a `PH_*` placeholder. Builders and prefabs read slots; nothing references a pack asset directly.

| Slot | Type | Intended asset | Gap fallback |
|---|---|---|---|
| `FloorTile`, `WallSegment`, `DoorFrame` | GameObject | #1 Sci-Fi Styled Modular | cube with #1 material |
| `ContainmentDoor` | GameObject | #3 Sci Fi Doors | cube panel |
| `GatePanel` | GameObject | #3 (two doors) | `PH_GatePanel` primitive with #1 wall material |
| `FinalGate` | GameObject | #3 scaled | `PH_FinalGate` primitive |
| `Crate`, `Cart`, `Cabinet` | GameObject | #1 props | `PH_Crate`, `PH_Cart`, `PH_Cabinet` cubes |
| `Pedestal` | GameObject | #1 column/machine | `PH_Pedestal` cylinder |
| `Stopwatch` | GameObject | Sketchfab Vintage Pocket Watch (CC BY 4.0) | `PH_Stopwatch` cylinder + disc |
| `Bunk` | GameObject | #1 beds | cube |
| `Locker` | GameObject | none confirmed | `PH_Locker` cube with #1 panel material |
| `LanternRack` | GameObject | #1 shelf + #10 lanterns (unlit) | cubes |
| `LeverPanel` | GameObject | #1 machine/computer | cube |
| `LeverHandle` | GameObject | none found | `PH_LeverHandle` primitive |
| `LabBench` | GameObject | #4 / #1 desks | cube |
| `OilPipe` | GameObject | none found | `PH_OilPipe` cylinder with #1 metal |
| `CeilingLight`, `AlarmLight` | GameObject | #1 lights (alarm: red emissive material copy) | small cube + Light |
| `WallFireLantern` | GameObject | #10 | `PH_WallLantern` |
| `GuardBody` | GameObject | #5 SWAT (Humanoid) | `PH_GuardCapsule` capsule + head sphere |
| `GuardController` | RuntimeAnimatorController | `Art/Guard.controller` on #6 clips | none (animation optional) |
| `GuardLantern` | GameObject | #10 hand-held or CC0 Low poly Lantern OBJ | `PH_GuardLantern` |
| `LanternFireVfx`, `FireSmallVfx`, `PipeBurstVfx`, `LanternSmashVfx` | GameObject | #9, #8 | default Particle System with flat orange |
| `OilDripVfx` | GameObject | Kenney Particle Pack sprite | `PH_OilDrip` |
| `ShadowDecalMaterial`, `EntityBlobMaterial`, `OilDropMaterial`, `ViewConeMaterial`, `OutlineMaterial` | Material | baked | `PH_*` |
| `ViewConeMesh`, `TeardropMesh` | Mesh | baked | `PH_ViewCone`, `PH_Teardrop` |
| Audio slots (IMPL §22.2) | AudioClip | as listed | `PH_Tone_*` |

---

## 24. Beat wiring summary

| Beat | Scene objects involved | Systems | Acceptance test(s) |
|---|---|---|---|
| 0 Intro | Wave guards, G-GLANCE route dwell, containment doors, klaxon, close-up camera | S7, S13, S17, S18, S19 | `IntroTests.AfterIntro_EssenceNinety_ControlOn_Beat0` |
| 1 Catch a ride | G-TIME route (west half), Player Room shadow, `TimeRoomArea` | S3, S4, S7, S17 | `Beat1Tests.RideGTime_DeliversToTimeRoom_OneJump`, `FlowTests.AllWaveGuardsPosted_PlayerOutside_Missed` |
| 2 Stopwatch | G-TIME post loop, cabinet shadow, pedestal | S8, S10, S11, S12 | `Beat2Tests.TimeLoop_HasFourSecondWindow`, `ConsumeTests.Pedestal_Unseen_CostsAndGrantsPower`, `ConsumeTests.Pedestal_InCone_LeadsToDeath` |
| 3 Lever | G-TIME history, G-MAIN, SP-SLEEP-M, G-BARRACKS, lever panel | S7, S12, S13 | `GuardMotionTests.Reverse_RetracesExactPath`, `TimeTests.Release_LeavesGuardOnHold`, `TimeTests.Rewind_OnlyFromLever_InMeltdown_WithPower`, `Beat3Tests.ReverseGTime_PassesDoors_EndsNearLever` |
| 4 Semi-Final Gate | SP-SIDE-N routes, G-SEMI | S12, S14, S15 | `GateTests.PlayerAlone_GateStaysClosed`, `GateTests.ReversedGuard_OpensGate`, `Beat4Tests.ReverseThroughGate_ReachesFinalCorridor` |
| 5 Oil and Final Gate | Pipes, SP-FINAL-N, Final Gate | S10, S11, S16 | `ConsumeTests.Gate_RefusedAtEightyOrLess_TCOSTGATE`, `PipeTests.SafeRelease_SpawnsThreeOpaqueDrops_TPIPEDROPS`, `PipeTests.Burst_AllCorridorGuardsThrow` |

Beat PlayMode tests load `Facility` with `EditorSceneManager.LoadSceneInPlayMode` (Editor-only API, so tests don't depend on the build scene list), place the entity directly with `ShadowEntity.Place`, and drive guards with `Tick`. Verify the API with `unity_docs` before use.

---

## 25. Editor tooling

### 25.1 Builders

| Menu item | Script | Owns |
|---|---|---|
| `Nullflame/Build Facility` | `FacilityBuilder` | `Level_Generated`, `State_*/Generated_*` |
| `Nullflame/Build Pipes` | `PipeBuilder` | `Shared/Pipes_Generated` |

### 25.2 Bakers

| Menu item | Script | Produces |
|---|---|---|
| `Nullflame/Bake Decals` | `DecalBaker` | `PH_EntityBlob.png`, `PH_ShadowDecal.png`, `PH_OilDrop.png`, `PH_Outline` material, `PH_ViewCone.asset` mesh (from `viewAngle`, `viewRange`), `PH_Teardrop.asset` mesh, `PH_EssenceGradient.png`, `PH_AlertIcon.png` |
| `Nullflame/Bake Placeholder Tones` | `ToneBaker` | `PH_Tone_<Event>.wav` for events with no approved clip |

Each baker writes into `Assets/_Project/Placeholders/`, overwrites its own outputs (idempotent), and has rows in `docs/ASSET-MANIFEST.md` and `docs/PLACEHOLDERS.md`.

---

## 26. Risks found while writing this spec

These are design-level findings. Each has an `OD-*` row; none is resolved here.

1. **Beat 1 jump-range default can't hold** `[OD-013]`. §9.1 says the other wave guards walk the east half of the Main Corridor "out of jump range (> 6 m) of the doorway". The corridor is 6 m wide (X 15.75–21.75), so a guard on the east half is at most about 6 m from the doorway, and its shadow is closer still: about 4.9 m. Wrong guards will be in range. §9 Beat 1 already says riding a wrong guard just leads to the soft fail.
2. **Beat 3 Essence budget doesn't close** `[OD-004]`. Start of Beat 3 ≈ 65. With T-REV-SPEED as 2 × the Meltdown walk (4.8 m/s), the 56 m reverse costs about 59; with 2 × 2 m/s (the doc's math) it costs about 70. Either way, one guard eaten (+25) leaves about 20 to 31 at the lever, and the rewind needs Essence > 30. The player has to eat two guards, or a tunable changes.
3. **Pipe-burst escape is marginal** `[OD-003]`. The burst point is under a wall pipe; the opposite wall's pipe strip is about 4.3 to 5.3 m away across the 6 m corridor. Surviving the 5 m oil fire (plus the entity's 0.6 m radius) needs a diagonal jump of at least 3 m along the corridor too.
4. **G-BARRACKS's lantern reaches the lever** `[OD-003]`. G-BARRACKS paces between the rack and the lever, so its 3 m lantern sphere overlaps the lever shadow for part of its loop and drains the player during the 2 s channel.
5. **Packages added without a record** `[OD-005]`. See IMPL §1.

---

## Appendix A: Layout values not given in design doc §4 (all PROPOSED)

Nothing in this appendix is built until its `OD-*` row is answered. Coordinates are metres, X east, Z north, origin at the Time SCP Room's south-west corner (§4.1).

### A.1 Walls and openings `[OD-001]`

- Wall thickness 0.2 m, centred on the area boundary lines. Wall height = T-LVL-CEIL (4 m).
- No ceiling geometry (IMPL §6.2 step 9).

| Wall (centreline) | Openings on it (centre, width) |
|---|---|
| Z = 0, X 0–37.5 | – |
| X = 0, Z 0–12 | – |
| X = 37.5, Z 0–12 | – |
| X = 7.5 / 15 / 22.5 / 30, Z 0–6 (room dividers) | – |
| Z = 6, X 0–37.5 | D-SCP-T (3.75, 6) 2 m · D-SCP-A (11.25, 6) 2 m · D-SCP-B (18.75, 6) 2 m · D-SCP-C (26.25, 6) 2 m · D-SCP-D (33.75, 6) 2 m |
| Z = 12, X 0–15.75 and X 21.75–37.5 | Main Corridor mouth X 15.75–21.75 is open (no wall) |
| X = 15.75, Z 12–40 | D-PLAYER (15.75, 26) 2 m, open frame |
| Player Room: Z = 22.25 and Z = 29.75, X 8.25–15.75; X = 8.25, Z 22.25–29.75 | – |
| X = 21.75, Z 12–40 | D-BARRACKS (21.75, 35) 2.5 m |
| Lab: Z = 19.5, X 21.75–32; X = 32, Z 19.5–29.75 | – |
| Z = 29.75, X 21.75–32 | D-LAB (27, 29.75) 2 m |
| X = 32, Z 29.75–40 | D-SLEEP (32, 37.4) 2 m, open; show-only door visual on the alcove's east wall |
| Alcove: Z = 34.85 and Z = 40, X 32–33.75; X = 33.75, Z 34.85–40 | – |
| Z = 40, X 21.75–32 | – |
| X = 15.75 and X = 21.75, Z 40–41.5 (Semi-Final Gate jambs) | gate slot X 15.75–21.75 |
| X = 15.75, Z 41.5–53 and X = 21.75, Z 41.5–53 | side corridor mouths Z 53–56 open |
| West side corridor: Z = 53 and Z = 56, X 9.75–15.75; X = 9.75, Z 53–56 | D-SIDE-W (9.75, 54.5) 2 m, show-only |
| East side corridor: Z = 53 and Z = 56, X 21.75–27.75; X = 27.75, Z 53–56 | D-SIDE-E (27.75, 54.5) 2 m, show-only |
| X = 15.75 and X = 21.75, Z 56–57.5; Z = 57.5, X 15.75–21.75 | Final Gate slot at Z 56 |

Blockers: one invisible `LevelSolid` box in each of D-SCP-A..D (2 m × 0.4 m, full height), as §4.2 requires. No other blockers: the player can only reach floor where shadows exist, and a blocker at D-SLEEP or a side-corridor mouth could trap a player left in a void shadow there after a despawn or a reverse.

### A.2 Props and static shadows `[OD-001]`

| Prop | Centre (X, Z) | Size W × D × H | Shadow rect (X range, Z range) |
|---|---|---|---|
| Crate 1 (Main Corridor) | 16.45, 16 | 1.2 × 1.2 × 1.2 | 15.85–17.65, 15.1–16.9 |
| Crate 2 (Main Corridor) | 16.45, 33 | 1.2 × 1.2 × 1.2 | 15.85–17.65, 32.1–33.9 |
| Cart (Access Corridor) | 6, 8 | 1.4 × 0.8 × 1.0 | 5.1–6.9, 7.2–9.0 |
| Cabinet (Time Room) | 1.0, 5.6 | 1.2 × 0.6 × 2.0 | 0.3–1.9, 4.4–5.9 |
| Pedestal (Time Room) | 3.75, 3 | Ø 1.0 × 1.0 | 2.95–4.55, 2.2–3.8 |
| Lantern rack (Barracks) | 29, 39.6 | 2.0 × 0.5 × 1.8 | 28.0–30.0, 38.6–39.9 |
| Lever panel (Barracks) | 24, 39.7 | 1.2 × 0.4 × 1.6 | 23.0–25.0, 38.3–39.9 |
| Bunk 1 (Barracks) | 23.6, 31.5 | 1.0 × 2.0 × 1.2 | 22.9–24.4, 30.3–32.7 |
| Bunk 2 (Barracks) | 30.2, 31.5 | 1.0 × 2.0 × 1.2 | 29.4–30.9, 30.3–32.7 |
| Locker 1, 2 (Barracks) | 25.2, 30.2 / 28.9, 30.2 | 0.6 × 0.5 × 2.0 | none (dressing only). Moved from 26.0 / 27.9 during B1 so neither blocks D-LAB (X 26–28) |
| Lab bench 1, 2 (SHOULD) | 25, 24 / 29, 24 | 2.0 × 0.8 × 0.9 | 23.9–26.1, 23.3–24.9 / 27.9–30.1, 23.3–24.9 |
| Final Gate shadow | – | – | 15.85–21.65, 54.5–56 |
| Player Room floor shadow | – | – | 8.35–15.65, 22.35–29.65 (whole floor; see `[OD-013]` for the option of ending it 1.5 m short of the door) |

### A.3 Lights `[OD-003]`

| Light | Positions (X, Z) |
|---|---|
| Ceiling lights (harmless, both states) | Access Corridor: Z 9 at X 4, 12, 20, 28, 34 · Main Corridor: X 18.75 at Z 15, 21, 27, 33, 39 · Final Corridor: X 18.75 at Z 44, 49, 54 · one per room centre (Time Room, Barracks, Lab) |
| Alarm lights (Meltdown, 2 m floor radius) | Access Corridor: (8, 8.0) and (30, 11.0) · Main Corridor: (20.5, 14) and (20.5, 30). Each is ≥ 2.6 m (radius + entity radius) from G-TIME's route (A.4) so a clean ride costs nothing (§9 Beat 1) |
| Wall fire lanterns (Meltdown visuals, height 2 m) | West wall X 15.95 and east wall X 21.55, at Z 44, 48.5, 53 |
| Player Room rim light | (12, 26), dim, cool colour |

### A.4 Meltdown routes `[OD-002]`

Meltdown walk speed 2.4 m/s. "W" = waypoint.

| Guard | Spawn | Travel waypoints | Post loop |
|---|---|---|---|
| G-GLANCE | (24.0, 36.0) | (23.0, 35.0) → (17.25, 35.0) → (17.25, 26.0) dwell T-INTRO-GLANCE, look at (12, 26) → (17.25, 11.0) → (11.25, 7.5) | (9.75, 7.5) ↔ (12.75, 7.5), dwell 1 s each |
| G-B | (24.0, 37.0) | (23.0, 35.0) → (20.25, 35.0) → (20.25, 9.0) → (18.75, 7.5) | (17.5, 7.5) ↔ (20.0, 7.5) |
| G-TIME | (29.5, 38.5) | (23.0, 35.0) → (17.25, 35.0) → (17.25, 11.0) → (3.75, 11.0) → (3.75, 5.0) | (3.75, 5.0) → (6.25, 5.0) dwell 4 s, look due north at (6.25, 7) (AD-017) → (6.25, 1.0) → (1.25, 1.0) → (1.25, 5.0) → back to (3.75, 5.0) |
| G-C | (24.0, 38.0) | (23.0, 35.0) → (20.25, 35.0) → (20.25, 9.0) → (26.25, 7.5) | (25.0, 7.5) ↔ (27.5, 7.5) |
| G-D | (25.0, 38.0) | (23.0, 35.0) → (20.25, 35.0) → (20.25, 9.0) → (33.75, 7.5) | (32.5, 7.5) ↔ (35.0, 7.5) |
| G-MAIN | (25.0, 37.0) | (23.0, 35.0) → (19.5, 35.0) | (19.5, 14) ↔ (19.5, 38) |
| G-BARRACKS | (25.0, 37.0), starts on enable | – | (26.0, 37.0) dwell 2.5 s → (24.5, 37.0) dwell 3 s, look at lever (24, 39.7). Loop ≈ 8 s (§9.1) |
| SP-SLEEP-M | D-SLEEP (32.9, 37.4) | (31.0, 37.4) → (23.0, 35.0) → (19.5, 35.0) → (19.5, 14.0) → (19.5, 35.0) → (23.0, 35.0) → (32.9, 37.4), despawn | – |
| SP-FINAL-M | D-SIDE-E (27.0, 54.5) | (18.75, 54.5) | (18.75, 43) ↔ (18.75, 54), scenery |

Checks these values were picked to pass (the tests confirm them):
- G-TIME's travel length ≈ 56 m (§9 Beat 3 says about 55 m).
- G-TIME passes D-PLAYER at X 17.25, 1.5 m from the doorway (§9.1: within 3 m).
- G-TIME's loop: 18 m perimeter / 2.4 m/s + 4 s stop ≈ 11.5 s (§9 Beat 2 says about 12 s). Longest safe window ≈ 5.4 s (AD-017).
- G-TIME's Access Corridor leg at Z 11 keeps it ≥ 3.6 m (lantern radius + entity radius) from G-GLANCE's post at Z 7.5, so a clean ride costs nothing.
- G-TIME's spawn shadow to the lever shadow ≈ 5 m (§9 Beat 3: within jump range). G-BARRACKS's nearest point to G-TIME's spawn ≈ 3.8 m, outside lantern reach.

### A.5 Normal routes `[OD-002]`

Normal walk speed 2 m/s.

| Guard | Spawn | Travel | Post loop |
|---|---|---|---|
| G-ROUTINE-1 | (24.0, 33.0), starts on enable | – | Barracks loop (24.0, 33.0) → bunk 1 side (25.0, 31.5) dwell 3 s → D-LAB (27.0, 30.5) → Lab bench 1 (25.0, 25.5) dwell 3 s → (27.0, 30.5) → back |
| G-ROUTINE-2 | (29.0, 26.0), starts on enable | – | Lab loop (29.0, 26.0) → bench 2 (29.0, 25.5) dwell 3 s → (27.0, 30.5) → bunk 2 side (29.0, 31.5) dwell 3 s → back |
| G-PATROL | (19.5, 14.0), starts on enable | – | (19.5, 14) ↔ (19.5, 38) (§10) |
| SP-SLEEP-N | D-SLEEP (32.9, 37.4) | (31.0, 37.4) → (28.0, 35.0) | uses G-ROUTINE-1's loop |
| SP-SIDE-N (W) | D-SIDE-W (10.5, 54.5) | (17.5, 54.5) → (17.5, 41.5) → (17.5, 38.0) → (17.5, 35.0) → (23.0, 35.0) → (31.0, 37.4) → (32.9, 37.4), despawn | – |
| SP-SIDE-N (E) | D-SIDE-E (27.0, 54.5) | (20.0, 54.5) → (20.0, 41.5) → (20.0, 38.0) → (20.0, 35.0) → (23.0, 35.0) → (31.0, 37.4) → (32.9, 37.4), despawn | – |
| SP-FINAL-N | alternating D-SIDE-W / D-SIDE-E | to (18.75, 54.0) | (18.75, 54) ↔ (18.75, 43); the two patrollers are offset by half a loop by spawning the second one half a loop later (§9 Beat 5) |

### A.6 Final Corridor pipes and drops `[OD-003]`

- Pipes at height 2.5 m on both walls (§4.3). Crack points every 3 m: segments Z 42–45, 45–48, 48–51, 51–54 on each wall (8 total, §7.6).
- Soft shadow strip per segment: 1.0 m wide along the wall. West strips X 15.85–16.85, east strips X 20.65–21.65.
- Drop anchors per segment: 3 drops `pipeDropSpacing` (1.5 m) apart along Z, centred on the segment's middle, at X 17.45 (west) or X 20.05 (east), i.e. "slightly into the corridor".
- Burst point: floor directly below the segment's middle, against the wall.

---

## Appendix B: Traceability (tunables to tests)

Every §13 row and each new `T-*` row that has gameplay effect has at least one test naming it. The test names in this document carry the id without hyphens (for example `_TCOSTGATE`), and each test has a comment with the hyphenated id. `docs/tasks/B{n}.md` lists the tests each task delivers.
