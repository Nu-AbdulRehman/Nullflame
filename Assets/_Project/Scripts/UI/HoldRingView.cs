using UnityEngine;
using UnityEngine.UI;
using Nullflame.Logic;

namespace Nullflame
{
    // Radial ring around the reticle while a hold action runs (design doc §12, IMPL §20).
    // On a pipe the ring takes the meter zone's colour: grey, green (safe), red (warning) (§7.6, EN-032).
    public sealed class HoldRingView : MonoBehaviour
    {
        [SerializeField] ConsumeAction consume;
        [SerializeField] Image ring;
        [SerializeField] Color normal = Color.white;
        [SerializeField] Color grey = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] Color safe = new Color(0.3f, 1f, 0.35f);
        [SerializeField] Color warning = new Color(1f, 0.3f, 0.25f);

        void LateUpdate()
        {
            bool onPipe = consume.Pipe != null;
            bool active = consume.Target != null || onPipe;
            ring.enabled = active;
            if (!active) return;
            ring.fillAmount = consume.Progress;
            ring.color = !onPipe ? normal
                : consume.Zone == PipeZone.Safe ? safe
                : consume.Zone == PipeZone.Grey ? grey
                : warning;
        }
    }
}
