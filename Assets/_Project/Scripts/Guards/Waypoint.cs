using UnityEngine;

namespace Nullflame
{
    // Optional per-waypoint data: a pause, and a point to face during it (IMPL §10.1). Level data, not tuning.
    public sealed class Waypoint : MonoBehaviour
    {
        [Tooltip("Seconds to stand at this point.")]
        public float dwell;
        public bool hasLook;
        [Tooltip("World point to face while dwelling.")]
        public Vector3 lookAt;

        void OnDrawGizmosSelected()
        {
            if (!hasLook) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, lookAt);
        }
    }
}
