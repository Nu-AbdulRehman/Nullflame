using UnityEngine;

namespace Nullflame
{
    public enum WorldState { Meltdown, Normal }

    // Owns the two world-state roots; the facility rewind is the only thing that swaps them
    // (design doc §10, IMPL §15). One-way: Meltdown -> Normal.
    public sealed class WorldStateController : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] GameObject meltdownRoot;
        [SerializeField] GameObject normalRoot;
        [SerializeField] ScreenFx fx;

        float swapTimer = -1f;

        public WorldState State { get; private set; } = WorldState.Meltdown;

        public void Rewind()
        {
            if (State != WorldState.Meltdown || swapTimer >= 0f) return;
            fx.PlayRewind(tuning.rewindEffectTime);
            SfxPlayer.Play(SfxId.RewindSweep);
            swapTimer = tuning.rewindEffectTime * 0.5f; // swap behind the white-out peak
        }

        void Update()
        {
            if (swapTimer < 0f) return;
            swapTimer -= Time.deltaTime;
            if (swapTimer > 0f) return;
            swapTimer = -1f;
            normalRoot.SetActive(true);
            Destroy(meltdownRoot); // Meltdown guards, alarms, wall fire and doors go with it (§10)
            State = WorldState.Normal;
            SfxPlayer.Loop(SfxId.MusicMeltdown, false);
            SfxPlayer.Loop(SfxId.MusicNormal, true);
        }
    }
}
