using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Unlocked by eating the Time SCP (design doc §7.4, IMPL §14). B6: local reverse. Rewind arrives in B7.
    public sealed class TimePowers : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] ShadowEntity entity;
        [SerializeField] Essence essence;
        [SerializeField] ScreenFx fx;

        GuardController reversing;
        GameObject riddenOwner;
        GuardController ridden;

        public bool HasTimePower { get; private set; }

        // The guard whose shadow the entity rides, if any (cached by shadow owner).
        public GuardController RiddenGuard
        {
            get
            {
                GameObject owner = entity.IsRiding && entity.State == EntityState.InShadow ? entity.Current.owner : null;
                if (owner != riddenOwner)
                {
                    riddenOwner = owner;
                    ridden = owner != null ? owner.GetComponent<GuardController>() : null;
                }
                return ridden;
            }
        }

        public bool CanReverse => HasTimePower && RiddenGuard != null && ridden.Motion.HistoryCount > 0
                                  && essence.Value > tuning.essenceFloor;

        public void Grant() => HasTimePower = true;

        void Update()
        {
            if (!HasTimePower) return;
            if (reversing == null)
            {
                if (input.ReverseHeld && CanReverse) Begin(ridden);
                return;
            }
            // Stops on release, on hopping off, at the end of history, or at T-ESS-FLOOR (§7.4).
            bool keep = input.ReverseHeld && RiddenGuard == reversing
                        && reversing.Motion.State == GuardMoveState.Reversing
                        && essence.SpendHeld(tuning.reverseCostPerSecond * Time.deltaTime);
            if (!keep) Stop();
        }

        void Begin(GuardController g)
        {
            reversing = g;
            g.BeginReverse();
            fx.SetTimeEffect(true);
        }

        void Stop()
        {
            reversing.EndReverse(); // the guard holds where it is
            reversing = null;
            fx.SetTimeEffect(false);
        }

        void OnDisable()
        {
            if (reversing != null) Stop();
        }
    }
}
