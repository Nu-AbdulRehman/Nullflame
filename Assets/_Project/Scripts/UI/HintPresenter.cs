using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // One-shot tutorial hints at the first opportunity (design doc §12, IMPL §20.2). They reset on reload.
    public sealed class HintPresenter : MonoBehaviour
    {
        enum Hint { Move, Jump, Consume, Reverse, Rewind, Pipe, Count }

        [SerializeField] ShadowEntity entity;
        [SerializeField] JumpTargeting jump;
        [SerializeField] ConsumeAction consume;
        [SerializeField] TimePowers timePowers;
        [SerializeField] Text label;
        [SerializeField] float showTime = 5f;
        [Tooltip("In order: move, jump, consume, reverse, rewind, crack pipe.")]
        [SerializeField] string[] texts =
        {
            "W/A/S/D to slide within your shadow. Move the mouse to look.",
            "Aim at a shadow and left-click to jump to it.",
            "Hold E to consume the owner of this shadow.",
            "Hold R while riding a guard to reverse their time.", // §9 Beat 2, copied exactly
            "Hold Q to rewind the facility.",
            "Hold E to crack the pipe. Release in the green.",
        };

        int shown; // bit per Hint
        float left;

        void Update()
        {
            if (entity.HasControl)
            {
                Try(Hint.Move, true);
                Try(Hint.Jump, jump.HasValidTarget);
                Try(Hint.Consume, consume.Available != null);
                Try(Hint.Reverse, timePowers.HasTimePower);
                Try(Hint.Rewind, timePowers.AtRewindSpot);
                Try(Hint.Pipe, consume.AvailablePipe != null);
            }
            if (left <= 0f) return;
            left -= Time.deltaTime;
            if (left <= 0f) label.enabled = false;
        }

        void Try(Hint hint, bool condition)
        {
            int bit = 1 << (int)hint;
            if (!condition || (shown & bit) != 0) return;
            shown |= bit;
            label.text = texts[(int)hint];
            label.enabled = true;
            left = showTime;
        }
    }
}
