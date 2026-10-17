namespace Nullflame.Logic
{
    // Hold-to-complete timer for consumes and the rewind channel (IMPL §4.7).
    public sealed class HoldProgress
    {
        readonly float duration;
        float elapsed;
        bool done;

        public HoldProgress(float duration) => this.duration = duration;

        public float Normalized => duration <= 0f ? 1f : elapsed / duration;

        // True only on the completing frame.
        public bool Tick(float dt)
        {
            if (done) return false;
            elapsed += dt;
            if (elapsed < duration) return false;
            elapsed = duration;
            done = true;
            return true;
        }

        public void Cancel()
        {
            elapsed = 0f;
            done = false;
        }
    }
}
