using UnityEngine;

namespace Nullflame
{
    public enum SpawnerId { SleepMeltdown, SleepNormal, SideNormal, FinalNormal, FinalMeltdown }

    [System.Serializable]
    public struct SpawnerTuning
    {
        public float interval;
        public int maxAlive;
        public bool refillOnly;
    }

    // Every gameplay number lives here (design doc §13, IMPL §3). Tooltips start with the T-* id.
    [CreateAssetMenu(menuName = "Nullflame/Game Tuning", fileName = "GameTuning")]
    public sealed class GameTuning : ScriptableObject
    {
        [Header("Level")]
        [Tooltip("T-LVL-CEIL: ceiling height (m)")] public float ceilingHeight = 4f;

        [Header("Entity")]
        [Tooltip("T-PLY-SIZE: entity decal diameter (m)")] public float entitySize = 1.2f;
        [Tooltip("T-PLY-SLIDE: slide speed inside a shadow (m/s)")] public float slideSpeed = 2.5f;

        [Header("Jump")]
        [Tooltip("T-JMP-RANGE: max jump distance (m)")] public float jumpRange = 6f;
        [Tooltip("T-JMP-ARC: jump apex height (m)")] public float jumpArcHeight = 1.5f;
        [Tooltip("T-JMP-TIME: jump duration (s)")] public float jumpTime = 0.45f;
        [Tooltip("T-JMP-SNAP: reticle snap distance to the nearest shadow (m)")] public float reticleSnapDistance = 2f;
        [Tooltip("T-JMP-RAY-Y: height of the jump line-of-sight raycast (m)")] public float jumpRayHeight = 0.5f;

        [Header("Essence")]
        [Tooltip("T-ESS-MAX: Essence max")] public float essenceMax = 100f;
        [Tooltip("T-ESS-START: Essence at new game")] public float essenceStart = 100f;
        [Tooltip("T-ESS-INTRO: intro graze damage")] public float introGrazeDamage = 10f;
        [Tooltip("T-ESS-FLOOR: held time powers stop here")] public float essenceFloor = 10f;
        [Tooltip("T-ESS-LOW: bar pulses and heartbeat plays below this")] public float essenceLowThreshold = 25f;

        [Header("Flame")]
        [Tooltip("T-DMG-LANTERN: guard lantern drain (per s)")] public float lanternDps = 8f;
        [Tooltip("T-DMG-ALARM: alarm light drain (per s)")] public float alarmDps = 5f;
        [Tooltip("T-DMG-FIRE: lantern fire drain (per s), AD-022")] public float fireDps = 15f;
        [Tooltip("T-DMG-FLOOR: lantern or alarm light drain while on bare floor (per s), AD-023")] public float floorLightDps = 40f;
        [Tooltip("T-LANTERN-RADIUS: guard lantern damage radius (m)")] public float lanternRadius = 3f;
        [Tooltip("T-ALARM-RADIUS: alarm light floor radius (m)")] public float alarmFloorRadius = 2f;

        [Header("Lantern throw and fire")]
        [Tooltip("T-THROW-TIME: lantern flight time (s)")] public float throwTime = 0.6f;
        [Tooltip("T-FIRE-START: fire radius at impact (m)")] public float fireStartRadius = 1f;
        [Tooltip("T-FIRE-RADIUS: lantern fire max radius (m)")] public float fireMaxRadius = 4f;
        [Tooltip("T-FIRE-RADIUS-OIL: fire max radius at a pipe burst (m)")] public float fireOilRadius = 5f;
        [Tooltip("T-FIRE-GROW: fire growth time to max radius (s)")] public float fireGrowTime = 2f;
        [Tooltip("T-FIRE-LIFE: fire burn time at max radius (s)")] public float fireLifeTime = 6f;
        [Tooltip("T-FIRE-SHRINK: fire shrink-out time (s)")] public float fireShrinkTime = 1f;
        [Tooltip("T-LANTERN-RELIGHT: time a guard is lanternless after throwing (s)")] public float lanternRelightTime = 10f;

