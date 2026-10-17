using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    // Local reverse (design doc §7.4). Rides G-MAIN after it has walked for a while to build history.
    public sealed class TimeTests : HazardFixture
    {
        InputReader input;
        TimePowers powers;

        IEnumerator RideWalkingGuard(float walk)
        {
            yield return Setup();
            input = entity.GetComponent<InputReader>();
            powers = entity.GetComponent<TimePowers>();
            ShadowArea shadow = guard.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            guard.StartRoute();
            yield return Wait(walk);
        }

        // T-COST-REVERSE: 5 Essence per second while held.
        [UnityTest]
        public IEnumerator Reverse_CostsTCOSTREVERSE_PerSecond()
        {
            yield return RideWalkingGuard(2.5f);
            powers.Grant();
            float before = essence.Value;
            input.Simulate(consume: false, reverse: true);
            yield return Wait(1f);
            Assert.AreEqual(GuardMoveState.Reversing, guard.Motion.State);
            Assert.AreEqual(tuning.reverseCostPerSecond, before - essence.Value, 0.5f);
        }

        // T-ESS-FLOOR: the reverse stops at 10 and the guard holds.
        [UnityTest]
        public IEnumerator Reverse_StopsAtFloor_TESSFLOOR()
        {
            yield return RideWalkingGuard(2.5f);
            powers.Grant();
            essence.Set(tuning.essenceFloor + 2f);
            input.Simulate(consume: false, reverse: true);
            yield return Wait(1f);
            Assert.AreEqual(tuning.essenceFloor, essence.Value, 1e-3f);
            Assert.AreEqual(GuardMoveState.Hold, guard.Motion.State);
        }

        // Beat 3: releasing R leaves the guard on Hold, still carrying the entity.
        [UnityTest]
        public IEnumerator Release_LeavesGuardOnHold_Beat3()
        {
            yield return RideWalkingGuard(2.5f);
            powers.Grant();
            input.Simulate(consume: false, reverse: true);
            yield return Wait(0.5f);
            input.Simulate(consume: false, reverse: false);
            yield return null;
            Vector3 p = guard.transform.position;
            yield return Wait(1f);
            Assert.AreEqual(GuardMoveState.Hold, guard.Motion.State);
            Assert.Less(Vector3.Distance(p, guard.transform.position), 1e-3f, "stands still");
            Assert.IsTrue(entity.IsRiding);
        }

        // §7.4: without the time power, R does nothing.
        [UnityTest]
        public IEnumerator NoPower_RDoesNothing()
        {
            yield return RideWalkingGuard(1f);
            input.Simulate(consume: false, reverse: true);
            yield return Wait(0.5f);
            Assert.AreEqual(GuardMoveState.Travel, guard.Motion.State);
            Assert.AreEqual(tuning.essenceStart, essence.Value, 1e-3f);
        }
    }

    // Guard-only gate (design doc §7.8, Beat 4).
    public sealed class GateTests : HazardFixture
    {
        GuardGate gate;

        IEnumerator SetupGate()
        {
            yield return Setup();
            gate = Object.FindAnyObjectByType<GuardGate>();
        }

        // A guard walking north to south through G-SEMI on a temporary route.
        GuardController SpawnGuardThroughGate()
        {
            var routeGo = new GameObject("TestRoute");
            routeGo.AddComponent<WaypointRoute>();
            foreach (float z in new[] { 44f, 36f })
            {
                var w = new GameObject("W");
                w.transform.SetParent(routeGo.transform, false);
                w.transform.position = new Vector3(18.75f, 0f, z);
            }
            var holder = new GameObject("Inactive");
            holder.SetActive(false);
            GuardController g = Object.Instantiate(guard, holder.transform);
            g.travelRoute = routeGo.GetComponent<WaypointRoute>();
            g.postRoute = null;
            g.startOnEnable = true;
            g.transform.SetParent(null);
            g.gameObject.SetActive(true);
            return g;
        }

        // Beat 4: the gate never opens for the player alone.
        [UnityTest]
        public IEnumerator PlayerAlone_GateStaysClosed_Beat4()
        {
            yield return SetupGate();
            guard.gameObject.SetActive(false);
            PlaceAt(new Vector3(18.75f, 0f, 39f));
            yield return Wait(1f);
            Assert.IsFalse(gate.IsOpen);
        }

        // Beat 4: a reversed guard opens it.
        [UnityTest]
        public IEnumerator ReversedGuard_OpensGate_Beat4()
        {
            yield return SetupGate();
            guard.gameObject.SetActive(false);
            GuardController g = SpawnGuardThroughGate();
            yield return Wait(8f / tuning.guardWalkSpeedMeltdown + tuning.gateCloseDelay + 0.5f);
            Assert.IsFalse(gate.IsOpen, "closed again after the guard left");
            g.BeginReverse();
            yield return Wait(1.2f);
            Assert.AreEqual(GuardMoveState.Reversing, g.Motion.State);
            Assert.IsTrue(gate.IsOpen, "opens for the reversing guard");
        }

        // §7.8: a closed gate blocks the jump line of sight.
        [UnityTest]
        public IEnumerator ClosedGate_BlocksJump()
        {
            yield return SetupGate();
            guard.gameObject.SetActive(false);
            PlaceAt(new Vector3(18.75f, 0f, 39f));
            ShadowArea far = MakeShadow(new Vector3(18.75f, 0f, 43.5f));
            var targeting = entity.GetComponent<JumpTargeting>();
            Assert.IsFalse(targeting.TryGetTarget(far.Center, out ShadowArea t, out _));
            Assert.AreSame(far, t);
        }
    }
}
