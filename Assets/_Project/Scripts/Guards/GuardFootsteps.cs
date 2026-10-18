using UnityEngine;

namespace Nullflame
{
    // One 3D AudioSource per guard: the walking loop plays while the guard moves and stops when it stands (IMPL §22.1).
    [RequireComponent(typeof(AudioSource))]
    public sealed class GuardFootsteps : MonoBehaviour
    {
        [SerializeField] AssetRegistry registry;

        AudioSource source;
        Vector3 last;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.clip = registry.Clip(SfxId.Footstep);
            source.loop = true;
            last = transform.position;
        }

        void Update()
        {
            Vector3 p = transform.position;
            bool moving = (p - last).sqrMagnitude > 1e-6f;
            last = p;
            if (source.clip == null || moving == source.isPlaying) return;
            if (moving) source.Play();
            else source.Stop();
        }
    }
}
