using UnityEngine;

namespace Nullflame
{
    // Keeps an object on a bone after the animation has posed it (EN-048): the lantern in the hand, the SCP
    // badge on the back. Position = bone + offset in the guard's frame; rotation = guard yaw + yawOffset.
    public sealed class LanternHold : MonoBehaviour
    {
        [SerializeField] string handBone = "Hand_R";
        [SerializeField] Vector3 offset;
        [SerializeField] float yawOffset;

        Transform hand;

        void Awake()
        {
            foreach (Transform t in transform.parent.GetComponentsInChildren<Transform>(true))
                if (t.name == handBone) { hand = t; break; }
        }

        void LateUpdate()
        {
            if (hand == null) return;
            Quaternion yaw = Quaternion.Euler(0f, transform.parent.eulerAngles.y, 0f);
            transform.SetPositionAndRotation(hand.position + yaw * offset, yaw * Quaternion.Euler(0f, yawOffset, 0f));
        }
    }
}
