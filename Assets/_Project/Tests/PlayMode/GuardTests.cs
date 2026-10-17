using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class GuardTests
    {
        static GuardController Guard(string name) => GameObject.Find(name).GetComponent<GuardController>();

        // §7.2: being in a guard's shadow moves the entity with it.
        [UnityTest]
        public IEnumerator RideGuard_EntityMovesWithShadow()
        {
            yield return TestScenes.LoadFacility();
            var entity = Object.FindAnyObjectByType<ShadowEntity>();
            GuardController g = Guard("G-MAIN");
            ShadowArea shadow = g.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            Vector3 start = entity.transform.position;

            g.StartRoute();
            for (float t = 0f; t < 1f; t += Time.deltaTime) yield return null;

            Assert.IsTrue(entity.IsRiding);
            Assert.Greater(Vector3.Distance(start, entity.transform.position), 1f, "moved with the guard");
            Assert.AreEqual(shadow.Center.x, entity.transform.position.x, 1e-3f);
            Assert.AreEqual(shadow.Center.z, entity.transform.position.z, 1e-3f);
        }
    }

    // §8.3 perception. G-MAIN stands idle at (25, 37) facing +Z (north) until the intro releases it at 18 s.
    public sealed class VisionTests
    {
        GuardController guard;
        ShadowEntity entity;
        bool spotted;

        IEnumerator Setup()
        {
            yield return TestScenes.LoadFacility();
            guard = GameObject.Find("G-MAIN").GetComponent<GuardController>();
            guard.GetComponent<GuardVision>().Spotted += _ => spotted = true;
            entity = Object.FindAnyObjectByType<ShadowEntity>();
            spotted = false;
        }

        ShadowArea MakeShadow(Vector3 at, ShadowOpacity opacity)
        {
            ShadowArea s = Object.Instantiate(GameObject.Find("CartShadow").GetComponent<ShadowArea>(), at, Quaternion.identity);
            s.opacity = opacity;
            s.SetSize(new Vector2(1.2f, 1.2f));
            return s;
        }

        static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }

        [UnityTest]
        public IEnumerator OpaqueIdle_NeverSpotted()
        {
            yield return Setup();
            ShadowArea s = MakeShadow(new Vector3(25f, 0f, 39f), ShadowOpacity.Opaque);
            entity.Place(s, s.Center);
            yield return Wait(0.6f);
            Assert.IsFalse(spotted);
        }

        // T-VIEW-GRACE: soft shadows hide nothing, but spotting waits 0.2 s.
        [UnityTest]
        public IEnumerator SoftShadow_Spotted_AfterGrace_TVIEWGRACE()
        {
            yield return Setup();
            ShadowArea s = MakeShadow(new Vector3(25f, 0f, 39f), ShadowOpacity.Soft);
            entity.Place(s, s.Center);
            yield return Wait(0.1f);
            Assert.IsFalse(spotted, "not before the grace time");
            yield return Wait(0.3f);
            Assert.IsTrue(spotted);
        }

        [UnityTest]
        public IEnumerator Jump_InCone_Spotted()
        {
            yield return Setup();
            ShadowArea a = MakeShadow(new Vector3(24.2f, 0f, 39f), ShadowOpacity.Opaque);
            ShadowArea b = MakeShadow(new Vector3(26.2f, 0f, 39f), ShadowOpacity.Opaque);
            entity.Place(a, a.Center);
            yield return Wait(0.3f);
            Assert.IsFalse(spotted, "idle in opaque");
            entity.BeginJump(b, b.Center);
            yield return Wait(0.6f);
            Assert.IsTrue(spotted, "the jump is visible");
        }

        // Only guards with a lit lantern can spot (design doc §8.3 [Default]).
        [UnityTest]
        public IEnumerator Lanternless_DoesNotSpot()
        {
            yield return Setup();
            guard.HasLantern = false;
            ShadowArea s = MakeShadow(new Vector3(25f, 0f, 39f), ShadowOpacity.Soft);
            entity.Place(s, s.Center);
            yield return Wait(0.6f);
            Assert.IsFalse(spotted);
        }
    }

    // §9 Beat 2 design check: at least one window of >= 4 s per loop where both the pedestal and the
    // cabinet-to-pedestal jump path are outside G-TIME's cone. Reads the route and shadows from the scene.
    public sealed class Beat2Tests
    {
        [UnityTest]
        public IEnumerator TimeLoop_HasFourSecondWindow_Beat2()
        {
            yield return TestScenes.LoadFacility();
            GameTuning tuning = AssetLoad.Tuning();
            var post = new List<RoutePoint>();
            GameObject.Find("R_GTime_Post").GetComponent<WaypointRoute>().ToRoutePoints(post);
            ShadowArea pedestal = GameObject.Find("PedestalShadow").GetComponent<ShadowArea>();
            ShadowArea cabinet = GameObject.Find("CabinetShadow").GetComponent<ShadowArea>();

            var probes = new List<Vector3>();
            float hx = pedestal.size.x * 0.5f, hz = pedestal.size.y * 0.5f;
            Vector3 c = pedestal.Center;
            probes.Add(c);
            probes.Add(c + new Vector3(hx, 0f, hz)); probes.Add(c + new Vector3(-hx, 0f, hz));
            probes.Add(c + new Vector3(hx, 0f, -hz)); probes.Add(c + new Vector3(-hx, 0f, -hz));
            for (int i = 0; i <= 8; i++) probes.Add(Vector3.Lerp(cabinet.Center, c, i / 8f));

            var travel = new List<RoutePoint> { post[0] };
            var m = new GuardMotion(travel, post, tuning.guardWalkSpeedMeltdown, tuning.reverseSpeedMultiplier, tuning.historyStep);
            m.Start();
            const float dt = 1f / 60f;
            float window = 0f, best = 0f;
            for (float t = 0f; t < 40f; t += dt)
            {
                m.Tick(dt);
                Vector3 eye = m.Position + Vector3.up * tuning.viewEyeHeight;
                bool safe = true;
                foreach (Vector3 p in probes)
                    if (ViewConeMath.Contains(eye, m.Facing, tuning.viewAngle, tuning.viewRange, p)) { safe = false; break; }
                window = safe ? window + dt : 0f;
                if (t > 12f) best = Mathf.Max(best, window); // skip the first loop's partial start
            }
            Assert.GreaterOrEqual(best, 4f, $"longest safe window {best:0.00} s");
        }
    }
}
