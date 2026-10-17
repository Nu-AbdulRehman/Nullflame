using NUnit.Framework;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class HoldProgressTests
    {
        // T-CON-GUARD: a guard consume completes after 2 s of holding.
        [Test]
        public void Completes_AtDuration_TCONGUARD()
        {
            var h = new HoldProgress(2f);
            Assert.IsFalse(h.Tick(1.9f));
            Assert.IsTrue(h.Tick(0.2f));
            Assert.AreEqual(1f, h.Normalized, 1e-4f);
            Assert.IsFalse(h.Tick(1f), "fires once");
        }

        [Test]
        public void Cancel_Resets()
        {
            var h = new HoldProgress(2f);
            h.Tick(1.5f);
            h.Cancel();
            Assert.AreEqual(0f, h.Normalized);
            Assert.IsFalse(h.Tick(1.9f));
        }
    }
}
