using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    static class Flow
    {
        public static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime) yield return null;
        }

        public static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();
        public static GuardController Guard(string name) => GameObject.Find(name).GetComponent<GuardController>();

        // Rewinds to Normal (Beat 4-5 state: the Final Corridor wall fire is off) and waits out the white-out.
        public static IEnumerator ToNormal()
        {
            Find<WorldStateController>().Rewind();
            for (float t = 0f; t < AssetLoad.Tuning().rewindEffectTime + 0.1f; t += Time.deltaTime) yield return null;
        }

        public static IEnumerator LoadAsRetry()
        {
            GameFlow.HasSeenIntro = true;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(TestScenes.FacilityPath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            yield return null;
        }
    }

    public sealed class FlowTests
    {
        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        // §14: death reloads the scene and the retry skips the intro (Essence 90).
        [UnityTest]
        public IEnumerator Death_ReloadsAndSkipsIntro()
        {
            yield return TestScenes.LoadFacility();
            GameTuning tuning = AssetLoad.Tuning();
            Flow.Find<Essence>().Kill();
            yield return Flow.Wait(tuning.autoRestartDelay + 1f);
            Assert.IsTrue(GameFlow.HasSeenIntro);
            Assert.AreEqual(FlowState.Playing, Flow.Find<GameFlow>().State);
            Assert.AreEqual(tuning.essenceStart - tuning.introGrazeDamage, Flow.Find<Essence>().Value, 1e-3f);
        }

        // Beat 0 acceptance on a retry: Essence 90, control, G-TIME not yet past D-PLAYER.
        [UnityTest]
        public IEnumerator Retry_StartsAtNinety_Beat0()
        {
            yield return Flow.LoadAsRetry();
            GameTuning tuning = AssetLoad.Tuning();
            Assert.AreEqual(FlowState.Playing, Flow.Find<GameFlow>().State);
            Assert.AreEqual(tuning.essenceStart - tuning.introGrazeDamage, Flow.Find<Essence>().Value, 1e-3f);
            Assert.IsTrue(Flow.Find<ShadowEntity>().HasControl);
            Assert.Greater(Flow.Guard("G-TIME").transform.position.z, 26f, "G-TIME is still to pass D-PLAYER (Z 26)");
            Assert.AreNotEqual(GuardMoveState.Idle, Flow.Guard("G-GLANCE").Motion.State, "the wave is under way");
        }
    }

    public sealed class SpawnerTests
    {
        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        static int Children(GuardSpawner s) => s.GetComponentsInChildren<GuardController>().Length;

        // T-SPN-SLEEP-M: never more than 2 alive.
        [UnityTest]
        public IEnumerator CapRespected_TSPNSLEEPM()
        {
            yield return TestScenes.LoadFacility();
            GuardSpawner sp = GameObject.Find("SP-SLEEP-M").GetComponent<GuardSpawner>();
            Time.timeScale = 20f;
            int max = 0;
            for (float t = 0f; t < 80f; t += Time.deltaTime)
            {
                max = Mathf.Max(max, Children(sp));
                yield return null;
            }
            Assert.GreaterOrEqual(max, 1);
            Assert.LessOrEqual(max, AssetLoad.Tuning().spawnSleepMeltdown.maxAlive);
        }

        // §8.4: transit guards despawn at the end of their route.
        [UnityTest]
        public IEnumerator SpawnedGuardDespawnsAtEnd()
        {
            yield return TestScenes.LoadFacility();
            GuardSpawner sp = GameObject.Find("SP-SLEEP-M").GetComponent<GuardSpawner>();
            Time.timeScale = 20f;
            GuardController first = null;
            for (float t = 0f; t < 40f && first == null; t += Time.deltaTime)
            {
                first = sp.GetComponentInChildren<GuardController>();
                yield return null;
            }
            Assert.IsNotNull(first, "spawned");
            for (float t = 0f; t < 60f && first != null; t += Time.deltaTime) yield return null;
            Assert.IsTrue(first == null, "despawned at the end of its route");
        }
    }

    public sealed class BeatTests
    {
        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        // Beat 1: riding G-TIME from its release delivers the entity into the Time SCP Room, no further input.
        [UnityTest]
        public IEnumerator RideGTime_DeliversToTimeRoom_OneJump_Beat1()
        {
            yield return TestScenes.LoadFacility();
            GuardController gTime = Flow.Guard("G-TIME");
            Flow.Find<IntroSequence>().Run();
            Time.timeScale = 4f;
            // Board at D-PLAYER (Z 26) as the player would; at its release G-TIME stands in the other wave lanterns.
            for (float t = 0f; t < 30f && gTime.transform.position.z > 26.5f; t += Time.deltaTime) yield return null;
            var entity = Flow.Find<ShadowEntity>();
            ShadowArea shadow = gTime.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            var flow = Flow.Find<GameFlow>();
            var essence = Flow.Find<Essence>();
            for (float t = 0f; t < 60f && !gTime.Motion.ReachedEnd && flow.State == FlowState.Playing; t += Time.deltaTime) yield return null;
            Time.timeScale = 1f;
            Assert.AreEqual(FlowState.Playing, flow.State, $"essence {essence.Value:0.0}");
            Assert.IsTrue(gTime.Motion.ReachedEnd);
            Assert.IsTrue(entity.IsRiding);
            Vector3 p = entity.transform.position;
            Assert.IsTrue(AssetLoadLayout().TryGetArea("TimeRoom", out Rect room) && room.Contains(new Vector2(p.x, p.z)), "in the Time SCP Room");
            Assert.Greater(Flow.Find<Essence>().Value, 0f);
        }

        // Beat 3: G-TIME reversed from the Time Room ends at its spawn, with the lever shadow jumpable from its shadow.
        [UnityTest]
        public IEnumerator ReverseGTime_EndsNearLever_Beat3()
        {
            yield return TestScenes.LoadFacility();
            GuardController gTime = Flow.Guard("G-TIME");
            Vector3 spawn = gTime.transform.position;
            gTime.StartRoute();
            Time.timeScale = 20f;
            for (float t = 0f; t < 60f && !gTime.Motion.ReachedEnd; t += Time.deltaTime) yield return null;
            gTime.BeginReverse();
            for (float t = 0f; t < 60f && gTime.Motion.State == GuardMoveState.Reversing; t += Time.deltaTime) yield return null;
            Time.timeScale = 1f;
            Assert.AreEqual(GuardMoveState.Hold, gTime.Motion.State);
            Assert.Less(Vector3.Distance(spawn, gTime.transform.position), 1e-2f, "back at its spawn by the lantern rack");
            var entity = Flow.Find<ShadowEntity>();
            ShadowArea shadow = gTime.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            ShadowArea lever = GameObject.Find("LeverShadow").GetComponent<ShadowArea>();
            Assert.IsTrue(entity.GetComponent<JumpTargeting>().TryGetTarget(lever.Center, out ShadowArea t2, out _), "lever shadow in jump range");
            Assert.AreSame(lever, t2);
        }

        // Beat 4: an SP-SIDE-N guard reversed from the Main Corridor carries the entity through G-SEMI.
        [UnityTest]
        public IEnumerator ReverseThroughGate_ReachesFinalCorridor_Beat4()
        {
            yield return TestScenes.LoadFacility();
            var world = Flow.Find<WorldStateController>();
            world.Rewind();
            yield return Flow.Wait(AssetLoad.Tuning().rewindEffectTime);
            Assert.AreEqual(WorldState.Normal, world.State);

            GuardSpawner side = GameObject.Find("SP-SIDE-N").GetComponent<GuardSpawner>();
            Time.timeScale = 10f;
            GuardController g = null;
            for (float t = 0f; t < 60f; t += Time.deltaTime)
            {
                g = side.GetComponentInChildren<GuardController>();
                if (g != null && g.transform.position.z < 37f) break;
                yield return null;
            }
            Time.timeScale = 1f;
            Assert.IsNotNull(g, "a transit guard came through the gate");

            var entity = Flow.Find<ShadowEntity>();
            ShadowArea shadow = g.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            entity.GetComponent<TimePowers>().Grant();
            var input = entity.GetComponent<InputReader>();
            input.Simulate(consume: false, reverse: true);
            bool gateOpened = false;
            GuardGate gate = Flow.Find<GuardGate>();
            for (float t = 0f; t < 6f && g.transform.position.z < 44f; t += Time.deltaTime)
            {
                gateOpened |= gate.IsOpen;
                yield return null;
            }
            input.Simulate(consume: false);
            Assert.IsTrue(gateOpened, "the gate opened for the reversing guard");
            Assert.Greater(entity.transform.position.z, 41.5f, "in the Final Corridor");
        }

        static FacilityLayout AssetLoadLayout()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<FacilityLayout>("Assets/_Project/Settings/FacilityLayout.asset");
#else
            return null;
#endif
        }
    }
}
