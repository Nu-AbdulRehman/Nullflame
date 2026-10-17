using NUnit.Framework;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class SpotTimerTests
    {
        const float Grace = 0.2f; // T-VIEW-GRACE

        [Test]
        public void Spot_RequiresContinuousGrace_TVIEWGRACE()
        {
            var t = new SpotTimer(Grace);
            Assert.IsFalse(t.Tick(true, 0.1f));
            Assert.IsFalse(t.Tick(true, 0.09f));
            Assert.IsTrue(t.Tick(true, 0.02f));
        }

        [Test]
        public void Spot_ResetsWhenConditionBreaks()
        {
            var t = new SpotTimer(Grace);
            t.Tick(true, 0.15f);
            t.Tick(false, 0.01f);
            Assert.IsFalse(t.Tick(true, 0.15f), "grace restarts after a break");
        }

        [Test]
        public void Spot_FiresOnce_UntilReset()
        {
            var t = new SpotTimer(Grace);
            Assert.IsTrue(t.Tick(true, 0.3f));
            Assert.IsFalse(t.Tick(true, 0.3f));
            t.Reset();
            Assert.IsTrue(t.Tick(true, 0.3f));
        }
    }

    public sealed class ViewConeMathTests
    {
        const float Angle = 70f, Range = 8f; // T-VIEW-ANGLE, T-VIEW-RANGE

        [Test]
        public void Contains_EdgeOfAngle_TVIEWANGLE()
        {
            Vector3 inside = Quaternion.Euler(0f, 34f, 0f) * Vector3.forward * 5f;
            Vector3 outside = Quaternion.Euler(0f, 36f, 0f) * Vector3.forward * 5f;
            Assert.IsTrue(ViewConeMath.Contains(Vector3.up * 1.6f, Vector3.forward, Angle, Range, inside));
            Assert.IsFalse(ViewConeMath.Contains(Vector3.up * 1.6f, Vector3.forward, Angle, Range, outside));
        }

        [Test]
        public void Contains_BeyondRange_TVIEWRANGE()
        {
            Assert.IsTrue(ViewConeMath.Contains(Vector3.zero, Vector3.forward, Angle, Range, Vector3.forward * 7.9f));
            Assert.IsFalse(ViewConeMath.Contains(Vector3.zero, Vector3.forward, Angle, Range, Vector3.forward * 8.1f));
        }

        [Test]
        public void Contains_Behind_IsFalse()
        {
            Assert.IsFalse(ViewConeMath.Contains(Vector3.zero, Vector3.forward, Angle, Range, Vector3.back * 2f));
        }
    }
}
