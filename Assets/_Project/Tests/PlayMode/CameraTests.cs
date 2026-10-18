using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    public static class TestScenes
    {
        public const string FacilityPath = "Assets/_Project/Scenes/Facility.unity";

        // Loads Facility (IMPL §24) and cancels the intro it starts (AD-032), so guards stay at their spawns
        // and Essence is at its start value. Editor only.
        public static IEnumerator LoadFacility()
        {
            GameFlow.HasSeenIntro = false;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(FacilityPath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            Object.FindAnyObjectByType<IntroSequence>().Cancel();
            Object.FindAnyObjectByType<GameFlow>().BeginPlaying();
            yield return null;
        }
    }

    public sealed class CameraTests
    {
        // T-CAM-PITCH-MIN / T-CAM-PITCH-MAX: pitch never leaves the clamp, whatever the mouse does.
        [UnityTest]
        public IEnumerator Pitch_IsClamped_TCAMPITCH()
        {
            yield return TestScenes.LoadFacility();
            var cam = Object.FindAnyObjectByType<OrbitCamera>();
            Assert.IsNotNull(cam, "OrbitCamera in Facility");
            var tuning = AssetDatabaseLoad();

            Assert.AreEqual(tuning.cameraPitchStart, cam.Pitch, 1e-3f, "T-CAM-PITCH-START");
            cam.Rotate(new Vector2(0f, -100000f));
            Assert.AreEqual(tuning.cameraPitchMax, cam.Pitch, 1e-3f, "T-CAM-PITCH-MAX");
            cam.Rotate(new Vector2(0f, 100000f));
            Assert.AreEqual(tuning.cameraPitchMin, cam.Pitch, 1e-3f, "T-CAM-PITCH-MIN");
        }

        static GameTuning AssetDatabaseLoad()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameTuning>("Assets/_Project/Settings/GameTuning.asset");
#else
            return null;
#endif
        }
    }
}
