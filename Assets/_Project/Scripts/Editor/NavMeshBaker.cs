using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Nullflame.EditorTools
{
    // Bakes the guard NavMesh from Level_Generated's render meshes (AD-020): walls, doors, props and floors.
    // Hand-placed objects under Shared and the state roots are not part of it, so the Semi-Final Gate doorway
    // stays walkable and the entity (a shadow) never carves it. Re-run after Build Facility.
    public static class NavMeshBaker
    {
        [MenuItem("Nullflame/Bake NavMesh")]
        public static void Bake()
        {
            GameObject root = GameObject.Find("Level_Generated");
            if (root == null) { Debug.LogError("Level_Generated not found. Run Nullflame/Build Facility first."); return; }
            if (!root.TryGetComponent(out NavMeshSurface surface)) surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.layerMask = LayerMask.GetMask("Default", "LevelSolid");
            surface.BuildNavMesh();
            // The runtime API leaves the data in memory; store it as an asset so the scene keeps it.
            const string folder = "Assets/_Project/Scenes/Facility";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Facility");
            AssetDatabase.CreateAsset(surface.navMeshData, folder + "/NavMesh-Level_Generated.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("NavMesh baked for Level_Generated.");
        }
    }
}
