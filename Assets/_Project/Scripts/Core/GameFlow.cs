using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nullflame
{
    public enum FlowState { Title, Playing, Paused, Dead, Missed, Won }

    // Title, play, pause, death, Beat 1 soft fail and restart (design doc §9, §12, §14, IMPL §19).
    // Restart reloads the scene; HasSeenIntro is the only static that survives it (CLAUDE.md §3).
    [DefaultExecutionOrder(-100)]
    public sealed class GameFlow : MonoBehaviour
    {
        public static bool HasSeenIntro;

        [SerializeField] GameTuning tuning;
        [SerializeField] FacilityLayout layout;
        [SerializeField] Essence essence;
        [SerializeField] ShadowEntity entity;
        [SerializeField] InputReader input;
        [SerializeField] TimePowers timePowers;
        [SerializeField] OrbitCamera cameraRig;
        [SerializeField] IntroSequence intro;
        [Tooltip("G-GLANCE, G-B, G-TIME, G-C, G-D: when all reach their posts, Beat 1 is missed.")]
        [SerializeField] GuardController[] waveGuards;
        [SerializeField] GameObject titleScreen;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject deathScreen;
        [SerializeField] GameObject missedScreen;
        [SerializeField] GameObject winScreen;
        [SerializeField] UnityEngine.UI.Text winTimeText;

        float restartTimer = -1f;
        Rect timeRoom;

        public FlowState State { get; private set; } = FlowState.Title;
        public float RunTime { get; private set; } // win screen "time taken", from the moment of control (EN-009)

        void OnEnable() => essence.Died += OnDied;
        void OnDisable() => essence.Died -= OnDied;

        void Start()
        {
            layout.TryGetArea("TimeRoom", out timeRoom);
            if (HasSeenIntro) intro.Skip();
            else
            {
                titleScreen.SetActive(true);
                SetGameplay(false);
            }
        }

        public void Play()
        {
            titleScreen.SetActive(false);
            intro.Run();
        }

        // Called when the player gets control (end of the intro, or straight away on a retry).
        public void BeginPlaying()
        {
            HasSeenIntro = true;
            titleScreen.SetActive(false);
            State = FlowState.Playing;
            SetGameplay(true);
        }

        void Update()
        {
            if (restartTimer >= 0f)
            {
                restartTimer -= Time.unscaledDeltaTime;
                if (restartTimer <= 0f) Restart();
                return;
            }
            if (State == FlowState.Playing)
            {
                RunTime += Time.deltaTime;
                if (input.PausePressed) Pause();
                else CheckMissedChance();
            }
            else if (State == FlowState.Paused && input.PausePressed) Resume();
        }

        public void Pause()
        {
            State = FlowState.Paused;
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
            SetGameplay(false);
        }

        public void Resume()
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
            State = FlowState.Playing;
            SetGameplay(true);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            HasSeenIntro = true;
            SceneManager.LoadScene(gameObject.scene.name);
        }

        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            HasSeenIntro = false; // the title and intro show again (EN-008)
            SceneManager.LoadScene(gameObject.scene.name);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Final Gate consumed (Beat 5): win screen with the run time and credits.
        public void Win()
        {
            State = FlowState.Won;
            int s = Mathf.FloorToInt(RunTime);
            winTimeText.text = $"Time taken: {s / 60}:{s % 60:00}";
            winScreen.SetActive(true);
            SfxPlayer.Play(SfxId.WinSting);
            SetGameplay(false);
        }

        void OnDied()
        {
            State = FlowState.Dead;
            deathScreen.SetActive(true);
            SfxPlayer.Play(SfxId.DeathSting);
            SetGameplay(false);
            restartTimer = tuning.autoRestartDelay;
        }

        // Beat 1 soft fail: every wave guard is at its post and the entity never reached the Time Room.
        void CheckMissedChance()
        {
            if (timePowers.HasTimePower) return;
            Vector3 p = entity.transform.position;
            if (timeRoom.Contains(new Vector2(p.x, p.z))) return;
            for (int i = 0; i < waveGuards.Length; i++)
                if (waveGuards[i] == null || !waveGuards[i].Motion.ReachedEnd) return; // destroyed by the rewind: Beat 1 is over
            State = FlowState.Missed;
            missedScreen.SetActive(true);
            SetGameplay(false);
            restartTimer = tuning.autoRestartDelay;
        }

        // Camera look and entity control on or off; the cursor follows the camera rig (locked while it runs).
        void SetGameplay(bool on)
        {
            entity.SetControl(on);
            cameraRig.enabled = on;
        }
    }
}
