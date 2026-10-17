using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nullflame.EditorTools;

namespace Nullflame.Tests
{
    // FacilityBuilder must rebuild, never duplicate (CLAUDE.md §4.1). Builds into the Test Runner's
    // temporary scene and removes its roots afterwards.
    public sealed class FacilityBuilderTests
    {
        Scene scene;
        FacilityLayout layout;
        GameTuning tuning;
        AssetRegistry registry;

        [SetUp]
        public void SetUp()
        {
            scene = SceneManager.GetActiveScene();
            layout = AssetDatabase.LoadAssetAtPath<FacilityLayout>(FacilityBuilder.LayoutPath);
            tuning = AssetDatabase.LoadAssetAtPath<GameTuning>(FacilityBuilder.TuningPath);
            registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>(FacilityBuilder.RegistryPath);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var name in new[] { FacilityBuilder.LevelRootName, FacilityBuilder.MeltdownRootName, FacilityBuilder.NormalRootName })
            {
                Transform root = FindRoot(name);
                if (root != null) Object.DestroyImmediate(root.gameObject);
            }
        }

        [Test]
        public void Build_TwiceProducesSameChildCount()
        {
            FacilityBuilder.Build(scene, layout, tuning, registry);
            int first = CountAll();
            FacilityBuilder.Build(scene, layout, tuning, registry);

            Assert.Greater(first, 0);
            Assert.AreEqual(first, CountAll());
            Assert.IsNotNull(FindRoot(FacilityBuilder.LevelRootName));
            Assert.IsNotNull(FindRoot(FacilityBuilder.MeltdownRootName));
            Assert.IsNotNull(FindRoot(FacilityBuilder.NormalRootName));
        }

        [Test]
        public void Build_LeavesForeignChildrenOfStateRoots()
        {
            FacilityBuilder.Build(scene, layout, tuning, registry);
            var handPlaced = new GameObject("Guards");
            handPlaced.transform.SetParent(FindRoot(FacilityBuilder.MeltdownRootName), false);
            FacilityBuilder.Build(scene, layout, tuning, registry);

            Assert.IsTrue(handPlaced != null, "hand-placed objects under a state root survive a rebuild");
        }

        int CountAll()
        {
            int n = 0;
            foreach (var root in scene.GetRootGameObjects()) n += root.GetComponentsInChildren<Transform>(true).Length;
            return n;
        }

        Transform FindRoot(string name)
        {
            foreach (var root in scene.GetRootGameObjects()) if (root.name == name) return root.transform;
            return null;
        }
    }
}
