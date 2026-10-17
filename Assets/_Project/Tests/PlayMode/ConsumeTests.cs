using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Nullflame.Tests
{
    public sealed class ConsumeTests : HazardFixture
    {
        InputReader input;
        TimePowers powers;
        ConsumeAction consume;

        IEnumerator SetupConsume()
        {
            yield return Setup();
            input = entity.GetComponent<InputReader>();
            powers = entity.GetComponent<TimePowers>();
            consume = entity.GetComponent<ConsumeAction>();
        }

        static Consumable SpawnPedestal(Vector3 at)
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Consumable>("Assets/_Project/Prefabs/P_Pedestal.prefab");
            return Object.Instantiate(prefab, at, Quaternion.identity);
#else
            return null;
#endif
        }

        // T-GAIN-GUARD / T-CON-GUARD: eating a guard takes 2 s and gives +25.
        [UnityTest]
        public IEnumerator Guard_GainsTGAINGUARD_AfterTCONGUARD()
        {
            yield return SetupConsume();
            essence.Set(50f);
            ShadowArea shadow = guard.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.guardConsumeTime * 0.5f);
            Assert.AreEqual(50f, essence.Value, 1e-3f, "nothing until complete");
            yield return Wait(tuning.guardConsumeTime * 0.5f + 0.3f);
            Assert.AreEqual(50f + tuning.guardGain, essence.Value, 1e-3f);
            Assert.IsTrue(guard == null, "guard destroyed");
        }

        // §7.3 step 6: releasing early costs nothing and changes nothing.
        [UnityTest]
        public IEnumerator ReleaseEarly_NoCost()
        {
            yield return SetupConsume();
            ShadowArea ped = GameObject.Find("PedestalShadow").GetComponent<ShadowArea>();
            entity.Place(ped, ped.Center);
            input.Simulate(consume: true);
            yield return Wait(1f);
            input.Simulate(consume: false);
            yield return null;
            Assert.AreEqual(tuning.essenceStart, essence.Value, 1e-3f);
            Assert.IsFalse(powers.HasTimePower);
            Assert.AreEqual(EntityState.InShadow, entity.State);
            Assert.IsNotNull(GameObject.Find("PedestalShadow"));
        }

        // §7.3 step 5: another guard seeing the consume cancels it at no cost.
        [UnityTest]
        public IEnumerator Spotted_CancelsAtNoCost()
        {
            yield return SetupConsume();
            guard.GetComponentInChildren<FlameVolume>().GetComponent<Collider>().enabled = false; // isolate the consume cost from lantern drain
            Consumable ped = SpawnPedestal(new Vector3(25f, 0f, 39.2f));
            ShadowArea s = ped.GetComponentInChildren<ShadowArea>();
            entity.Place(s, s.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.viewGrace + 0.15f);
            input.Simulate(consume: false);
            Assert.IsNull(consume.Target, "cancelled");
            Assert.AreEqual(tuning.essenceStart, essence.Value, 1e-3f);
            Assert.IsFalse(powers.HasTimePower);
        }

        // Beat 2: eating the pedestal unseen costs 20 and unlocks time powers.
        [UnityTest]
        public IEnumerator Pedestal_Unseen_CostsAndGrantsPower_Beat2()
        {
            yield return SetupConsume();
            ShadowArea ped = GameObject.Find("PedestalShadow").GetComponent<ShadowArea>();
            entity.Place(ped, ped.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.pedestalConsumeTime + 0.3f);
            Assert.AreEqual(tuning.essenceStart - tuning.pedestalCost, essence.Value, 1e-3f, "T-COST-PEDESTAL");
            Assert.IsTrue(powers.HasTimePower);
            Assert.IsNull(GameObject.Find("Pedestal"), "pedestal and stopwatch eaten together");
        }

        // Beat 2: eating the pedestal inside a cone gets the entity burned.
        [UnityTest]
        public IEnumerator Pedestal_InCone_GetsBurned_Beat2()
        {
            yield return SetupConsume();
            Consumable ped = SpawnPedestal(new Vector3(25f, 0f, 39.2f));
            ShadowArea s = ped.GetComponentInChildren<ShadowArea>();
            entity.Place(s, s.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.viewGrace + 0.15f);
            input.Simulate(consume: false);
            float before = essence.Value;
            yield return Wait(tuning.alertTime + tuning.throwTime + 0.3f);
            Assert.Less(essence.Value, before, "burning in the lantern fire (T-DMG-FIRE, AD-022)");
            Assert.IsFalse(powers.HasTimePower);
        }

        // §7.5: after a consume the entity sits in a void shadow.
        [UnityTest]
        public IEnumerator LeavesVoidShadow()
        {
            yield return SetupConsume();
            ShadowArea shadow = guard.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.guardConsumeTime + 0.3f);
            input.Simulate(consume: false);
            yield return null;
            Assert.IsNotNull(entity.Current);
            Assert.IsTrue(entity.Current.destroyedByFire, "void shadow");
        }

        // T-COST-GATE, T-CON-GATE: the placed Final Gate refuses at Essence 80 and wins above it (Beat 5).
        [UnityTest]
        public IEnumerator Gate_RefusedAtEightyOrLess_TCOSTGATE()
        {
            yield return SetupConsume();
            yield return Flow.ToNormal();
            foreach (string n in new[] { "SP-SIDE-N", "SP-FINAL-N", "G-PATROL" }) GameObject.Find(n).SetActive(false);
            var gate = GameObject.Find("FinalGate").GetComponent<Consumable>();
            ShadowArea shadow = GameObject.Find("GateShadow").GetComponent<ShadowArea>();
            var flow = Flow.Find<GameFlow>();

            essence.Set(tuning.gateCost);
            entity.Place(shadow, shadow.Center);
            input.Simulate(consume: true);
            yield return Wait(tuning.gateConsumeTime + 0.3f);
            Assert.IsTrue(gate != null, "refused at 80");
            Assert.AreEqual(FlowState.Playing, flow.State);

            input.Simulate(consume: false);
            yield return null;
            essence.Set(tuning.gateCost + 1f);
            input.Simulate(consume: true);
            yield return Wait(tuning.gateConsumeTime + 0.3f);
            input.Simulate(consume: false);
            Assert.IsTrue(gate == null, "consumed");
            Assert.AreEqual(FlowState.Won, flow.State);
            Assert.IsTrue(GameObject.Find("Canvas").transform.Find("WinScreen").gameObject.activeSelf);
        }
    }

    public sealed class UiTests : HazardFixture
    {
        // §12: the bar shows a ghost segment of the cost while in a consumable shadow.
        [UnityTest]
        public IEnumerator Bar_ShowsGhostCost()
        {
            yield return Setup();
            ShadowArea ped = GameObject.Find("PedestalShadow").GetComponent<ShadowArea>();
            entity.Place(ped, ped.Center);
            yield return null;
            yield return null;
            Assert.AreEqual(tuning.pedestalCost, essence.PreviewCost, 1e-3f);
            Assert.IsTrue(GameObject.Find("EssenceBar").transform.Find("Ghost").gameObject.activeSelf);
        }

        // §6: an unaffordable consume shows its cost in red.
        [UnityTest]
        public IEnumerator Prompt_RedWhenUnaffordable()
        {
            yield return Setup();
            essence.Set(15f);
            ShadowArea ped = GameObject.Find("PedestalShadow").GetComponent<ShadowArea>();
            entity.Place(ped, ped.Center);
            yield return null;
            yield return null;
            Text label = GameObject.Find("Prompt").GetComponent<Text>();
            StringAssert.Contains("Time SCP", label.text);
            Assert.Greater(label.color.r, 0.9f);
            Assert.Less(label.color.g, 0.5f);
        }
    }
}
