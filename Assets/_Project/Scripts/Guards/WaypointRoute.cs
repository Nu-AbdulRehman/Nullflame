using System.Collections.Generic;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // A route is a parent whose children are waypoints, in order (IMPL §10.1). Drag children to edit.
    public sealed class WaypointRoute : MonoBehaviour
    {
        [Tooltip("Post loops close back to the first point; travel routes don't.")]
        public bool loop;

        public void ToRoutePoints(List<RoutePoint> buffer)
        {
            buffer.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);
                var p = new RoutePoint { Position = t.position };
                if (t.TryGetComponent(out Waypoint w))
                {
                    p.Dwell = w.dwell;
                    p.HasLook = w.hasLook;
                    p.LookAt = w.lookAt;
                }
                buffer.Add(p);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = loop ? Color.green : Color.yellow;
            int n = transform.childCount;
            for (int i = 0; i < n; i++)
            {
                Vector3 a = transform.GetChild(i).position;
                Gizmos.DrawSphere(a, 0.15f);
                if (i + 1 < n) Gizmos.DrawLine(a, transform.GetChild(i + 1).position);
                else if (loop && n > 1) Gizmos.DrawLine(a, transform.GetChild(0).position);
            }
        }
    }
}
