namespace Nullflame.Logic
{
    // When a guard spawner should spawn (design doc §8.4, IMPL §4.11).
    // Interval mode: one spawn every `interval` seconds while below the cap (the clock waits at the cap).
    // Refill mode: fills to the cap at start (`initialSpacing` apart), then one spawn `interval` seconds after
    // the count drops below the cap; interval <= 0 means never refill.
    public sealed class SpawnTimer
    {
        readonly float interval, initialSpacing;
        readonly int maxAlive;
        readonly bool refillOnly;
        float timer;
        float spacingTimer;
        bool filled;
        bool spawnedAny;

        public SpawnTimer(float interval, int maxAlive, bool refillOnly, float initialSpacing = 0f)
        {
            this.interval = interval;
            this.maxAlive = maxAlive;
            this.refillOnly = refillOnly;
            this.initialSpacing = initialSpacing;
        }

        public bool Tick(float dt, int aliveCount)
        {
            if (aliveCount >= maxAlive)
            {
                filled = true;
                timer = 0f;
                return false;
            }
            if (refillOnly && !filled)
            {
                spacingTimer += dt;
                if (spawnedAny && spacingTimer < initialSpacing) return false;
                spacingTimer = 0f;
                spawnedAny = true;
                if (aliveCount + 1 >= maxAlive) filled = true;
                return true;
            }
            if (refillOnly && interval <= 0f) return false;
            timer += dt;
            if (timer < interval) return false;
            timer = 0f;
            return true;
        }
    }
}
