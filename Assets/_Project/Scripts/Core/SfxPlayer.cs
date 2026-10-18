using UnityEngine;

namespace Nullflame
{
    public enum SfxId
    {
        Klaxon, AlertSting, JumpWhoosh, ReverseWhoosh, RewindSweep, ConsumeLoop, ConsumeDone, PowerGranted,
        FireLoop, LanternSmash, PipeCreak, PipeCrack, OilDrip, PipeBurst, GateSlide, DoorSlide, Footstep,
        Heartbeat, MusicMeltdown, MusicNormal, DeathSting, WinSting, UiClick,
        Hurt, Scream, // appended: a burn starting, a guard being eaten
    }

    // 2D one-shots and named loops (IMPL §22.1). Clips come only from the AssetRegistry; an empty slot is silent.
    public sealed class SfxPlayer : MonoBehaviour
    {
        [SerializeField] AssetRegistry registry;
        [SerializeField] Essence essence;
        [SerializeField] GameTuning tuning;
        [SerializeField] AudioSource oneShots;
        [Tooltip("Loop sources, one each: MusicMeltdown, MusicNormal, ReverseWhoosh, Heartbeat.")]
        [SerializeField] AudioSource[] loops;

        static readonly SfxId[] LoopIds = { SfxId.MusicMeltdown, SfxId.MusicNormal, SfxId.ReverseWhoosh, SfxId.Heartbeat };

        void Start()
        {
            for (int i = 0; i < loops.Length; i++)
            {
                loops[i].clip = registry.Clip(LoopIds[i]);
                loops[i].loop = true;
            }
            SetLoop(SfxId.MusicMeltdown, true);
        }

        void Update() => SetLoop(SfxId.Heartbeat, essence.Value < tuning.essenceLowThreshold && essence.Value > 0f);

        // For any system: silent when there is no player or no clip.
        public static void Play(SfxId id)
        {
            GameContext c = GameContext.Current;
            if (c != null && c.sfx != null) c.sfx.PlayOneShot(id);
        }

        public static void Loop(SfxId id, bool on)
        {
            GameContext c = GameContext.Current;
            if (c != null && c.sfx != null) c.sfx.SetLoop(id, on);
        }

        public void PlayOneShot(SfxId id)
        {
            AudioClip clip = registry.Clip(id);
            if (clip != null) oneShots.PlayOneShot(clip);
        }

        public void SetLoop(SfxId id, bool on)
        {
            AudioSource s = loops[System.Array.IndexOf(LoopIds, id)];
            if (s.clip == null || s.isPlaying == on) return;
            if (on) s.Play();
            else s.Stop();
        }
    }
}
