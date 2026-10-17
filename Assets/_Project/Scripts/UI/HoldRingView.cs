using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Radial ring around the reticle while a hold action runs (design doc §12, IMPL §20).
    public sealed class HoldRingView : MonoBehaviour
    {
        [SerializeField] ConsumeAction consume;
        [SerializeField] Image ring;

        void LateUpdate()
        {
            bool active = consume.Target != null;
            ring.enabled = active;
            if (active) ring.fillAmount = consume.Progress;
        }
    }
}
