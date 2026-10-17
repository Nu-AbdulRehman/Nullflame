namespace Nullflame.Logic
{
    // Spotting needs the condition to hold continuously for the grace time (design doc §8.3, IMPL §4.3).
    public sealed class SpotTimer
    {
        readonly float grace;
        float held;
        bool fired;

        public SpotTimer(float grace) => this.grace = grace;

        // True only on the frame the condition has held for >= grace. Fires once until Reset or the condition breaks.
        public bool Tick(bool conditionHolds, float dt)
        {
            if (!conditionHolds)
            {
                Reset();
                return false;
            }
            held += dt;
            if (fired || held < grace) return false;
            fired = true;
            return true;
        }

        public void Reset()
        {
            held = 0f;
            fired = false;
        }
    }
}
