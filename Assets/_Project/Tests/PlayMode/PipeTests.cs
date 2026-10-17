using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    // Final Corridor oil pipes in Normal state (design doc §7.6, Beat 5).
    public sealed class PipeTests
    {
        GameTuning tuning;
        ShadowEntity entity;
        InputReader input;
        Essence essence;
        CrackablePipe pipe;

        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        // keepFinalGuards: leave SP-FINAL-N running (burst test); otherwise no guard can spot or burn the entity.
        IEnumerator Setup(bool keepFinalGuards)
        {
            yield return TestScenes.LoadFacility();
            tuning = AssetLoad.Tuning();
            yield return Flow.ToNormal();
            foreach (string n in new[] { "SP-SIDE-N", "SP-FINAL-N", "G-PATROL" })
                if (n != "SP-FINAL-N" || !keepFinalGuards) GameObject.Find(n).SetActive(false);
            entity = Flow.Find<ShadowEntity>();
            input = entity.GetComponent<InputReader>();
            essence = Flow.Find<Essence>();
            pipe = GameObject.Find("Pipe_W2").GetComponent<CrackablePipe>();
            entity.Place(pipe.strip, pipe.strip.Center);
            yield return null;
        }

        IEnumerator HoldE(float seconds)
        {
            input.Simulate(consume: true);
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
            input.Simulate(consume: false);
            yield return null;
        }

        static int OilDrops()
        {
            int n = 0;
            foreach (var s in Object.FindObjectsByType<ShadowArea>(FindObjectsSortMode.None))
                if (s.destroyedByFire && s.opacity == ShadowOpacity.Opaque) n++;
            return n;
        }

        // T-PIPE-DROPS, T-COST-PIPE: a safe-zone release cracks for 5 and drops 3 opaque oil shadows (Beat 5).
        [UnityTest]
        public IEnumerator SafeRelease_SpawnsThreeOpaqueDrops_TPIPEDROPS()
        {
            yield return Setup(false);
            float before = essence.Value;
            yield return HoldE((tuning.pipeSafeStart + tuning.pipeSafeEnd) * 0.5f);
            Assert.IsTrue(pipe.Cracked);
            Assert.AreEqual(before - tuning.pipeCost, essence.Value, 1e-3f);
            for (float t = 0f; t < tuning.pipeDropDuration + 0.3f; t += Time.deltaTime) yield return null;
            Assert.AreEqual(tuning.pipeDropCount, OilDrops());
        }

        [UnityTest]
        public IEnumerator GreyRelease_DoesNothing()
        {
            yield return Setup(false);
            float before = essence.Value;
            yield return HoldE(tuning.pipeSafeStart * 0.5f);
            Assert.IsFalse(pipe.Cracked);
            Assert.AreEqual(before, essence.Value, 1e-3f);
        }

        // §7.6 [Default]: a cracked crack point can't be cracked again.
        [UnityTest]
        public IEnumerator CrackedPipe_CannotCrackAgain()
        {
            yield return Setup(false);
            yield return HoldE((tuning.pipeSafeStart + tuning.pipeSafeEnd) * 0.5f);
            for (float t = 0f; t < tuning.pipeDropDuration + 0.3f; t += Time.deltaTime) yield return null;
            entity.Place(pipe.strip, pipe.strip.Center); // the drops are smaller, so stand on the strip again
            float before = essence.Value;
            yield return HoldE(tuning.pipeBurstTime + 0.2f);
            Assert.AreEqual(before, essence.Value, 1e-3f, "no second crack or burst");
            Assert.AreEqual(tuning.pipeDropCount, OilDrops());
            Assert.IsNull(Object.FindAnyObjectByType<ThrownLantern>());
        }

        // T-PIPE-BURST, T-PIPE-BURST-COST: holding to 1.5 s bursts for 5 (AD-014); no guards, so nothing is thrown.
        [UnityTest]
        public IEnumerator HoldToBurst_Costs_TPIPEBURSTCOST()
        {
            yield return Setup(false);
            float before = essence.Value;
            yield return HoldE(tuning.pipeBurstTime + 0.1f);
            Assert.IsTrue(pipe.Cracked);
            Assert.AreEqual(before - tuning.pipeBurstCost, essence.Value, 1e-3f);
            Assert.AreEqual(0, OilDrops(), "a burst pours oil but drops no oil shadows");
        }

        static bool InCorridor(GuardController g) =>
            g.transform.position.x > 15.75f && g.transform.position.x < 21.75f && g.transform.position.z > 41.5f && g.transform.position.z < 56f;

        // T-PIPE-BURST: every lantern guard in the Final Corridor throws at the burst point (§7.6, Beat 5).
        [UnityTest]
        public IEnumerator Burst_AllCorridorGuardsThrow()
        {
            yield return Setup(true);
            var spawner = GameObject.Find("SP-FINAL-N");
            var safe = GameObject.Find("PlayerRoomFloor").GetComponent<ShadowArea>();
            entity.Place(safe, safe.Center); // out of the patrollers' sight while they spawn
            GuardController[] guards = null;
            Time.timeScale = 5f;
            for (float t = 0f; t < 40f; t += Time.deltaTime)
            {
                guards = spawner.GetComponentsInChildren<GuardController>();
                if (guards.Length == 2 && InCorridor(guards[0]) && InCorridor(guards[1])) break;
                yield return null;
            }
            Time.timeScale = 1f;
            Assert.AreEqual(2, guards.Length, "both patrollers spawned");
            foreach (var g in guards)
            {
                Assert.IsTrue(InCorridor(g), $"{g.name} in the Final Corridor");
                g.GetComponent<GuardVision>().enabled = false; // only the burst throws
            }
            // The pipe furthest from both patrollers, so no guard shadow passes over it during the hold.
            float best = -1f;
            foreach (var c in Object.FindObjectsByType<CrackablePipe>(FindObjectsSortMode.None))
            {
                float d = Mathf.Min(Vector3.Distance(c.strip.Center, guards[0].transform.position),
                                    Vector3.Distance(c.strip.Center, guards[1].transform.position));
                if (d > best) { best = d; pipe = c; }
            }
            entity.Place(pipe.strip, pipe.strip.Center);

            input.Simulate(consume: true);
            // The hold starts the frame after E goes down; wait on the result, not on a frame count.
            for (float t = 0f; t < tuning.pipeBurstTime + 1f && !pipe.Cracked; t += Time.deltaTime) yield return null;
            input.Simulate(consume: false);

            Assert.IsTrue(pipe.Cracked);
            foreach (var g in guards) Assert.IsFalse(g.HasLantern, $"{g.name} threw its lantern");
            Assert.AreEqual(2, Object.FindObjectsByType<ThrownLantern>(FindObjectsSortMode.None).Length);
        }

        // T-FIRE-RADIUS-OIL: the burst fire grows to 5 m and destroys the oil drops it reaches (§7.6).
        [UnityTest]
        public IEnumerator OilFire_DestroysDrops_TFIRERADIUSOIL()
        {
            yield return Setup(false);
            yield return HoldE((tuning.pipeSafeStart + tuning.pipeSafeEnd) * 0.5f);
            for (float t = 0f; t < tuning.pipeDropDuration + 0.3f; t += Time.deltaTime) yield return null;
            Assert.AreEqual(tuning.pipeDropCount, OilDrops());
            entity.Place(GameObject.Find("Pipe_E4").GetComponent<CrackablePipe>().strip, new Vector3(21.15f, 0f, 52.5f)); // out of reach

            var fire = Object.Instantiate(AssetLoad.Prefab<LanternFire>("P_LanternFire"), pipe.burstPoint.position, Quaternion.identity);
            fire.UseOilRadius();
            for (float t = 0f; t < tuning.fireGrowTime + 0.1f; t += Time.deltaTime) yield return null;
            Assert.AreEqual(tuning.fireOilRadius, fire.Radius, 1e-2f);
            Assert.AreEqual(0, OilDrops());
        }
    }
}
