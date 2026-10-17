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

        // A prop's shadow can sit inside the NavMesh clearance band round the prop; stepping out must still work.
        [UnityTest]
        public IEnumerator PropShadow_CanStepOutToFloor()
        {
            yield return Setup();
            Vector3 crate = GameObject.Find("Crate1").GetComponentInChildren<Renderer>().bounds.center;
            ShadowArea shadow = null;
            float best = float.MaxValue;
            foreach (ShadowArea s in Object.FindObjectsByType<ShadowArea>(FindObjectsSortMode.None))
            {
                float d = Vector3.Distance(s.Center, crate);
                if (!s.isMoving && d < best) { best = d; shadow = s; }
            }
            entity.Place(shadow, shadow.Center);
            float startX = entity.transform.position.x;
            for (int i = 0; i < 60; i++) entity.Move(new Vector3(0.05f, 0f, 0f)); // east, into the corridor
            Assert.Greater(entity.transform.position.x, startX + 0.5f, "walked out of the crate's shadow");
            Assert.IsTrue(entity.OnFloor);
        }

        // A jump with no shadow under the reticle lands on bare floor where the floor is allowed.
        [UnityTest]
        public IEnumerator JumpToFloor_Lands_OnFloor()
        {
            yield return Setup();
            PlaceAt(new Vector3(19f, 0f, 20f));
            var jump = entity.GetComponent<JumpTargeting>();
            Assert.IsTrue(jump.TryGetTarget(new Vector3(19f, 0f, 23.5f), out ShadowArea target, out Vector3 landing));
            Assert.IsNull(target);
            entity.BeginJump(target, landing);
            yield return Wait(tuning.jumpTime + 0.1f);
            Assert.IsTrue(entity.OnFloor);
            Assert.AreEqual(23.5f, entity.transform.position.z, 0.05f);
            yield return Flow.ToNormal();
            entity.Place(GameObject.Find("Pipe_W2").GetComponent<CrackablePipe>().strip, new Vector3(16.35f, 0f, 45f));
            // (18.6, 46.5) is more than T-JMP-SNAP from every other strip, so nothing snaps: bare corridor floor.
            Assert.IsFalse(jump.TryGetTarget(new Vector3(18.6f, 0f, 46.5f), out ShadowArea none, out _), "no floor jump in the Final Corridor");
            Assert.IsNull(none);
        }

        // After a jump from a guard's shadow to an object's, the consume prompt for the guard goes away.
        [UnityTest]
        public IEnumerator GuardToObject_PromptClears()
        {
            yield return Setup();
            guard.GetComponent<GuardVision>().enabled = false;
            ShadowArea ride = guard.GetComponentInChildren<ShadowArea>();
            entity.Place(ride, ride.Center);
            var consume = entity.GetComponent<ConsumeAction>();
            var label = GameObject.Find("Canvas").transform.Find("HUD/Prompt").GetComponent<UnityEngine.UI.Text>();
            yield return null;
            Assert.IsNotNull(consume.Available, "guard consumable offered while riding");
            ShadowArea crate = MakeShadow(guard.transform.position + new Vector3(3f, 0f, 0f));
            var jump = entity.GetComponent<JumpTargeting>();
            Assert.IsTrue(jump.TryGetTarget(crate.Center, out ShadowArea target, out Vector3 landing));
            Assert.AreSame(crate, target);
            entity.BeginJump(target, landing);
            yield return Wait(tuning.jumpTime + 0.1f);
            Assert.AreSame(crate, entity.Current);
            Assert.IsNull(consume.Available);
            Assert.IsFalse(label.enabled, "no consume prompt on an object with nothing to consume");
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
