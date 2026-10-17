using System;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Scene wrapper around EssenceLogic with change events (design doc §6, IMPL §9).
    [DefaultExecutionOrder(-900)] // Awake before any view or system reads it
    public sealed class Essence : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;

        EssenceLogic logic;
        bool diedRaised;

        public float Value => logic.Value;
        public float Max => logic.Max;
        public float PreviewCost { get; set; }
        public event Action<float> Changed;
        public event Action Died;
        public event Action FloorReached;

        void Awake() => logic = new EssenceLogic(tuning.essenceMax, tuning.essenceStart, tuning.essenceFloor);

        public bool CanAfford(float cost) => logic.CanAfford(cost);

        public bool TrySpend(float cost)
        {
            if (!logic.TrySpend(cost)) return false;
            Changed?.Invoke(Value);
            return true;
        }

        public bool SpendHeld(float amount)
        {
            bool more = logic.SpendHeld(amount);
            Changed?.Invoke(Value);
            if (!more) FloorReached?.Invoke();
            return more;
        }

        public void Damage(float amount)
        {
            logic.Damage(amount);
            Report();
        }

        public void Kill()
        {
            logic.Kill();
            Report();
        }

        public void Gain(float amount)
        {
            logic.Gain(amount);
            Changed?.Invoke(Value);
        }

        public void Set(float value)
        {
            logic.Set(value);
            Changed?.Invoke(Value);
        }

        void Report()
        {
            Changed?.Invoke(Value);
            if (!logic.IsDead || diedRaised) return;
            diedRaised = true;
            Died?.Invoke();
        }
    }
}
