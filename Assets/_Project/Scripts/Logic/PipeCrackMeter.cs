namespace Nullflame.Logic
{
    public enum PipeZone { Grey, Safe, Warning, Burst }
    public enum PipeResult { None, Crack, Burst }

    // Hold-E meter on an oil pipe (design doc §7.6, IMPL §4.6): grey, green safe zone, red warning, burst.
    public sealed class PipeCrackMeter
    {
        readonly float safeStart, safeEnd, burstTime;

        public PipeCrackMeter(float safeStart, float safeEnd, float burstTime)
        {
            this.safeStart = safeStart;
            this.safeEnd = safeEnd;
            this.burstTime = burstTime;
        }

        public float Elapsed { get; private set; }
        public float Normalized => Elapsed / burstTime;

        public PipeZone Zone =>
            Elapsed >= burstTime ? PipeZone.Burst
            : Elapsed >= safeEnd ? PipeZone.Warning
            : Elapsed >= safeStart ? PipeZone.Safe
            : PipeZone.Grey;

        // Burst on the frame Elapsed reaches burstTime; the meter resets.
        public PipeResult Hold(float dt)
        {
            Elapsed += dt;
            if (Elapsed < burstTime) return PipeResult.None;
            Elapsed = 0f;
            return PipeResult.Burst;
        }

        // Grey does nothing; safe and warning both crack (§7.6). Resets the meter.
        public PipeResult Release()
        {
            PipeResult r = Zone == PipeZone.Grey ? PipeResult.None : PipeResult.Crack;
            Elapsed = 0f;
            return r;
        }
    }
}
