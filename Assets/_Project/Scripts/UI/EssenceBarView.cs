using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Top-left Essence bar: fill, cost ghost, number, low pulse (design doc §12, IMPL §20).
    public sealed class EssenceBarView : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] Essence essence;
        // Filled (horizontal) images, so the bar's cut corners clip instead of squashing; Ghost sits under Fill.
        [SerializeField] Image fill;
        [SerializeField] Image ghost;
        [SerializeField] Text label;
        [SerializeField] float pulseSpeed = 6f;

        Color baseColor;

        void Awake() => baseColor = fill.color;

        void OnEnable()
        {
            essence.Changed += Refresh;
            Refresh(essence.Value);
        }

        void OnDisable() => essence.Changed -= Refresh;

        void Refresh(float value) => label.text = Mathf.CeilToInt(value).ToString(); // only on change

        void LateUpdate()
        {
            float v = essence.Value / essence.Max;
            float cost = essence.PreviewCost / essence.Max;
            ghost.gameObject.SetActive(cost > 0f);
            ghost.fillAmount = v;
            fill.fillAmount = Mathf.Max(0f, v - cost); // the cost shows as the ghost between the two
            Color c = baseColor;
            if (essence.Value < tuning.essenceLowThreshold)
                c.a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed)); // T-ESS-LOW pulse
            fill.color = c;
        }
    }
}
