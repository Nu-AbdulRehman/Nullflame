using UnityEngine;

namespace Nullflame.Logic
{
    // The one bar (design doc §6, IMPL §4.1). Spending can never kill; only damage takes it to 0 (AD-010 context, O3).
    public sealed class EssenceLogic
    {
        readonly float floor;

        public EssenceLogic(float max, float start, float floor)
        {
            Max = max;
            this.floor = floor;
            Value = Mathf.Clamp(start, 0f, max);
        }

        public float Value { get; private set; }
        public float Max { get; }
        public bool IsDead => Value <= 0f;
        public bool AtFloor => Value <= floor;

        // Object consumes and the facility rewind need strictly more than the cost.
        public bool CanAfford(float cost) => Value > cost;

        public bool TrySpend(float cost)
        {
            if (!CanAfford(cost)) return false;
            Value -= cost;
            return true;
        }

        // Held time powers drain down to the floor and stop there. Returns false once at the floor.
        public bool SpendHeld(float amount)
        {
            if (Value <= floor) return false;
            Value = Mathf.Max(floor, Value - amount);
            return Value > floor;
        }

        public void Damage(float amount) => Value = Mathf.Max(0f, Value - amount);
        public void Kill() => Value = 0f;
        public void Gain(float amount) => Value = Mathf.Min(Max, Value + amount);
        public void Set(float value) => Value = Mathf.Clamp(value, 0f, Max);
    }
}
