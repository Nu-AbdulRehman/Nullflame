using UnityEngine;
using UnityEngine.Rendering;

namespace Nullflame
{
    // Screen effects (IMPL §20.3). B6: time-power desaturation (Volume weight) + scanlines
    // (Full Screen Pass reading the global _TimeFxIntensity, AD-013).
    public sealed class ScreenFx : MonoBehaviour
    {
        static readonly int TimeFxIntensity = Shader.PropertyToID("_TimeFxIntensity");

        [SerializeField] Volume timeVolume;
        [SerializeField] float fadeSpeed = 6f;

        float current, target;

        public void SetTimeEffect(bool on) => target = on ? 1f : 0f;

        void Update()
        {
            current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime * fadeSpeed);
            timeVolume.weight = current;
            Shader.SetGlobalFloat(TimeFxIntensity, current);
        }

        void OnDisable() => Shader.SetGlobalFloat(TimeFxIntensity, 0f);
    }
}
