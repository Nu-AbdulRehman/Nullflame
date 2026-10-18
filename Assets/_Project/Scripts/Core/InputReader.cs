using UnityEngine;
using UnityEngine.InputSystem;

namespace Nullflame
{
    // The only place gameplay reads input (AD-006, IMPL §7.4). Reads Input System devices directly.
    public sealed class InputReader : MonoBehaviour
    {
        public Vector2 Look { get; private set; }
        public Vector2 Move { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool ConsumeHeld { get; private set; }
        public bool ReverseHeld { get; private set; }
        public bool RewindHeld { get; private set; }
        public bool SkipPressed { get; private set; }

        bool simulated;

        // Tests drive holds without devices. Look, move and jump stay zero while simulated.
        public void Simulate(bool consume, bool reverse = false, bool rewind = false)
        {
            simulated = true;
            Look = Move = Vector2.zero;
            JumpPressed = SkipPressed = false;
            ConsumeHeld = consume;
            ReverseHeld = reverse;
            RewindHeld = rewind;
        }

        void Update()
        {
            if (simulated) return;
            var mouse = Mouse.current;
            var kb = Keyboard.current;
            Look = mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
            JumpPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
            if (kb == null)
            {
                Move = Vector2.zero;
                ConsumeHeld = ReverseHeld = RewindHeld = SkipPressed = false;
                return;
            }
            Move = new Vector2(Axis(kb.dKey, kb.aKey), Axis(kb.wKey, kb.sKey));
            ConsumeHeld = kb.eKey.isPressed;
            ReverseHeld = kb.rKey.isPressed;
            RewindHeld = kb.qKey.isPressed;
            SkipPressed = kb.spaceKey.wasPressedThisFrame;
        }

        static float Axis(UnityEngine.InputSystem.Controls.KeyControl positive, UnityEngine.InputSystem.Controls.KeyControl negative) =>
            (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }
}
