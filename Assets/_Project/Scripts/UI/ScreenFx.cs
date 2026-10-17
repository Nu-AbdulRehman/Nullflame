using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Nullflame
{
    // Screen effects (IMPL §20.3): time-power desaturation (Volume weight) + scanlines (Full Screen Pass
    // reading the global _TimeFxIntensity, AD-013), orange in-flame vignette, red hit flash, rewind white-out
    // and the intro black screen.
    public sealed class ScreenFx : MonoBehaviour
    {
        static readonly int TimeFxIntensity = Shader.PropertyToID("_TimeFxIntensity");

        [SerializeField] Volume timeVolume;
        [SerializeField] Volume flameVolume;
        [SerializeField] FlameReceiver flame;
        [SerializeField] float fadeSpeed = 6f;
        [SerializeField] Image whiteOut;
        [SerializeField] Image hitFlash;
        [SerializeField] float hitFlashTime = 0.35f;

        float current, target;
        float rewindTime = -1f, rewindDuration;
        float flashLeft;
        bool wasInFlame;

        public void SetTimeEffect(bool on) => target = on ? 1f : 0f;

        // White-out that peaks at the midpoint, where the world swaps (§10).
        public void PlayRewind(float duration)
        {
            rewindDuration = duration;
            rewindTime = 0f;
        }

        public void SetBlack(float alpha)
        {
            whiteOut.color = new Color(0f, 0f, 0f, alpha);
            whiteOut.enabled = alpha > 0f;
        }

        public void HitFlash() => flashLeft = hitFlashTime;

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            current = Mathf.MoveTowards(current, target, dt * fadeSpeed);
            timeVolume.weight = current;
            Shader.SetGlobalFloat(TimeFxIntensity, Mathf.Max(current, RewindAlpha()));

            bool inFlame = flame.IsInFlame;
            if (inFlame && !wasInFlame) HitFlash();
            wasInFlame = inFlame;
            flameVolume.weight = Mathf.MoveTowards(flameVolume.weight, inFlame ? 1f : 0f, dt * fadeSpeed);

            flashLeft = Mathf.Max(0f, flashLeft - dt);
            Color f = hitFlash.color;
            f.a = 0.5f * flashLeft / hitFlashTime;
            hitFlash.color = f;
            hitFlash.enabled = flashLeft > 0f;

            if (rewindTime < 0f) return;
            rewindTime += dt;
            whiteOut.color = new Color(1f, 1f, 1f, RewindAlpha());
            whiteOut.enabled = whiteOut.color.a > 0f;
            if (rewindTime >= rewindDuration) rewindTime = -1f;
        }

        float RewindAlpha() => rewindTime < 0f ? 0f : 1f - Mathf.Abs(rewindTime / rewindDuration * 2f - 1f);

        void OnDisable() => Shader.SetGlobalFloat(TimeFxIntensity, 0f);
    }
}
