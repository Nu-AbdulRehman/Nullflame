using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Bottom-centre context prompt (design doc §12, IMPL §20.1). Rebuilds the string only when it changes.
    public sealed class PromptPresenter : MonoBehaviour
    {
        [SerializeField] ConsumeAction consume;
        [SerializeField] Essence essence;
        [SerializeField] Text label;
        [SerializeField] Color normal = Color.white;
        [SerializeField] Color unaffordable = new Color(1f, 0.3f, 0.25f);

        Consumable shown;
        bool shownAffordable;

        void LateUpdate()
        {
            Consumable c = consume.Target != null ? consume.Target : consume.Available;
            bool affordable = c == null || c.IsGain || essence.CanAfford(c.Amount) || consume.Target != null;
            if (c == shown && affordable == shownAffordable) return;
            shown = c;
            shownAffordable = affordable;
            label.enabled = c != null;
            if (c == null) return;
            int amount = Mathf.RoundToInt(c.Amount);
            label.text = c.IsGain ? $"[E] Consume {c.displayName} (+{amount})" : $"[E] Consume {c.displayName} ({amount})";
            label.color = affordable ? normal : unaffordable;
        }
    }
}
