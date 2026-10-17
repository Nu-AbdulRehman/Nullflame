using System.Collections.Generic;
using UnityEngine;

namespace Nullflame
{
    // Every active ShadowArea, guard and fire, and the queries on them (IMPL §7.2).
    public sealed class ShadowRegistry : MonoBehaviour
    {
        readonly List<ShadowArea> shadows = new List<ShadowArea>();
        readonly List<LanternFire> fires = new List<LanternFire>();
        readonly List<GuardController> guards = new List<GuardController>();

        public IReadOnlyList<ShadowArea> Shadows => shadows;

        public void Register(ShadowArea s) => shadows.Add(s);

        public void Unregister(ShadowArea s) => shadows.Remove(s);
        public IReadOnlyList<GuardController> Guards => guards;
        public void Register(GuardController g) => guards.Add(g);
        public void Unregister(GuardController g) => guards.Remove(g);
        public void Register(LanternFire f) => fires.Add(f);
        public void Unregister(LanternFire f) => fires.Remove(f);

        // A burning fire blocks jumps that land inside it (design doc §7.9 rule 5).
        public bool IsInsideAnyFire(Vector3 point, float extraRadius)
        {
            for (int i = 0; i < fires.Count; i++)
                if (fires[i].Contains(point, extraRadius)) return true;
            return false;
        }

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
