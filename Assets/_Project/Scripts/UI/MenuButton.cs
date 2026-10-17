using UnityEngine;
using UnityEngine.UI;

namespace Nullflame
{
    public enum MenuAction { Play, Resume, Restart, Quit, ReturnToTitle }

    // Binds a uGUI Button to a GameFlow action chosen in the Inspector (EN-026).
    [RequireComponent(typeof(Button))]
    public sealed class MenuButton : MonoBehaviour
    {
        public MenuAction action;
        [SerializeField] GameFlow flow;

        void Awake() => GetComponent<Button>().onClick.AddListener(Run);

        void Run()
        {
            switch (action)
            {
                case MenuAction.Play: flow.Play(); break;
                case MenuAction.Resume: flow.Resume(); break;
                case MenuAction.Restart: flow.Restart(); break;
                case MenuAction.Quit: flow.Quit(); break;
                case MenuAction.ReturnToTitle: flow.ReturnToTitle(); break;
            }
        }
    }
}
