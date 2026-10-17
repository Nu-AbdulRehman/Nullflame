using System.Collections.Generic;
using UnityEngine;

namespace Nullflame
{
    // Every active ShadowArea and the queries on them (IMPL §7.2). Guards and fires join in later steps.
    public sealed class ShadowRegistry : MonoBehaviour
    {
        readonly List<ShadowArea> shadows = new List<ShadowArea>();

        public IReadOnlyList<ShadowArea> Shadows => shadows;

        public void Register(ShadowArea s) => shadows.Add(s);

        public void Unregister(ShadowArea s) => shadows.Remove(s);

        // Smallest shadow containing point, so an oil drop on a pipe strip or a guard shadow over the
        // Player Room floor wins over the larger one.
        public ShadowArea FindAt(Vector3 point, ShadowArea exclude)
        {
            ShadowArea best = null;
            for (int i = 0; i < shadows.Count; i++)
            {
                ShadowArea s = shadows[i];
                if (s == exclude || !s.Contains(point)) continue;
                if (best == null || s.Area < best.Area) best = s;
            }
            return best;
        }

        public ShadowArea FindNearest(Vector3 point, float maxDistance, ShadowArea exclude)
        {
            ShadowArea best = null;
            float bestSqr = maxDistance * maxDistance;
            for (int i = 0; i < shadows.Count; i++)
            {
                ShadowArea s = shadows[i];
                if (s == exclude) continue;
                Vector3 d = s.ClosestPoint(point) - point;
                d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr <= bestSqr) { bestSqr = sqr; best = s; }
            }
            return best;
        }
    }
}
