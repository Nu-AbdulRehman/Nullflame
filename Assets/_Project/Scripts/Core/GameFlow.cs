using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nullflame
{
    // Death -> "Burned away." -> reload (design doc §12, §14, IMPL §19). Title, pause and win arrive in B8/B9.
    [DefaultExecutionOrder(-100)]
    public sealed class GameFlow : MonoBehaviour
    {
        // The only static that survives a reload (CLAUDE.md §3): retries skip the intro.
        public static bool HasSeenIntro;

        [SerializeField] GameTuning tuning;
        [SerializeField] Essence essence;
        [SerializeField] GameObject deathScreen;

        float restartTimer = -1f;

        void OnEnable() => essence.Died += OnDied;
        void OnDisable() => essence.Died -= OnDied;

        void OnDied()
        {
            deathScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            restartTimer = tuning.autoRestartDelay;
        }

        void Update()
        {
            if (restartTimer < 0f) return;
            restartTimer -= Time.unscaledDeltaTime;
            if (restartTimer <= 0f) Restart();
        }

        public void Restart()
        {
            HasSeenIntro = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
