using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Nullflame
{
    // Outro scene: the win cutscene, then looping credits video with music and scrolling credits, then the menu.
    public sealed class OutroFlow : MonoBehaviour
    {
        [SerializeField] VideoPlayer video;
        [SerializeField] VideoClip outro;
        [SerializeField] VideoClip creditsLoop;
        [SerializeField] AudioSource music;
        [SerializeField] AudioClip musicClip;
        [SerializeField] GameObject creditsRoot;
        [SerializeField] RectTransform credits;
        [SerializeField] float scrollSpeed = 80f; // canvas units per second
        [SerializeField] float endPause = 2f;
        [SerializeField] string menuScene = "Menu";

        bool rolling;
        float endTimer;

        void Awake()
        {
            creditsRoot.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Start()
        {
            video.loopPointReached += OnOutroDone;
            video.errorReceived += OnVideoError;
            video.isLooping = false;
            video.clip = outro;
            if (outro != null) video.Play(); else StartCredits();
        }

        void OnDestroy()
        {
            video.loopPointReached -= OnOutroDone;
            video.errorReceived -= OnVideoError;
        }

        void OnOutroDone(VideoPlayer _) => StartCredits();
        void OnVideoError(VideoPlayer _, string message) => StartCredits();

        void StartCredits()
        {
            if (rolling) return;
            rolling = true;
            video.loopPointReached -= OnOutroDone;
            video.Stop();
            video.clip = creditsLoop;
            video.isLooping = true;
            if (creditsLoop != null) video.Play();
            if (music != null && musicClip != null)
            {
                music.clip = musicClip;
                music.loop = true;
                music.Play();
            }
            creditsRoot.SetActive(true);
            // Start just below the screen; the canvas is 1080 tall with the pivot at the centre.
            credits.anchoredPosition = new Vector2(0f, -540f - credits.rect.height * 0.5f);
        }

        void Update()
        {
            if (!rolling) return;
            credits.anchoredPosition += Vector2.up * (scrollSpeed * Time.deltaTime);
            if (credits.anchoredPosition.y < 540f + credits.rect.height * 0.5f) return;
            endTimer += Time.deltaTime;
            if (endTimer >= endPause) SceneManager.LoadScene(menuScene);
        }
    }
}
