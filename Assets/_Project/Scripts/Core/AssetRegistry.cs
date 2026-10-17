using UnityEngine;

namespace Nullflame
{
    // One slot per visual purpose (CLAUDE.md §5.2, IMPL §23). Each slot points at a pack prefab,
    // a prefab variant under Art/, or a PH_ placeholder. An empty slot means "use a primitive".
    // Audio slots are added in B10.
    [CreateAssetMenu(menuName = "Nullflame/Asset Registry", fileName = "AssetRegistry")]
    public sealed class AssetRegistry : ScriptableObject
    {
        [Header("Facility kit")]
        public GameObject floorTile;
        public GameObject wallSegment;
        public GameObject doorFrame;
        public GameObject containmentDoor;
        public GameObject gatePanel;
        public GameObject finalGate;
        public GameObject showOnlyDoor;

        [Header("Props")]
        public GameObject crate;
        public GameObject cart;
        public GameObject cabinet;
        public GameObject pedestal;
        public GameObject stopwatch;
        public GameObject bunk;
        public GameObject locker;
        public GameObject lanternRack;
        public GameObject leverPanel;
        public GameObject leverHandle;
        public GameObject labBench;
        public GameObject oilPipe;

        [Header("Lights")]
        public GameObject ceilingLight;
        public GameObject alarmLight;
        public GameObject wallFireLantern;

        [Header("Guards")]
        public GameObject guardBody;
        public RuntimeAnimatorController guardController;
        public GameObject guardLantern;

        [Header("VFX")]
        public GameObject lanternFireVfx;
        public GameObject fireSmallVfx;
        public GameObject pipeBurstVfx;
        public GameObject lanternSmashVfx;
        public GameObject oilDripVfx;

        [Header("Decals")]
        public Material shadowDecalMaterial;
        public Material entityBlobMaterial;
        public Material oilDropMaterial;
        public Material viewConeMaterial;
        public Material outlineMaterial;
        public Mesh viewConeMesh;
        public Mesh teardropMesh;
    }
}
