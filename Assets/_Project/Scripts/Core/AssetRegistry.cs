using UnityEngine;

namespace Nullflame
{
    // One slot per visual purpose (CLAUDE.md §5.2, IMPL §23). Each slot points at a pack prefab,
    // a prefab variant under Art/, or a PH_ placeholder. An empty slot means "use a primitive".
    // Slots are added in the build step that first reads them.
    [CreateAssetMenu(menuName = "Nullflame/Asset Registry", fileName = "AssetRegistry")]
    public sealed class AssetRegistry : ScriptableObject
    {
        [Header("Facility kit")]
        public GameObject floorTile;
        public GameObject wallSegment;
        public GameObject doorFrame;
        public GameObject containmentDoor;
        public GameObject showOnlyDoor;

        [Header("Props")]
        public GameObject crate;
        public GameObject cart;
        public GameObject cabinet;
        public GameObject bunk;
        public GameObject locker;
        public GameObject lanternRack;
        public GameObject labBench;
        public GameObject oilPipe;
        [Tooltip("Spare lanterns stood on the lantern rack (look only).")]
        public GameObject rackLantern;

        [Header("Lights")]
        public GameObject ceilingLight;
        public GameObject alarmLight;
        public GameObject wallFireLantern;

        [Header("VFX")]
        public GameObject lanternFireVfx;
        public GameObject fireSmallVfx;
        public GameObject lanternSmashVfx;

        [Header("Audio (IMPL §22.2)")]
        public SfxClip[] sounds;

        [System.Serializable]
        public struct SfxClip
        {
            public SfxId id;
            public AudioClip clip;
        }

        public AudioClip Clip(SfxId id)
        {
            if (sounds == null) return null;
            for (int i = 0; i < sounds.Length; i++)
                if (sounds[i].id == id) return sounds[i].clip;
            return null;
        }
    }
}
