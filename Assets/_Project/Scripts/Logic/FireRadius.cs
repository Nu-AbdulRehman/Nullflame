using UnityEngine;

namespace Nullflame.Logic
{
    // Lantern fire size over time (design doc §7.9, IMPL §4.5, AD-011): grow start -> max, burn at max, shrink to 0.
    public sealed class FireRadius
    {
        readonly float start, max, grow, life, shrink;
        float t;

        public FireRadius(float start, float max, float grow, float life, float shrink)
        {
            this.start = start;
            this.max = max;
            this.grow = grow;
            this.life = life;
            this.shrink = shrink;
        }

        public float Radius
        {
            get
            {
                if (t < grow) return Mathf.Lerp(start, max, t / grow);
                if (t < grow + life) return max;
                if (t < grow + life + shrink) return Mathf.Lerp(max, 0f, (t - grow - life) / shrink);
                return 0f;
            }
        }

        public bool Done => t >= grow + life + shrink;

        public void Tick(float dt) => t += dt;
    }
}
