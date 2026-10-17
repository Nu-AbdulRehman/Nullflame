using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Nullflame
{
    // Screen effects (IMPL §20.3). B6: time-power desaturation (Volume weight) + scanlines
    // (Full Screen Pass reading the global _TimeFxIntensity, AD-013).
    public sealed class ScreenFx : MonoBehaviour
    {
        static readonly int TimeFxIntensity = Shader.PropertyToID("_TimeFxIntensity");

        [SerializeField] Volume timeVolume;
        [SerializeField] float fadeSpeed = 6f;
        [SerializeField] Image whiteOut;

        float current, target;
        float rewindTime = -1f, rewindDuration;

        public void SetTimeEffect(bool on) => target = on ? 1f : 0f;

        // White-out that peaks at the midpoint, where the world swaps (§10).
        public void PlayRewind(float duration)
        {
            rewindDuration = duration;
            rewindTime = 0f;
        }

        void Update()
        {
            current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime * fadeSpeed);
            timeVolume.weight = current;
            Shader.SetGlobalFloat(TimeFxIntensity, Mathf.Max(current, RewindAlpha()));
            if (rewindTime < 0f) return;
            rewindTime += Time.unscaledDeltaTime;
            Color c = whiteOut.color;
            c.a = RewindAlpha();
            whiteOut.color = c;
            whiteOut.enabled = c.a > 0f;
            if (rewindTime >= rewindDuration) rewindTime = -1f;
        }

        float RewindAlpha() => rewindTime < 0f ? 0f : 1f - Mathf.Abs(rewindTime / rewindDuration * 2f - 1f);

        void OnDisable() => Shader.SetGlobalFloat(TimeFxIntensity, 0f);
    }
}
