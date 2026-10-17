using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Hold E in a consumable shadow to eat its owner (design doc §7.3, IMPL §13.2).
    public sealed class ConsumeAction : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] ShadowEntity entity;
        [SerializeField] Essence essence;
        [SerializeField] TimePowers timePowers;
        [Tooltip("How far the target sinks below the floor (look only).")]
        [SerializeField] float sinkDepth = 2.2f;

        HoldProgress hold;
        Consumable target;
        GuardController targetGuard;
        Vector3 sinkStart;
        bool waitForRelease; // after a cancel, E must be let go before a new consume starts

        public Consumable Target => target;
        public float Progress => hold != null ? hold.Normalized : 0f;

        // The consumable under the entity, if any (prompt and cost preview read this).
        public Consumable Available =>
            entity.State == EntityState.InShadow && entity.Current != null ? entity.Current.consumable : null;

        void Update()
        {
            if (target != null)
            {
                if (!input.ConsumeHeld) { Cancel(); return; }
                if (hold.Tick(Time.deltaTime)) Complete();
                else Sink(hold.Normalized);
                return;
            }

            Consumable c = Available;
            essence.PreviewCost = c != null && !c.IsGain ? c.Amount : timePowers.AtRewindSpot ? tuning.rewindCost : 0f;
            if (!input.ConsumeHeld) waitForRelease = false;
            if (c == null || !entity.HasControl || !input.ConsumeHeld || waitForRelease) return;
            if (!c.IsGain && !essence.CanAfford(c.Amount)) return; // shown red in the prompt
            Begin(c);
        }

        void Begin(Consumable c)
        {
            target = c;
            hold = new HoldProgress(c.Duration);
            sinkStart = c.sinkRoot != null ? c.sinkRoot.localPosition : Vector3.zero;
            targetGuard = c.GetComponent<GuardController>();
            if (targetGuard != null)
            {
                targetGuard.SetPaused(true);
                targetGuard.IsBeingConsumed = true;
            }
            entity.BeginConsume();
        }

        // The target sinks in the last T-CON-SINK of the duration.
        void Sink(float t)
        {
            if (target.sinkRoot == null) return;
            float s = Mathf.InverseLerp(1f - tuning.consumeSinkFraction, 1f, t);
            target.sinkRoot.localPosition = sinkStart + Vector3.down * (s * sinkDepth);
        }

        void Complete()
        {
            Consumable c = target;
            target = null;
            hold = null;
            essence.PreviewCost = 0f;
            if (c.IsGain) essence.Gain(c.Amount);
            else essence.TrySpend(c.Amount);
            if (c.kind == ConsumableKind.Pedestal) timePowers.Grant(); // the stopwatch is eaten with the pedestal
            entity.EndConsume();
            Destroy(c.gameObject); // its shadow goes too; the entity drops into a void (§7.5)
        }

        // Releasing early, or another guard spotting the consume, cancels at no cost (§7.3).
        public void Cancel()
        {
            if (target == null) return;
            if (target.sinkRoot != null) target.sinkRoot.localPosition = sinkStart;
            if (targetGuard != null)
            {
                targetGuard.IsBeingConsumed = false;
                targetGuard.SetPaused(false);
            }
            target = null;
            targetGuard = null;
            hold = null;
            waitForRelease = true;
            entity.EndConsume();
        }
    }
}
