using UnityEngine;

namespace Nullflame
{
    // Beat 0 script (design doc §9, IMPL §21). B3 part: releases the wave guards one by one.
    // Camera, graze, doors and skip arrive in B8/B10.
    public sealed class IntroSequence : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [Tooltip("In release order: G-GLANCE, G-B, G-TIME, G-C, G-D, then G-MAIN.")]
        [SerializeField] GuardController[] wave;

        float elapsed;
        int released;

        void Update()
        {
            if (released >= wave.Length) return;
            elapsed += Time.deltaTime;
            // Guard i leaves at spacing * (i + 1): 3 s, 6 s, 9 s ... (design doc §9 Beat 0, T-INTRO-WAVE).
            while (released < wave.Length && elapsed >= tuning.introWaveSpacing * (released + 1))
                wave[released++].StartRoute();
        }
    }
}
