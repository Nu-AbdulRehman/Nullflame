using NUnit.Framework;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class PipeCrackMeterTests
    {
        static PipeCrackMeter Meter() => new PipeCrackMeter(0.6f, 1.1f, 1.5f);

        [Test]
        public void Release_InGrey_DoesNothing()
        {
            var m = Meter();
            m.Hold(0.5f);
            Assert.AreEqual(PipeZone.Grey, m.Zone);
            Assert.AreEqual(PipeResult.None, m.Release());
            Assert.AreEqual(0f, m.Elapsed);
        }

        // T-PIPE-SAFE-START, T-PIPE-SAFE-END: 0.6 to 1.1 s is the safe zone (Beat 5).
        [Test]
        public void Release_InSafe_Cracks_TPIPESAFE()
        {
            var m = Meter();
            m.Hold(0.7f);
            Assert.AreEqual(PipeZone.Safe, m.Zone);
            Assert.AreEqual(PipeResult.Crack, m.Release());
        }

        [Test]
        public void Release_InWarning_Cracks()
        {
            var m = Meter();
            m.Hold(1.2f);
            Assert.AreEqual(PipeZone.Warning, m.Zone);
            Assert.AreEqual(PipeResult.Crack, m.Release());
        }

        // T-PIPE-BURST: holding to 1.5 s bursts (Beat 5).
        [Test]
        public void Hold_ToBurstTime_Bursts_TPIPEBURST()
        {
            var m = Meter();
            Assert.AreEqual(PipeResult.None, m.Hold(1.4f));
            Assert.AreEqual(PipeResult.Burst, m.Hold(0.2f));
        }
    }
}
