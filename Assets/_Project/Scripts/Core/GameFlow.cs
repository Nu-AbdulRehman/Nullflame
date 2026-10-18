using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nullflame
{
    public enum FlowState { Intro, Playing, Dead, Won }

    // Intro, play, death, win and restart (design doc §9, §12, §14, IMPL §19). The title screen and the pause
    // menu are gone (AD-032): a new game runs the intro as soon as the scene loads, until a Menu scene replaces
    // the title. The Beat 1 "missed chance" fail was removed with floor movement (AD-024).
    // Restart reloads the scene; HasSeenIntro is the only static that survives it (CLAUDE.md §3).
    [DefaultExecutionOrder(-100)]
    public sealed class GameFlow : MonoBehaviour
    {
        public static bool HasSeenIntro;

        [SerializeField] GameTuning tuning;
        [SerializeField] Essence essence;
        [SerializeField] ShadowEntity entity;
        [SerializeField] OrbitCamera cameraRig;
        [SerializeField] IntroSequence intro;
        [SerializeField] GameObject deathScreen;
        [SerializeField] GameObject winScreen;
        [SerializeField] UnityEngine.UI.Text winTimeText;

        float restartTimer = -1f;

        public FlowState State { get; private set; } = FlowState.Intro;
        public float RunTime { get; private set; } // win screen "time taken", from the moment of control (EN-009)

        void OnEnable() => essence.Died += OnDied;
        void OnDisable() => essence.Died -= OnDied;

        void Start()
        {
            if (HasSeenIntro) intro.Skip();
            else
            {
                SetGameplay(false);
                intro.Run();
            }
        }

        // Called when the player gets control (end of the intro, or straight away on a retry).
        public void BeginPlaying()
        {
            HasSeenIntro = true;
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
            if (State == FlowState.Playing) RunTime += Time.deltaTime;
        }

        public void Restart()
        {
            HasSeenIntro = true;
            SceneManager.LoadScene(gameObject.scene.name);
        }

        // Win screen: a fresh run with the intro (EN-008, AD-032).
        public void NewGame()
        {
            HasSeenIntro = false;
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

        // Camera look and entity control on or off; the cursor follows the camera rig (locked while it runs).
        void SetGameplay(bool on)
        {
            entity.SetControl(on);
            cameraRig.enabled = on;
        }
    }
}
