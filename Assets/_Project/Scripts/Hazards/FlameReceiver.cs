using System.Collections.Generic;
using UnityEngine;

namespace Nullflame
{
    // On the Player: applies flame damage from overlapping FlameVolumes (IMPL §11.2).
    [DefaultExecutionOrder(20)]
    public sealed class FlameReceiver : MonoBehaviour
    {
        [SerializeField] ShadowEntity entity;
        [SerializeField] Essence essence;
        [SerializeField] GameTuning tuning;

        readonly List<FlameVolume> touching = new List<FlameVolume>(16);
        int solidMask;

        void Awake() => solidMask = LayerMask.GetMask("LevelSolid");

        public bool IsInFlame { get; private set; }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out FlameVolume f) && !touching.Contains(f)) touching.Add(f);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out FlameVolume f)) touching.Remove(f);
        }

        void Update()
        {
            IsInFlame = false;
            // Flames never touch an airborne entity (AD-011).
            if (entity.State != EntityState.InShadow && entity.State != EntityState.Consuming) return;
            // Any shadow is cover from flame light (lanterns, alarms); fire still burns (AD-018, AD-028).
            bool covered = entity.Current != null;

            float dps = 0f;
            for (int i = touching.Count - 1; i >= 0; i--)
            {
                FlameVolume f = touching[i];
                if (f == null) { touching.RemoveAt(i); continue; }
                if (!f.Lit || IsOwnLantern(f)) continue;
                if (f.IsLight && (covered || Blocked(f))) continue; // light stops at walls and opaque cover
                if (f.InstantKill)
                {
                    essence.Kill();
                    return;
                }
                dps += f.DamagePerSecond;
            }
            // A soft shadow (pipe strip) is thin cover: a slow drain until oil drops give opaque cover (AD-028).
            if (entity.Current != null && entity.Current.opacity == ShadowOpacity.Soft) dps += tuning.softShadowDps;
            if (dps <= 0f) return;
            IsInFlame = true;
            essence.Damage(dps * Time.deltaTime);
        }

        // Light from a lantern or alarm lamp reaches the entity only with a clear line to it.
        bool Blocked(FlameVolume f)
        {
            Vector3 target = entity.transform.position + Vector3.up * 0.3f;
            return Physics.Linecast(f.transform.position, target, solidMask, QueryTriggerInteraction.Ignore);
        }

        // A guard's own lantern never hurts the shadow it casts (design doc §7.2, O1).
        bool IsOwnLantern(FlameVolume f) =>
            f.kind == FlameKind.Lantern && f.sourceGuard != null && entity.Current != null
            && entity.Current.owner == f.sourceGuard.gameObject;
    }
}