        [Header("Costs and gains")]
        [Tooltip("T-COST-PIPE: pipe crack cost")] public float pipeCost = 5f;
        [Tooltip("T-PIPE-BURST-COST: pipe burst cost")] public float pipeBurstCost = 5f;
        [Tooltip("T-COST-PEDESTAL: pedestal + Time SCP cost")] public float pedestalCost = 20f;
        [Tooltip("T-COST-GATE: Final Gate cost")] public float gateCost = 80f;
        [Tooltip("T-COST-REVERSE: local reverse cost (per s)")] public float reverseCostPerSecond = 5f;
        [Tooltip("T-COST-REWIND: facility rewind cost")] public float rewindCost = 30f;
        [Tooltip("T-GAIN-GUARD: guard consume gain")] public float guardGain = 25f;

        [Header("Consume")]
        [Tooltip("T-CON-GUARD: guard consume time (s)")] public float guardConsumeTime = 2f;
        [Tooltip("T-CON-PEDESTAL: pedestal consume time (s)")] public float pedestalConsumeTime = 3f;
        [Tooltip("T-CON-GATE: gate consume time (s)")] public float gateConsumeTime = 5f;
        [Tooltip("T-CON-SINK: fraction of the consume at the end during which the target sinks")] public float consumeSinkFraction = 0.4f;

        [Header("Time powers")]
        [Tooltip("T-REWIND-CHANNEL: facility rewind hold time (s)")] public float rewindChannelTime = 2f;
        [Tooltip("T-REWIND-FX: rewind screen effect duration (s)")] public float rewindEffectTime = 1.5f;
        [Tooltip("T-REV-SPEED: reverse playback speed as a multiple of the guard's walk speed")] public float reverseSpeedMultiplier = 2f;

        [Header("Pipes")]
        [Tooltip("T-PIPE-BURST: pipe hold time to burst (s)")] public float pipeBurstTime = 1.5f;
        [Tooltip("T-PIPE-SAFE-START: safe zone start (s)")] public float pipeSafeStart = 0.6f;
        [Tooltip("T-PIPE-SAFE-END: safe zone end (s)")] public float pipeSafeEnd = 1.1f;
        [Tooltip("T-PIPE-DROPS: oil drops per crack")] public int pipeDropCount = 3;
        [Tooltip("T-PIPE-DROP-TIME: time over which the drops appear (s)")] public float pipeDropDuration = 2f;
        [Tooltip("T-PIPE-DROP-SPACING: spacing between drops (m)")] public float pipeDropSpacing = 1.5f;

        [Header("Shadow sizes")]
        [Tooltip("T-SHD-OIL: oil drop shadow size (m)")] public float oilDropSize = 1.2f;
        [Tooltip("T-SHD-VOID: void shadow size (m)")] public float voidShadowSize = 1.5f;
        [Tooltip("T-SHD-GUARD-OFFSET: guard shadow offset behind the guard (m)")] public float guardShadowOffset = 0.6f;
        [Tooltip("T-SHD-GUARD-W: guard shadow width across the guard (m)")] public float guardShadowWidth = 1.5f;
        [Tooltip("T-SHD-GUARD-D: guard shadow depth along the guard (m)")] public float guardShadowDepth = 1f;

        [Header("Guards")]
        [Tooltip("T-GRD-WALK: guard walk speed, Normal state (m/s)")] public float guardWalkSpeedNormal = 2f;
        [Tooltip("T-GRD-WALK-MELT: guard walk speed, Meltdown state (m/s)")] public float guardWalkSpeedMeltdown = 2.4f;
        [Tooltip("T-GRD-ALERT: alert time before the throw (s)")] public float alertTime = 0.6f;
        [Tooltip("T-GRD-HISTORY: history sample step while travelling (s)")] public float historyStep = 0.25f;

