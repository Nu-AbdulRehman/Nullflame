using UnityEngine;

namespace Nullflame
{
    public enum FlameKind { Lantern, Alarm, WallFire, LanternFire }

    // A flame-light damage trigger (design doc §7.7, IMPL §11.1). The Light is only the look;
    // this trigger decides damage. Disabling the object turns both off.
    [RequireComponent(typeof(Collider))]
    public sealed class FlameVolume : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        public FlameKind kind;
        [Tooltip("Lantern only: the guard carrying it. Its lantern never hurts the shadow it casts.")]
        public GuardController sourceGuard;

        Collider col;

        // Damage shapes follow the tunables, never a hand-typed collider size.
        void Awake()
        {
            col = GetComponent<Collider>();
            if (kind == FlameKind.Lantern && col is SphereCollider sphere) sphere.radius = tuning.lanternRadius;
            if (kind == FlameKind.Alarm && col is CapsuleCollider capsule) capsule.radius = tuning.alarmFloorRadius;
        }

        public bool Lit => isActiveAndEnabled && col.enabled;
        public bool InstantKill => kind == FlameKind.WallFire;
        // Lantern and alarm light: blocked by walls and by an opaque shadow (EN-048, AD-018). Fire is not light.
        public bool IsLight => kind == FlameKind.Lantern || kind == FlameKind.Alarm;

        // On bare floor, light drains T-DMG-FLOOR instead (AD-023).
        public float FloorDamagePerSecond => tuning.floorLightDps;

        public float DamagePerSecond =>
            kind == FlameKind.Lantern ? tuning.lanternDps : kind == FlameKind.Alarm ? tuning.alarmDps
            : kind == FlameKind.LanternFire ? tuning.fireDps : 0f;
    }
}
