using NUnit.Framework;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class GateLogicTests
    {
        const float Delay = 1.5f; // T-GATE-CLOSE

        [Test]
        public void Opens_WhenGuardInZone()
        {
            var g = new GateLogic(Delay);
            g.Tick(1, 0.02f);
            Assert.IsTrue(g.IsOpen);
        }

        // Beat 4: the gate never opens for the player alone.
        [Test]
        public void StaysClosed_ForPlayerOnly_Beat4()
        {
            var g = new GateLogic(Delay);
            for (int i = 0; i < 100; i++) g.Tick(0, 0.1f);
            Assert.IsFalse(g.IsOpen);
        }

        [Test]
        public void Closes_AfterDelay_TGATECLOSE()
        {
            var g = new GateLogic(Delay);
            g.Tick(1, 0.02f);
            g.Tick(0, 1.4f);
            Assert.IsTrue(g.IsOpen, "still open inside the delay");
            g.Tick(0, 0.2f);
            Assert.IsFalse(g.IsOpen);
        }

        [Test]
        public void ReopenWithinDelay_CancelsClose()
        {
            var g = new GateLogic(Delay);
            g.Tick(1, 0.02f);
            g.Tick(0, 1f);
            g.Tick(1, 0.02f);
            g.Tick(0, 1f);
            Assert.IsTrue(g.IsOpen);
        }
    }
}
