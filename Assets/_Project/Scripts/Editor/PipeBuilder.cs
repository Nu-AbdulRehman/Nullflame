using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nullflame.EditorTools
{
    // Places one P_CrackablePipe per FacilityLayout crack point under Shared/Pipes_Generated (IMPL §18.1).
    // Idempotent: clears and rebuilds only that root.
    public static class PipeBuilder
    {
        const string PipePrefabPath = "Assets/_Project/Prefabs/P_CrackablePipe.prefab";
        const string RootName = "Pipes_Generated";
        const float PipeRadius = 0.15f;

        [MenuItem("Nullflame/Build Pipes")]
        static void BuildMenu()
        {
            var layout = AssetDatabase.LoadAssetAtPath<FacilityLayout>(FacilityBuilder.LayoutPath);
            var tuning = AssetDatabase.LoadAssetAtPath<GameTuning>(FacilityBuilder.TuningPath);
            var registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>(FacilityBuilder.RegistryPath);
            var prefab = AssetDatabase.LoadAssetAtPath<CrackablePipe>(PipePrefabPath);
            if (layout == null || tuning == null || registry == null || prefab == null)
            {
                Debug.LogError("PipeBuilder: missing FacilityLayout, GameTuning, AssetRegistry or P_CrackablePipe.");
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            GameObject shared = null;
            foreach (var go in scene.GetRootGameObjects()) if (go.name == "Shared") shared = go;
            if (shared == null)
            {
                Debug.LogError("PipeBuilder: no Shared root in the active scene.");
                return;
            }
            Build(shared.transform, layout, tuning, registry, prefab);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        public static void Build(Transform shared, FacilityLayout layout, GameTuning tuning, AssetRegistry registry, CrackablePipe prefab)
        {
            Transform root = shared.Find(RootName);
            if (root != null) Object.DestroyImmediate(root.gameObject);
            root = new GameObject(RootName).transform;
            root.SetParent(shared, false);

            foreach (CrackPointSpec c in layout.crackPoints)
            {
                float zMid = (c.zMin + c.zMax) * 0.5f;
                float length = c.zMax - c.zMin;
                var pipe = (CrackablePipe)PrefabUtility.InstantiatePrefab(prefab, root);
                pipe.name = "Pipe_" + c.id;
                pipe.transform.position = new Vector3(c.stripX, 0f, zMid);
                Record(pipe.transform);

                pipe.strip.SetSize(new Vector2(layout.pipeStripWidth, length));
                Record(pipe.strip);

                int n = pipe.dropAnchors.Length;
                if (n != tuning.pipeDropCount)
                    Debug.LogError($"PipeBuilder: P_CrackablePipe has {n} drop anchors, T-PIPE-DROPS is {tuning.pipeDropCount}.");
                for (int i = 0; i < n; i++)
                {
                    pipe.dropAnchors[i].position = new Vector3(c.dropX, 0f, zMid + (i - (n - 1) * 0.5f) * tuning.pipeDropSpacing);
                    Record(pipe.dropAnchors[i]);
                }
                pipe.burstPoint.position = new Vector3(c.wallFaceX, 0f, zMid);
                Record(pipe.burstPoint);

                // The pipe sits against the wall face, 2.5 m up, running along Z.
                float side = Mathf.Sign(c.wallFaceX - c.stripX);
                var pipePos = new Vector3(c.wallFaceX - side * PipeRadius, layout.pipeHeight, zMid);
                var drip = pipe.GetComponentInChildren<ParticleSystem>(true);
                if (drip != null)
                {
                    drip.transform.position = pipePos;
                    Record(drip.transform);
                }
                if (registry.oilPipe == null) continue;
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(registry.oilPipe, pipe.transform);
                visual.transform.SetPositionAndRotation(pipePos, Quaternion.Euler(90f, 0f, 0f));
                visual.transform.localScale = new Vector3(PipeRadius * 2f, length * 0.5f, PipeRadius * 2f); // Unity cylinder is 2 m tall
            }
        }

        static void Record(Object o) => PrefabUtility.RecordPrefabInstancePropertyModifications(o);
    }
}
