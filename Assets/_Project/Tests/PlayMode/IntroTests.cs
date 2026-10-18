using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    public sealed class IntroTests
    {
        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        // New game: the scene runs the full intro by itself (AD-032).
        static IEnumerator PlayFullIntro()
        {
            GameFlow.HasSeenIntro = false;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(TestScenes.FacilityPath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            var flow = Flow.Find<GameFlow>();
            Time.timeScale = 4f;
            for (float t = 0f; t < 60f && flow.State != FlowState.Playing; t += Time.deltaTime) yield return null;
            Time.timeScale = 1f;
        }

        // Beat 0 acceptance: Essence 90 (T-ESS-INTRO), entity in the Player Room shadow with control,
        // G-TIME not yet past D-PLAYER.
        [UnityTest]
        public IEnumerator AfterIntro_EssenceNinety_ControlOn_Beat0()
        {
            yield return PlayFullIntro();
            GameTuning tuning = AssetLoad.Tuning();
            Assert.AreEqual(FlowState.Playing, Flow.Find<GameFlow>().State);
            Assert.AreEqual(tuning.essenceStart - tuning.introGrazeDamage, Flow.Find<Essence>().Value, 1e-3f);
            var entity = Flow.Find<ShadowEntity>();
            Assert.IsTrue(entity.HasControl);
            Assert.AreEqual("PlayerRoomFloor", entity.Current.name);
            Assert.Greater(Flow.Guard("G-TIME").transform.position.z, 26f, "G-TIME is still to pass D-PLAYER (Z 26)");
        }

        // EN-011: the skip leaves G-TIME where the full intro does, with the same history (Beat 3 depends on it).
        [UnityTest]
        public IEnumerator Skip_MatchesFullRun_GuardPositions()
        {
            yield return PlayFullIntro();
            GuardController full = Flow.Guard("G-TIME");
            Vector3 fullPos = full.transform.position;
            int fullHistory = full.Motion.HistoryCount;

            yield return Flow.LoadAsRetry();
            GuardController skipped = Flow.Guard("G-TIME");
            Assert.Less(Vector3.Distance(fullPos, skipped.transform.position), 0.5f);
            Assert.LessOrEqual(Mathf.Abs(fullHistory - skipped.Motion.HistoryCount), 2);
        }
    }
}
