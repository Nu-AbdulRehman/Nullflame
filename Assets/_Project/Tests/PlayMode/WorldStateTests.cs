using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    // World states and the facility rewind (design doc §7.4, §10, Beat 3).
    public sealed class WorldStateTests : HazardFixture
    {
        WorldStateController world;
        InputReader input;
        TimePowers powers;
        ShadowArea lever;

        IEnumerator SetupWorld()
        {
            yield return Setup();
            guard.gameObject.SetActive(false); // G-MAIN would see the channel at the lever
            world = Object.FindAnyObjectByType<WorldStateController>();
            input = entity.GetComponent<InputReader>();
            powers = entity.GetComponent<TimePowers>();
            lever = GameObject.Find("LeverShadow").GetComponent<ShadowArea>();
        }

        IEnumerator ChannelRewind()
        {
            entity.Place(lever, lever.Center);
            input.Simulate(consume: false, rewind: true);
            yield return Wait(tuning.rewindChannelTime + tuning.rewindEffectTime * 0.5f + 0.3f);
            input.Simulate(consume: false);
        }

        // T-COST-REWIND + Beat 3: rewinding at the lever with the power swaps Meltdown for Normal.
        [UnityTest]
        public IEnumerator Rewind_SwapsRoots_TCOSTREWIND_Beat3()
        {
            yield return SetupWorld();
            powers.Grant();
            yield return ChannelRewind();
            Assert.AreEqual(WorldState.Normal, world.State);
            Assert.IsNull(GameObject.Find("State_Meltdown"), "Meltdown root destroyed");
            Assert.IsNotNull(GameObject.Find("State_Normal"), "Normal root active");
            Assert.AreEqual(tuning.essenceStart - tuning.rewindCost, essence.Value, 1e-3f);
            Assert.AreSame(lever, entity.Current, "the entity stays in the shared lever shadow");
        }

        // Beat 3 result: the Final Corridor wall fire is gone.
        [UnityTest]
        public IEnumerator Rewind_TurnsOffWallFire_Beat3()
        {
            yield return SetupWorld();
            powers.Grant();
            yield return ChannelRewind();
            foreach (var f in Object.FindObjectsByType<FlameVolume>(FindObjectsSortMode.None))
                Assert.AreNotEqual(FlameKind.WallFire, f.kind);
        }

        // §7.4: one-way; a second rewind does nothing.
        [UnityTest]
        public IEnumerator Rewind_IsOneWay()
        {
            yield return SetupWorld();
            powers.Grant();
            yield return ChannelRewind();
            float e = essence.Value;
            yield return ChannelRewind();
            Assert.AreEqual(WorldState.Normal, world.State);
            Assert.AreEqual(e, essence.Value, 1e-3f, "no second cost");
        }

        // §10: guards from the old state are destroyed.
        [UnityTest]
        public IEnumerator Rewind_DestroysMeltdownGuards()
        {
            yield return SetupWorld();
            GameObject gTime = guard.transform.parent.Find("G-TIME").gameObject; // inactive in this fixture
            powers.Grant();
            yield return ChannelRewind();
            Assert.IsTrue(gTime == null, "G-TIME destroyed");
            Assert.IsTrue(guard == null, "G-MAIN destroyed");
            Assert.IsNotNull(GameObject.Find("G-PATROL"), "Normal guards present");
        }

        // Beat 3: the rewind only works from the lever's shadow, with the time power.
        [UnityTest]
        public IEnumerator Rewind_OnlyFromLever_WithPower_Beat3()
        {
            yield return SetupWorld();
            yield return ChannelRewind(); // no power
            Assert.AreEqual(WorldState.Meltdown, world.State, "needs the time power");

            powers.Grant();
            PlaceAt(new Vector3(26f, 0f, 33f));
            input.Simulate(consume: false, rewind: true);
            yield return Wait(tuning.rewindChannelTime + 0.5f);
            input.Simulate(consume: false);
            Assert.AreEqual(WorldState.Meltdown, world.State, "needs the lever's shadow");
            Assert.AreEqual(tuning.essenceStart, essence.Value, 1e-3f);
        }
    }
}