        [Header("Vision")]
        [Tooltip("T-VIEW-ANGLE: view cone total angle (degrees)")] public float viewAngle = 70f;
        [Tooltip("T-VIEW-RANGE: view cone range (m)")] public float viewRange = 8f;
        [Tooltip("T-VIEW-GRACE: time in view before spotted (s)")] public float viewGrace = 0.2f;
        [Tooltip("T-VIEW-EYE: guard eye height (m)")] public float viewEyeHeight = 1.6f;

        [Header("Semi-Final Gate")]
        [Tooltip("T-GATE-ZONE: trigger zone depth on each side (m)")] public float gateTriggerDepth = 2f;
        [Tooltip("T-GATE-CLOSE: close delay after the last guard leaves (s)")] public float gateCloseDelay = 1.5f;
        [Tooltip("T-GATE-SLIDE: panel slide time, visual only (s)")] public float gatePanelSlideTime = 0.5f;

        [Header("Intro and flow")]
        [Tooltip("T-INTRO-WAVE: spacing between wave guards (s)")] public float introWaveSpacing = 3f;
        [Tooltip("T-INTRO-GLANCE: G-GLANCE pause at D-PLAYER (s)")] public float introGlanceTime = 2f;
        [Tooltip("T-INTRO-CAM: intro camera pull-back time (s)")] public float introCameraPullTime = 1.5f;
        [Tooltip("T-FAIL-RESTART: auto restart delay on death (s)")] public float autoRestartDelay = 3f;

        [Header("Camera")]
        [Tooltip("T-CAM-DIST: camera distance (m)")] public float cameraDistance = 6f;
        [Tooltip("T-CAM-PITCH-MIN: min pitch (degrees)")] public float cameraPitchMin = 5f;
        [Tooltip("T-CAM-PITCH-MAX: max pitch (degrees)")] public float cameraPitchMax = 80f;
        [Tooltip("T-CAM-PITCH-START: start pitch (degrees)")] public float cameraPitchStart = 45f;
        [Tooltip("T-CAM-SENS: mouse sensitivity (degrees per count)")] public float mouseSensitivity = 0.12f;
        [Tooltip("T-CAM-DAMP: follow damping (s)")] public float cameraFollowDamping = 0.12f;
        [Tooltip("T-CAM-RADIUS: collision spherecast radius (m)")] public float cameraCollisionRadius = 0.3f;

        [Header("Spawners")]
        [Tooltip("T-SPN-SLEEP-M")] public SpawnerTuning spawnSleepMeltdown = new SpawnerTuning { interval = 20f, maxAlive = 1 };
        [Tooltip("T-SPN-SLEEP-N: max counts the routine guard")] public SpawnerTuning spawnSleepNormal = new SpawnerTuning { interval = 25f, maxAlive = 2 };
        [Tooltip("T-SPN-SIDE-N: max counts guards in transit")] public SpawnerTuning spawnSideNormal = new SpawnerTuning { interval = 15f, maxAlive = 1 };
        [Tooltip("T-SPN-FINAL-N: interval is the refill delay")] public SpawnerTuning spawnFinalNormal = new SpawnerTuning { interval = 10f, maxAlive = 1, refillOnly = true };
        [Tooltip("T-SPN-FINAL-M: no respawn")] public SpawnerTuning spawnFinalMeltdown = new SpawnerTuning { interval = 0f, maxAlive = 1, refillOnly = true };

        public SpawnerTuning GetSpawner(SpawnerId id)
        {
            switch (id)
            {
                case SpawnerId.SleepMeltdown: return spawnSleepMeltdown;
                case SpawnerId.SleepNormal: return spawnSleepNormal;
                case SpawnerId.SideNormal: return spawnSideNormal;
                case SpawnerId.FinalNormal: return spawnFinalNormal;
                default: return spawnFinalMeltdown;
            }
        }
    }
}
