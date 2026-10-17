using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Bottom-centre context prompt (design doc §12, IMPL §20.1). Rebuilds the string only when it changes.
    public sealed class PromptPresenter : MonoBehaviour
    {
        [SerializeField] ConsumeAction consume;
        [SerializeField] Essence essence;
        [SerializeField] TimePowers timePowers;
        [SerializeField] GameTuning tuning;
        [SerializeField] Text label;
        [SerializeField] Color normal = Color.white;
        [SerializeField] Color unaffordable = new Color(1f, 0.3f, 0.25f);

        Consumable shown;
        bool shownAffordable;
        bool shownReverse;

        void LateUpdate()
        {
            Consumable c = consume.Target != null ? consume.Target : consume.Available;
            bool affordable = c == null || c.IsGain || essence.CanAfford(c.Amount) || consume.Target != null;
            bool reverse = timePowers.CanReverse; // R/Q stay hidden until the time power is granted (§7.4)
            if (c == shown && affordable == shownAffordable && reverse == shownReverse) return;
            shown = c;
            shownAffordable = affordable;
            shownReverse = reverse;
            label.enabled = c != null || reverse;
            if (!label.enabled) return;
            string text = "";
            if (c != null)
            {
                int amount = Mathf.RoundToInt(c.Amount);
                text = c.IsGain ? $"[E] Consume {c.displayName} (+{amount})" : $"[E] Consume {c.displayName} ({amount})";
            }
            if (reverse) text += (text.Length > 0 ? "\n" : "") + $"[R] Reverse ({Mathf.RoundToInt(tuning.reverseCostPerSecond)}/s)";
            label.text = text;
            label.color = affordable ? normal : unaffordable;
        }
    }
}
