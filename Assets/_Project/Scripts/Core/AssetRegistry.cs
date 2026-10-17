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

        [Header("Lights")]
        public GameObject ceilingLight;
    }
}
