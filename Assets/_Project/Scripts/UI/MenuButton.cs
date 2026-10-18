using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    // Values kept from before the title and pause menus went (AD-032), so saved buttons stay bound.
    public enum MenuAction { Restart = 2, Quit = 3 }

    // Binds a uGUI Button to a GameFlow action chosen in the Inspector (EN-026).
    [RequireComponent(typeof(Button))]
    public sealed class MenuButton : MonoBehaviour
    {
        public MenuAction action;
        [SerializeField] GameFlow flow;

        void Awake() => GetComponent<Button>().onClick.AddListener(Run);

        void Run()
        {
            SfxPlayer.Play(SfxId.UiClick);
            switch (action)
            {
                case MenuAction.Restart: flow.Restart(); break;
                case MenuAction.Quit: flow.Quit(); break;
            }
        }
    }
}
