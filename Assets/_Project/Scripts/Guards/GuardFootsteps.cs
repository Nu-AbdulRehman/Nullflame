using UnityEngine;

namespace Nullflame
{
    // One 3D AudioSource per guard; a step every stride the guard travels, so the interval follows walk speed (IMPL §22.1).
    [RequireComponent(typeof(AudioSource))]
    public sealed class GuardFootsteps : MonoBehaviour
    {
        [SerializeField] AssetRegistry registry;
        [SerializeField] float strideLength = 0.75f; // cosmetic: metres per step

        AudioSource source;
        AudioClip clip;
        Vector3 last;
        float travelled;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            clip = registry.Clip(SfxId.Footstep);
            last = transform.position;
        }

        void Update()
        {
            Vector3 p = transform.position;
            travelled += Vector3.Distance(p, last);
            last = p;
            if (travelled < strideLength) return;
            travelled = 0f;
            if (clip != null) source.PlayOneShot(clip);
        }
    }
}
