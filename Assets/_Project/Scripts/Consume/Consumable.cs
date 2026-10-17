using UnityEngine;

namespace Nullflame
{
    public enum ConsumableKind { Guard, Pedestal, FinalGate }

    // Something the entity can eat through its shadow (design doc §7.3, IMPL §13.1).
    // Cost/gain and duration come from GameTuning by kind.
    public sealed class Consumable : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        public ConsumableKind kind;
        [Tooltip("Shown in the prompt: Guard, Time SCP, Final Gate.")]
        public string displayName = "Guard";
        [Tooltip("What sinks into the floor in the last part of the consume.")]
        public Transform sinkRoot;

        public bool IsGain => kind == ConsumableKind.Guard;
        public float Amount => kind == ConsumableKind.Guard ? tuning.guardGain
            : kind == ConsumableKind.Pedestal ? tuning.pedestalCost : tuning.gateCost;
        public float Duration => kind == ConsumableKind.Guard ? tuning.guardConsumeTime
            : kind == ConsumableKind.Pedestal ? tuning.pedestalConsumeTime : tuning.gateConsumeTime;
    }
}
