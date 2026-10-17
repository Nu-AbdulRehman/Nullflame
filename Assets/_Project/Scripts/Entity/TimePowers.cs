using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Unlocked by eating the Time SCP (design doc §7.4, IMPL §14): local reverse and the facility rewind.
    public sealed class TimePowers : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] ShadowEntity entity;
        [SerializeField] Essence essence;
        [SerializeField] ScreenFx fx;
        [SerializeField] WorldStateController world;

        GuardController reversing;
        GameObject riddenOwner;
        GuardController ridden;
        HoldProgress channel;
        LeverPanel lever;
        bool waitForRelease; // after a channel ends either way, Q must be let go first

        public bool HasTimePower { get; private set; }
        public float ChannelProgress => channel != null ? channel.Normalized : 0f;

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

        // In the lever's shadow, in Meltdown, with the power (cost is checked separately for the red prompt).
        public bool AtRewindSpot => HasTimePower && world.State == WorldState.Meltdown
                                    && entity.State == EntityState.InShadow && entity.Current != null && entity.Current.isRewindSpot;

        public void Grant() => HasTimePower = true;

        void Update()
        {
            if (!HasTimePower) return;
            TickRewind();
            if (channel != null) return;
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

        // Facility rewind: hold Q for T-REWIND-CHANNEL in the lever's shadow; costs T-COST-REWIND on completion.
        void TickRewind()
        {
            if (!input.RewindHeld) waitForRelease = false;
            if (channel == null)
            {
                if (!input.RewindHeld || waitForRelease || !AtRewindSpot || !essence.CanAfford(tuning.rewindCost)) return;
                channel = new HoldProgress(tuning.rewindChannelTime);
                lever = entity.Current.owner != null ? entity.Current.owner.GetComponent<LeverPanel>() : null;
                entity.BeginConsume(); // guards treat the channel as consuming (§7.4)
                fx.SetTimeEffect(true);
                return;
            }
            if (!input.RewindHeld || world.State != WorldState.Meltdown) { CancelChannel(); return; }
            bool done = channel.Tick(Time.deltaTime);
            if (lever != null) lever.SetProgress(channel.Normalized);
            if (!done) return;
            channel = null;
            waitForRelease = true;
            essence.TrySpend(tuning.rewindCost);
            entity.EndConsume();
            fx.SetTimeEffect(false);
            world.Rewind();
        }

        // Releasing Q, or a guard seeing the channel, cancels it at no cost.
        public void CancelChannel()
        {
            if (channel == null) return;
            channel = null;
            waitForRelease = true;
            if (lever != null) lever.SetProgress(0f);
            entity.EndConsume();
            fx.SetTimeEffect(false);
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
