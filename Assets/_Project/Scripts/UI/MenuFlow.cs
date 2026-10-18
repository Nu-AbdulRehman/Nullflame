using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Nullflame
{
    // Menu scene: plays the intro video full screen, then shows Play and Quit (AD-032).
    public sealed class MenuFlow : MonoBehaviour
    {
        [SerializeField] VideoPlayer video;
        [SerializeField] GameObject menu;
        [SerializeField] Button play;
        [SerializeField] Button quit;
        [SerializeField] string gameScene = "Facility";

        void Awake()
        {
            play.onClick.AddListener(Play);
            quit.onClick.AddListener(Quit);
            menu.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Start()
        {
            if (video == null || video.clip == null) { ShowMenu(); return; }
            video.loopPointReached += OnVideoDone;
            video.errorReceived += OnVideoError;
            video.Play();
        }

        void OnDestroy()
        {
            if (video == null) return;
            video.loopPointReached -= OnVideoDone;
            video.errorReceived -= OnVideoError;
        }

        void OnVideoDone(VideoPlayer _) => ShowMenu();
        void OnVideoError(VideoPlayer _, string message) => ShowMenu();

        void ShowMenu()
        {
            if (video != null) video.Stop();
            menu.SetActive(true);
        }

        void Play()
        {
            GameFlow.HasSeenIntro = false;
            SceneManager.LoadScene(gameScene);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
