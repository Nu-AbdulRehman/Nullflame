using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class GuardMotionTests
    {
        const float Walk = 2f, RevMul = 2f, Step = 0.25f, Dt = 1f / 60f;

        static RoutePoint P(float x, float z, float dwell = 0f) =>
            new RoutePoint { Position = new Vector3(x, 0f, z), Dwell = dwell };

        // An L-shaped route: 4 m east then 4 m north (4 s at 2 m/s).
        static List<RoutePoint> LRoute() => new List<RoutePoint> { P(0, 0), P(4, 0), P(4, 4) };

        static GuardMotion Run(List<RoutePoint> travel, List<RoutePoint> post, float seconds)
        {
            var m = new GuardMotion(travel, post, Walk, RevMul, Step);
            m.Start();
            for (float t = 0f; t < seconds; t += Dt) m.Tick(Dt);
            return m;
        }

        // T-GRD-HISTORY: one sample per 0.25 s of walking plus the corners.
        [Test]
        public void Travel_RecordsHistoryEveryStep_TGRDHISTORY()
        {
            var m = Run(LRoute(), null, 1f);
            // start + samples at 0.25, 0.5, 0.75, 1.0 s
            Assert.AreEqual(5, m.HistoryCount);
            Assert.AreEqual(0.5f, m.History[1].x, 1e-3f, "first sample 0.25 s at 2 m/s");
        }

        [Test]
        public void Travel_RecordsCorners()
        {
            var m = Run(LRoute(), null, 5f);
            Assert.IsTrue(HasPoint(m.History, new Vector3(4f, 0f, 0f)), "corner (4,0)");
            Assert.IsTrue(HasPoint(m.History, new Vector3(4f, 0f, 4f)), "end (4,4)");
        }

        [Test]
        public void Post_DoesNotRecordHistory()
        {
            var post = new List<RoutePoint> { P(4, 4), P(6, 4) };
            var m = Run(LRoute(), post, 4.5f);
            Assert.AreEqual(GuardMoveState.Post, m.State);
            int count = m.HistoryCount;
            for (int i = 0; i < 300; i++) m.Tick(Dt);
            Assert.AreEqual(count, m.HistoryCount);
        }

        // Beat 3 / CLAUDE.md §3: every reversed position lies on the forward polyline.
        [Test]
        public void Reverse_RetracesExactPath_Beat3()
        {
            var m = Run(LRoute(), null, 3f);
            var forward = new List<Vector3>(m.History);
            forward.Add(m.Position);
            m.BeginReverse();
            for (int i = 0; i < 300 && m.State == GuardMoveState.Reversing; i++)
            {
                m.Tick(Dt);
                Assert.Less(DistanceToPolyline(m.Position, forward), 1e-3f, "frame " + i);
            }
            Assert.AreEqual(Vector3.zero, m.Position, "ends at spawn");
        }

        // T-REV-SPEED: reverse runs at 2× walk speed.
        [Test]
        public void Reverse_RunsAtDoubleSpeed_TREVSPEED()
        {
            var m = Run(new List<RoutePoint> { P(0, 0), P(10, 0) }, null, 3f); // about x = 6
            float x0 = m.Position.x;
            m.BeginReverse();
            m.Tick(1f);
            Assert.AreEqual(x0 - Walk * RevMul, m.Position.x, 1e-3f);
        }

        [Test]
        public void Reverse_EmptyHistory_GoesToHold()
        {
            var m = Run(LRoute(), null, 1f);
            m.BeginReverse();
            for (int i = 0; i < 600; i++) m.Tick(Dt);
            Assert.AreEqual(GuardMoveState.Hold, m.State);
            Assert.AreEqual(0, m.HistoryCount);
        }

        [Test]
        public void EndReverse_HoldsFacingOriginalDirection()
        {
            var m = Run(new List<RoutePoint> { P(0, 0), P(10, 0) }, null, 3f);
            m.BeginReverse();
            m.Tick(0.5f);
            m.EndReverse();
            Assert.AreEqual(GuardMoveState.Hold, m.State);
            Assert.AreEqual(1f, m.Facing.x, 1e-3f, "still facing east, the way it walked");
            Vector3 p = m.Position;
            m.Tick(1f);
            Assert.AreEqual(p, m.Position, "Hold stands still");
        }

        [Test]
        public void Hold_ThenReverseAgain_Continues()
        {
            var m = Run(new List<RoutePoint> { P(0, 0), P(10, 0) }, null, 3f);
            m.BeginReverse();
            m.Tick(0.5f);
            m.EndReverse();
            float x = m.Position.x;
            m.BeginReverse();
            m.Tick(0.25f);
            Assert.AreEqual(x - 1f, m.Position.x, 1e-3f);
        }

        [Test]
        public void Dwell_AddsNoHistory()
        {
            var route = new List<RoutePoint> { P(0, 0), P(1, 0, 3f), P(2, 0) };
            var m = Run(route, null, 0.5f); // reached (1,0), dwelling
            int count = m.HistoryCount;
            for (int i = 0; i < 120; i++) m.Tick(Dt); // 2 s more dwell
            Assert.AreEqual(count, m.HistoryCount);
            Assert.AreEqual(1f, m.Position.x, 1e-4f);
        }

        [Test]
        public void Paused_FreezesEverything()
        {
            var m = Run(LRoute(), null, 1f);
            Vector3 p = m.Position;
            int count = m.HistoryCount;
            m.Paused = true;
            m.Tick(1f);
            Assert.AreEqual(p, m.Position);
            Assert.AreEqual(count, m.HistoryCount);
        }

        [Test]
        public void ArrivedAtEnd_FiresOnce()
        {
            int fired = 0;
            var m = new GuardMotion(LRoute(), new List<RoutePoint> { P(4, 4), P(6, 4) }, Walk, RevMul, Step);
            m.ArrivedAtEnd += () => fired++;
            m.Start();
            for (int i = 0; i < 600; i++) m.Tick(Dt);
            Assert.AreEqual(1, fired);
            Assert.IsTrue(m.ReachedEnd);
        }

        static bool HasPoint(IReadOnlyList<Vector3> pts, Vector3 p)
        {
            foreach (var q in pts) if (Vector3.Distance(p, q) < 1e-4f) return true;
            return false;
        }

        static float DistanceToPolyline(Vector3 p, List<Vector3> line)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                Vector3 a = line[i], ab = line[i + 1] - a;
                float t = ab.sqrMagnitude < 1e-8f ? 0f : Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector3.Distance(p, a + ab * t));
            }
            return best;
        }
    }
}
