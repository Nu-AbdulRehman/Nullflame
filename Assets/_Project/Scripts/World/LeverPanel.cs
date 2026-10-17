using UnityEngine;

namespace Nullflame
{
    // The breaker lever (design doc §9 Beat 3). The panel and its shadow are shared; the Meltdown
    // handle rotates down with the rewind channel (EN-006), the Normal root holds the handle already down.
    public sealed class LeverPanel : MonoBehaviour
    {
        [Tooltip("The Meltdown (up) handle pivot.")]
        [SerializeField] Transform handle;
        [SerializeField] float upAngle = -50f;
        [SerializeField] float downAngle = 50f;

        public void SetProgress(float t)
        {
            if (handle != null) handle.localRotation = Quaternion.Euler(Mathf.Lerp(upAngle, downAngle, t), 0f, 0f);
        }
    }
}
