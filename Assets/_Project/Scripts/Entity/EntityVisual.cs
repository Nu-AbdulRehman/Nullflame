using UnityEngine;

namespace Nullflame
{
    // Black blob in a shadow, black teardrop mid-jump (design doc §5.1, IMPL §7.6). Cosmetic only.
    public sealed class EntityVisual : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] ShadowEntity entity;
        [SerializeField] Transform blob;
        [SerializeField] Transform teardrop;
        [SerializeField] float wobbleAmount = 0.06f;
        [SerializeField] float wobbleSpeed = 3f;

        const float BlobHeight = 0.02f; // IMPL §2.8 draw order

        Vector3 lastPosition;

        void LateUpdate()
        {
            bool jumping = entity.State == EntityState.Jumping;
            blob.gameObject.SetActive(!jumping && entity.State != EntityState.Inactive);
            teardrop.gameObject.SetActive(jumping);

            Vector3 p = entity.transform.position;
            if (jumping)
            {
                Vector3 v = p - lastPosition;
                if (v.sqrMagnitude > 1e-8f) teardrop.rotation = Quaternion.LookRotation(v);
            }
            else
            {
                float s = tuning.entitySize * (1f + wobbleAmount * Mathf.Sin(Time.time * wobbleSpeed));
                blob.localScale = new Vector3(s, s, 1f);
                blob.position = new Vector3(p.x, BlobHeight, p.z);
            }
            lastPosition = p;
        }
    }
}
