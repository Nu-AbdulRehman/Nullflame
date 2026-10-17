using System;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Spots the entity when it is in the cone with line of sight AND acting (jumping, consuming,
    // or in a soft shadow), continuously for the grace time (design doc §8.3, IMPL §10.4).
    [DefaultExecutionOrder(10)]
    public sealed class GuardVision : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] GuardController guard;

        public event Action<GuardVision> Spotted;

        SpotTimer timer;
        int solidMask;

        void Awake()
        {
            timer = new SpotTimer(tuning.viewGrace);
            solidMask = LayerMask.GetMask("LevelSolid");
        }

        void Update()
        {
            ShadowEntity entity = GameContext.Current != null ? GameContext.Current.entity : null;
            bool sees = entity != null && guard.HasLantern && !guard.IsBeingConsumed && CanSee(entity);
            if (!timer.Tick(sees, Time.deltaTime)) return;
            GameContext.Current.consume.Cancel(); // a spotted consume or rewind channel is cancelled at no cost (§7.3, §7.4)
            GameContext.Current.timePowers.CancelChannel();
            Spotted?.Invoke(this);
        }

        bool CanSee(ShadowEntity e)
        {
            // The ridden guard never notices its rider (design doc §7.2).
            if (e.Current != null && e.Current.owner == guard.gameObject) return false;
            bool acting = e.State == EntityState.Jumping || e.State == EntityState.Consuming
                || (e.State == EntityState.InShadow && e.Current != null && e.Current.opacity == ShadowOpacity.Soft);
            if (!acting) return false;

            Vector3 eye = guard.transform.position + Vector3.up * tuning.viewEyeHeight;
            Vector3 target = e.transform.position;
            if (!ViewConeMath.Contains(eye, guard.transform.forward, tuning.viewAngle, tuning.viewRange, target)) return false;
            target.y = Mathf.Max(target.y, tuning.jumpRayHeight);
            return !Physics.Linecast(eye, target, solidMask, QueryTriggerInteraction.Ignore);
        }
    }
}
