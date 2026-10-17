using NUnit.Framework;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class SpawnTimerTests
    {
        // T-SPN-SLEEP-M: every 20 s, at most 2 alive.
        [Test]
        public void Interval_RespectsCap_TSPNSLEEPM()
        {
            var t = new SpawnTimer(20f, 2, false);
            int alive = 0;
            for (int i = 0; i < 1000; i++) if (t.Tick(0.1f, alive)) alive++;   // 100 s
            Assert.AreEqual(2, alive);
            var fresh = new SpawnTimer(20f, 2, false);
            Assert.IsFalse(fresh.Tick(19.9f, 0), "first spawn waits one interval");
            Assert.IsTrue(fresh.Tick(0.2f, 0));
        }

        // T-SPN-FINAL-N: fill to 2 at start (spaced), refill 10 s after a removal.
        [Test]
        public void Refill_AfterRemoval_TSPNFINALN()
        {
            var t = new SpawnTimer(10f, 2, true, 5.5f);
            Assert.IsTrue(t.Tick(0.02f, 0), "first spawn immediately");
            Assert.IsFalse(t.Tick(5f, 1), "second waits for the spacing");
            Assert.IsTrue(t.Tick(0.6f, 1));
            Assert.IsFalse(t.Tick(1f, 2), "at the cap");
            Assert.IsFalse(t.Tick(9.9f, 1), "one removed: wait the refill delay");
            Assert.IsTrue(t.Tick(0.2f, 1));
        }

        // T-SPN-FINAL-M: fill once, never refill.
        [Test]
        public void RefillWithZeroInterval_NeverRespawns_TSPNFINALM()
        {
            var t = new SpawnTimer(0f, 2, true);
            Assert.IsTrue(t.Tick(0.02f, 0));
            Assert.IsTrue(t.Tick(0.02f, 1));
            Assert.IsFalse(t.Tick(100f, 1));
        }
    }
}
