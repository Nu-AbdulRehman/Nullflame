using UnityEngine;

namespace Nullflame
{
    // Keeps the guard's lantern (light, damage volume, model) in its hand after the animation has posed it (EN-048).
    public sealed class LanternHold : MonoBehaviour
    {
        [SerializeField] string handBone = "Hand_R";

        Transform hand;

        void Awake()
        {
            foreach (Transform t in transform.parent.GetComponentsInChildren<Transform>(true))
                if (t.name == handBone) { hand = t; break; }
        }

        void LateUpdate()
        {
            if (hand != null) transform.SetPositionAndRotation(hand.position, Quaternion.Euler(0f, transform.parent.eulerAngles.y, 0f));
        }
    }
}
