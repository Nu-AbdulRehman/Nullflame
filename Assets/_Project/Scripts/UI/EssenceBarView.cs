using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Top-left Essence bar: fill, cost ghost, number, low pulse (design doc §12, IMPL §20).
    public sealed class EssenceBarView : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] Essence essence;
        [SerializeField] RectTransform fill;
        [SerializeField] RectTransform ghost;
        [SerializeField] Graphic fillGraphic;
        [SerializeField] Text label;
        [SerializeField] float pulseSpeed = 6f;

        Color baseColor;

        void Awake() => baseColor = fillGraphic.color;

        void OnEnable()
        {
            essence.Changed += Refresh;
            Refresh(essence.Value);
        }

        void OnDisable() => essence.Changed -= Refresh;

        void Refresh(float value)
        {
            fill.anchorMax = new Vector2(value / essence.Max, 1f);
            label.text = Mathf.CeilToInt(value).ToString(); // only on change
        }

        void LateUpdate()
        {
            float v = essence.Value / essence.Max;
            float cost = essence.PreviewCost / essence.Max;
            ghost.gameObject.SetActive(cost > 0f);
            if (cost > 0f)
            {
                ghost.anchorMin = new Vector2(Mathf.Max(0f, v - cost), 0f);
                ghost.anchorMax = new Vector2(v, 1f);
            }
            Color c = baseColor;
            if (essence.Value < tuning.essenceLowThreshold)
                c.a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed)); // T-ESS-LOW pulse
            fillGraphic.color = c;
        }
    }
}
