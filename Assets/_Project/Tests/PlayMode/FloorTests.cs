using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    // AD-023: bare-floor movement outside the Final Corridor.
    public sealed class FloorTests : HazardFixture
    {
        // Stepping out of a shadow's footprint puts the entity on the floor; stepping into one attaches.
        [UnityTest]
        public IEnumerator WalkOut_OnFloor_WalkIn_Attaches()
        {
            yield return Setup();
            PlaceAt(new Vector3(19f, 0f, 20f)); // Main Corridor, no guard nearby
            ShadowArea other = MakeShadow(new Vector3(20f, 0f, 22f));
            entity.Move(new Vector3(0f, 0f, 1.5f));
            Assert.IsTrue(entity.OnFloor, "on bare floor");
            Assert.AreEqual(21.5f, entity.transform.position.z, 0.05f);
            entity.Move(new Vector3(1f, 0f, 0.5f));
            Assert.AreSame(other, entity.Current, "attached to the shadow it walked into");
        }

        // Walls and props block floor steps (guard NavMesh).
        [UnityTest]
        public IEnumerator WallBlocksFloorStep()
        {
            yield return Setup();
            PlaceAt(new Vector3(19f, 0f, 20f));
            entity.Move(new Vector3(1.5f, 0f, 0f));
            Assert.IsTrue(entity.OnFloor);
            Vector3 before = entity.transform.position;
            entity.Move(new Vector3(3f, 0f, 0f)); // through the corridor's east wall into the Lab
            Assert.AreEqual(before.x, entity.transform.position.x, 1e-3f, "stopped by the wall");
        }

        // The Final Corridor allows no floor: a step out of a pipe strip is clamped inside it.
        [UnityTest]
        public IEnumerator FinalCorridor_NoFloor()
        {
            yield return Setup();
            yield return Flow.ToNormal();
            ShadowArea strip = GameObject.Find("Pipe_W2").GetComponent<CrackablePipe>().strip;
            entity.Place(strip, strip.Center);
            entity.Move(new Vector3(2f, 0f, 0f));
            Assert.IsFalse(entity.OnFloor);
            Assert.AreSame(strip, entity.Current);
            Assert.LessOrEqual(entity.transform.position.x, strip.Center.x + strip.size.x * 0.5f);
        }

        // T-DMG-FLOOR: lantern light on bare floor drains the floor rate, not the shadow rate.
        [UnityTest]
        public IEnumerator FloorLight_Drains_TDMGFLOOR()
        {
            yield return Setup();
            guard.GetComponent<GuardVision>().enabled = false;
            PlaceAt(new Vector3(25f, 0f, 39f), ShadowOpacity.Soft);
            entity.Move(new Vector3(0.8f, 0f, 0f));
            Assert.IsTrue(entity.OnFloor);
            yield return null;
            float before = essence.Value;
            yield return Wait(1f);
            Assert.AreEqual(tuning.floorLightDps, before - essence.Value, 3f);
        }
    }
}
