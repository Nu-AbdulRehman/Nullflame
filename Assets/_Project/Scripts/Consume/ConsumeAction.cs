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
        [SerializeField] GameFlow flow;
        [Tooltip("How far the target sinks below the floor (look only).")]
        [SerializeField] float sinkDepth = 2.2f;

        HoldProgress hold;
        Consumable target;
        GuardController targetGuard;
        Vector3 sinkStart;
        bool waitForRelease; // after a cancel, E must be let go before a new consume starts
        CrackablePipe pipe;
        PipeCrackMeter meter;

        public Consumable Target => target;
        public CrackablePipe Pipe => pipe;
        public float Progress => hold != null ? hold.Normalized : meter != null && pipe != null ? meter.Normalized : 0f;
        public PipeZone Zone => meter != null ? meter.Zone : PipeZone.Grey;

        // The consumable under the entity, if any (prompt and cost preview read this).
        public Consumable Available =>
            entity.State == EntityState.InShadow && entity.Current != null ? entity.Current.consumable : null;

        // The uncracked pipe under the entity, if any.
        public CrackablePipe AvailablePipe =>
            entity.State == EntityState.InShadow && entity.Current != null && entity.Current.pipe != null
            && !entity.Current.pipe.Cracked ? entity.Current.pipe : null;

        void Update()
        {
            if (target != null)
            {
                if (!input.ConsumeHeld) { Cancel(); return; }
                if (hold.Tick(Time.deltaTime)) Complete();
                else Sink(hold.Normalized);
                return;
            }
            if (pipe != null) { TickPipe(); return; }

            Consumable c = Available;
            CrackablePipe p = AvailablePipe;
            essence.PreviewCost = c != null && !c.IsGain ? c.Amount : p != null ? tuning.pipeCost
                : timePowers.AtRewindSpot ? tuning.rewindCost : 0f;
            if (!input.ConsumeHeld) waitForRelease = false;
            if (!entity.HasControl || !input.ConsumeHeld || waitForRelease) return;
            if (p != null && essence.CanAfford(tuning.pipeCost)) { pipe = p; return; }
            if (c == null) return;
            if (!c.IsGain && !essence.CanAfford(c.Amount)) return; // shown red in the prompt
            Begin(c);
        }

        // Hold E on a pipe (§7.6): release in safe or warning cracks; holding to T-PIPE-BURST bursts.
        // Leaving the strip drops the hold with no effect.
        void TickPipe()
        {
            meter ??= new PipeCrackMeter(tuning.pipeSafeStart, tuning.pipeSafeEnd, tuning.pipeBurstTime);
            if (entity.Current != pipe.strip || entity.State != EntityState.InShadow) { EndPipe(); meter.Release(); return; }
            if (input.ConsumeHeld)
            {
                if (meter.Hold(Time.deltaTime) != PipeResult.Burst) return;
                essence.TrySpend(tuning.pipeBurstCost); // AD-014
                pipe.Burst();
                EndPipe();
                return;
            }
            if (meter.Release() == PipeResult.Crack && essence.TrySpend(tuning.pipeCost)) pipe.Crack();
            EndPipe();
        }

        void EndPipe()
        {
            pipe = null;
            waitForRelease = true;
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
            SfxPlayer.Play(c.kind == ConsumableKind.Pedestal ? SfxId.PowerGranted : SfxId.ConsumeDone);
            if (c.kind == ConsumableKind.Guard) SfxPlayer.Play(SfxId.Scream);
            if (c.kind == ConsumableKind.Pedestal) timePowers.Grant(); // the stopwatch is eaten with the pedestal
            else if (c.kind == ConsumableKind.FinalGate) flow.Win(); // Beat 5
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
