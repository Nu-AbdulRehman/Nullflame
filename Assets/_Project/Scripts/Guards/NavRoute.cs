using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Nullflame.Logic;

namespace Nullflame
{
    // Threads a waypoint route through the baked NavMesh (AD-020): every leg becomes the NavMesh path's
    // corners, so guards walk round walls and props. Movement stays the deterministic lerp in GuardMotion;
    // the path is computed once, from static geometry. A leg with no complete path is left straight.
    public static class NavRoute
    {
        const float SampleRadius = 1f;
        static readonly NavMeshPath Path = new NavMeshPath();
        static readonly List<RoutePoint> Buffer = new List<RoutePoint>(64);

        public static void Expand(List<RoutePoint> points, bool loop)
        {
            if (points.Count < 2 || !NavMesh.SamplePosition(points[0].Position, out _, SampleRadius, NavMesh.AllAreas)) return;
            Buffer.Clear();
            int legs = loop ? points.Count : points.Count - 1;
            for (int i = 0; i < legs; i++)
            {
                RoutePoint a = points[i], b = points[(i + 1) % points.Count];
                Buffer.Add(a);
                if (!NavMesh.SamplePosition(a.Position, out NavMeshHit ha, SampleRadius, NavMesh.AllAreas)
                    || !NavMesh.SamplePosition(b.Position, out NavMeshHit hb, SampleRadius, NavMesh.AllAreas)
                    || !NavMesh.CalculatePath(ha.position, hb.position, NavMesh.AllAreas, Path)
                    || Path.status != NavMeshPathStatus.PathComplete) continue;
                Vector3[] corners = Path.corners;
                for (int c = 1; c < corners.Length - 1; c++)
                    Buffer.Add(new RoutePoint { Position = new Vector3(corners[c].x, a.Position.y, corners[c].z) });
            }
            if (!loop) Buffer.Add(points[points.Count - 1]);
            points.Clear();
            points.AddRange(Buffer);
        }
    }
}
