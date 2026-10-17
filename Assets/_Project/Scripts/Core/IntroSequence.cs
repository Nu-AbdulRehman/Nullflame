using System.Collections;
using UnityEngine;

namespace Nullflame
{
    // Beat 0 script (design doc §9, IMPL §21). Releases the wave guards one by one; Skip fast-forwards the
    // same deterministic motion to the control moment (EN-011).
    public sealed class IntroSequence : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] GameFlow flow;
        [SerializeField] Essence essence;
        [SerializeField] ScreenFx fx;
        [SerializeField] OrbitCamera cameraRig;
        [Tooltip("Close shot of the entity the camera pulls back from.")]
        [SerializeField] Transform closeUp;
        [SerializeField] AudioSource klaxon;
        [Tooltip("In release order: G-GLANCE, G-B, G-TIME, G-C, G-D, then G-MAIN.")]
        [SerializeField] GuardController[] wave;
        [Tooltip("G-GLANCE's pause point at D-PLAYER. Control is given once it walks on past it.")]
        [SerializeField] Transform glanceDoorWaypoint;

        const float SkipStep = 1f / 60f;
        const int SkipMaxSteps = 6000; // 100 s safety cap
        const float BlackTime = 1f, FadeTime = 1f; // cosmetic

        bool running;
        float elapsed;
        int released;

        public void Run() => StartCoroutine(Cinematic());

        IEnumerator Cinematic()
        {
            running = true;
            fx.SetBlack(1f);
            cameraRig.ShowPose(closeUp);
            yield return new WaitForSeconds(BlackTime);
            if (klaxon != null) klaxon.Play();
            for (float t = 0f; t < FadeTime; t += Time.deltaTime)
            {
                fx.SetBlack(1f - t / FadeTime);
                yield return null;
            }
            fx.SetBlack(0f);

            // G-GLANCE's lantern grazes the entity while it dwells at D-PLAYER: scripted, not volume damage (§6).
            float doorZ = glanceDoorWaypoint.position.z;
            Transform glance = wave[0].transform;
            while (Mathf.Abs(glance.position.z - doorZ) >= 0.01f) yield return null;
            essence.Damage(tuning.introGrazeDamage);
            fx.HitFlash();
            while (glance.position.z >= doorZ) yield return null;

            cameraRig.BlendFrom(closeUp, tuning.introCameraPullTime);
            yield return new WaitForSeconds(tuning.introCameraPullTime);
            flow.BeginPlaying();
        }

        // Retries start at Beat 1 with Essence 90 and the wave exactly where a full intro would leave it:
        // G-GLANCE has paused at D-PLAYER, walked on, and the camera pull has run.
        public void Skip()
        {
            if (klaxon != null) klaxon.Play();
            essence.Set(tuning.essenceStart - tuning.introGrazeDamage);
            running = true;
            float doorZ = glanceDoorWaypoint.position.z;
            bool atDoor = false;
            int pullSteps = -1;
            for (int step = 0; step < SkipMaxSteps && pullSteps != 0; step++)
            {
                Advance(SkipStep);
                for (int i = 0; i < wave.Length; i++) wave[i].Tick(SkipStep);
                if (pullSteps > 0) { pullSteps--; continue; }
                float z = wave[0].transform.position.z;
                if (Mathf.Abs(z - doorZ) < 0.01f) atDoor = true;
                else if (atDoor && z < doorZ) pullSteps = Mathf.RoundToInt(tuning.introCameraPullTime / SkipStep);
            }
            flow.BeginPlaying();
        }

        void Update()
        {
            if (running) Advance(Time.deltaTime);
        }

        void OnDisable() => StopAllCoroutines();

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
