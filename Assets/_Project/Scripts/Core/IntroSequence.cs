using UnityEngine;

namespace Nullflame
{
    // Beat 0 script (design doc §9, IMPL §21). Releases the wave guards one by one; Skip fast-forwards the
    // same deterministic motion to the control moment (EN-011). Camera, graze and doors arrive in B10.
    public sealed class IntroSequence : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] GameFlow flow;
        [SerializeField] Essence essence;
        [Tooltip("In release order: G-GLANCE, G-B, G-TIME, G-C, G-D, then G-MAIN.")]
        [SerializeField] GuardController[] wave;
        [Tooltip("G-GLANCE's pause point at D-PLAYER. Control is given once it walks on past it.")]
        [SerializeField] Transform glanceDoorWaypoint;

        const float SkipStep = 1f / 60f;
        const int SkipMaxSteps = 6000; // 100 s safety cap

        bool running;
        float elapsed;
        int released;

        public void Run()
        {
            running = true;
            flow.BeginPlaying(); // B10 adds the cinematic before control
        }

        // Retries start at Beat 1 with Essence 90 and the wave exactly where a full intro would leave it.
        public void Skip()
        {
            essence.Set(tuning.essenceStart - tuning.introGrazeDamage);
            running = true;
            float doorZ = glanceDoorWaypoint.position.z;
            bool atDoor = false;
            for (int step = 0; step < SkipMaxSteps; step++)
            {
                Advance(SkipStep);
                for (int i = 0; i < wave.Length; i++) wave[i].Tick(SkipStep);
                float z = wave[0].transform.position.z;
                if (Mathf.Abs(z - doorZ) < 0.01f) atDoor = true;
                else if (atDoor && z < doorZ) break;
            }
            flow.BeginPlaying();
        }

        void Update()
        {
            if (running) Advance(Time.deltaTime);
        }

        // Guard i leaves at spacing * (i + 1): 3 s, 6 s, 9 s ... (design doc §9 Beat 0, T-INTRO-WAVE).
        void Advance(float dt)
        {
            if (released >= wave.Length) return;
            elapsed += dt;
            while (released < wave.Length && elapsed >= tuning.introWaveSpacing * (released + 1))
                wave[released++].StartRoute();
        }
    }
}
