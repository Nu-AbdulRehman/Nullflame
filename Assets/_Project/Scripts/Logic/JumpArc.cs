using UnityEngine;

namespace Nullflame.Logic
{
    // Parabolic jump (design doc §5.3, IMPL §4.8). The caller re-reads `to` each frame to home on moving shadows.
    public static class JumpArc
    {
        public static Vector3 Evaluate(Vector3 from, Vector3 to, float apexHeight, float t)
        {
            t = Mathf.Clamp01(t);
            Vector3 p = Vector3.Lerp(from, to, t);
            p.y += 4f * apexHeight * t * (1f - t);
            return p;
        }
    }
}
