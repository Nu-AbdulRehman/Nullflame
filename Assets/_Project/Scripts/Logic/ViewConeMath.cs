using UnityEngine;

namespace Nullflame.Logic
{
    // Flat (XZ) view-cone test (design doc §8.3, IMPL §4.4). Line of sight is checked by the caller.
    public static class ViewConeMath
    {
        public static bool Contains(Vector3 eye, Vector3 forwardXZ, float totalAngleDeg, float range, Vector3 point)
        {
            Vector3 d = point - eye;
            d.y = 0f;
            float sqr = d.sqrMagnitude;
            if (sqr > range * range) return false;
            if (sqr < 1e-6f) return true;
            forwardXZ.y = 0f;
            return Vector3.Angle(forwardXZ, d) <= totalAngleDeg * 0.5f;
        }
    }
}
