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
        bool shownRewind;
        CrackablePipe shownPipe;

        void LateUpdate()
        {
            Consumable c = consume.Target != null ? consume.Target : consume.Available;
            bool affordable = c == null || c.IsGain || essence.CanAfford(c.Amount) || consume.Target != null;
            bool reverse = timePowers.CanReverse; // R/Q stay hidden until the time power is granted (§7.4)
            bool rewind = timePowers.AtRewindSpot;
            CrackablePipe pipe = consume.Pipe != null ? consume.Pipe : consume.AvailablePipe;
            if (rewind) affordable = essence.CanAfford(tuning.rewindCost) || timePowers.ChannelProgress > 0f;
            if (pipe != null) affordable = essence.CanAfford(tuning.pipeCost) || consume.Pipe != null;
            // ReferenceEquals: a consumed target is a destroyed object, which Unity's == reports equal to null,
            // so the plain compare kept the old prompt on screen after a consume.
            if (ReferenceEquals(c, shown) && affordable == shownAffordable && reverse == shownReverse && rewind == shownRewind && ReferenceEquals(pipe, shownPipe)) return;
            shownRewind = rewind;
            shownPipe = pipe;
            shown = c;
            shownAffordable = affordable;
            shownReverse = reverse;
            label.enabled = c != null || reverse || rewind || pipe != null;
            if (!label.enabled) return;
            string text = "";
            if (c != null)
            {
                int amount = Mathf.RoundToInt(c.Amount);
                text = c.IsGain ? $"[E] Consume {c.displayName} (+{amount})" : $"[E] Consume {c.displayName} ({amount})";
            }
            if (pipe != null) text = $"[E] Crack pipe ({Mathf.RoundToInt(tuning.pipeCost)})";
            if (reverse) text += (text.Length > 0 ? "\n" : "") + $"[R] Reverse ({Mathf.RoundToInt(tuning.reverseCostPerSecond)}/s)";
            if (rewind) text += (text.Length > 0 ? "\n" : "") + $"[Q] Rewind facility ({Mathf.RoundToInt(tuning.rewindCost)})";
            label.text = text;
            label.color = affordable ? normal : unaffordable;
        }
    }
}
