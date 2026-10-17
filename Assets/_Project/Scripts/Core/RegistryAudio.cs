using UnityEngine;

namespace Nullflame
{
    // Gives a 3D AudioSource its clip from the AssetRegistry (IMPL §22.1): fire loop, klaxon.
    [RequireComponent(typeof(AudioSource))]
    public sealed class RegistryAudio : MonoBehaviour
    {
        [SerializeField] AssetRegistry registry;
        [SerializeField] SfxId id;
        [SerializeField] bool playOnStart;

        void Awake() => GetComponent<AudioSource>().clip = registry.Clip(id);

        void Start()
        {
            if (playOnStart) GetComponent<AudioSource>().Play();
        }
    }
}
