using UnityEngine;

namespace Nullflame.Logic
{
    // Footprint math for ShadowArea rects (IMPL §4.9). A rect is size (local X, local Z) centred on
    // `center`, rotated by `yawDeg` about Y. Heights are ignored; results sit at center.y.
    public static class ShadowMath
    {
        public static bool Contains(Vector3 center, float yawDeg, Vector2 size, Vector3 point)
        {
            Vector3 l = ToLocal(center, yawDeg, point);
            return Mathf.Abs(l.x) <= size.x * 0.5f && Mathf.Abs(l.z) <= size.y * 0.5f;
        }

        public static Vector3 ClosestPoint(Vector3 center, float yawDeg, Vector2 size, Vector3 point) =>
            ClampInside(center, yawDeg, size, 0f, point);

        // Clamps point into the rect shrunk by margin on every side. An axis too small for the margin
        // collapses to the centre line (design doc §3: sliding only in shadows larger than the entity).
        public static Vector3 ClampInside(Vector3 center, float yawDeg, Vector2 size, float margin, Vector3 point)
        {
            Vector3 l = ToLocal(center, yawDeg, point);
            float hx = Mathf.Max(0f, size.x * 0.5f - margin);
            float hz = Mathf.Max(0f, size.y * 0.5f - margin);
            var clamped = new Vector3(Mathf.Clamp(l.x, -hx, hx), 0f, Mathf.Clamp(l.z, -hz, hz));
            return center + Quaternion.Euler(0f, yawDeg, 0f) * clamped;
        }

        static Vector3 ToLocal(Vector3 center, float yawDeg, Vector3 point)
        {
            Vector3 d = point - center;
            d.y = 0f;
            return Quaternion.Euler(0f, -yawDeg, 0f) * d;
        }
    }
}
