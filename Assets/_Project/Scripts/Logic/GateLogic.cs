namespace Nullflame.Logic
{
    // Guard-only gate state (design doc §7.8, IMPL §4.10): open while any guard is in the zone,
    // closes closeDelay after the last one leaves.
    public sealed class GateLogic
    {
        readonly float closeDelay;
        float closeTimer;

        public GateLogic(float closeDelay) => this.closeDelay = closeDelay;

        public bool IsOpen { get; private set; }

        public void Tick(int guardsInZone, float dt)
        {
            if (guardsInZone > 0)
            {
                IsOpen = true;
                closeTimer = closeDelay;
                return;
            }
            if (!IsOpen) return;
            closeTimer -= dt;
            if (closeTimer <= 0f) IsOpen = false;
        }
    }
}
