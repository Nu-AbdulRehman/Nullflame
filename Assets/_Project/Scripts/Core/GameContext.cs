using UnityEngine;

namespace Nullflame
{
    // Scene-wide manager references (AD-001, IMPL §2.4). Runtime-spawned prefabs read Current;
    // scene objects prefer serialized references. Cleared on destroy, so nothing survives a reload.
    // Fields are added as systems arrive.
    [DefaultExecutionOrder(-1000)]
    public sealed class GameContext : MonoBehaviour
    {
        public static GameContext Current { get; private set; }

        public ShadowRegistry shadows;
        public ShadowEntity entity;

        void Awake() => Current = this;

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
