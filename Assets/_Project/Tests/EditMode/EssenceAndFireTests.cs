using NUnit.Framework;
using Nullflame.Logic;

namespace Nullflame.Tests
{
    public sealed class EssenceLogicTests
    {
        static EssenceLogic New(float start = 100f) => new EssenceLogic(100f, start, 10f);

        // T-COST-GATE, Beat 5: the gate consume is refused at Essence <= 80.
        [Test]
        public void TrySpend_RefusesWhenValueEqualsCost_TCOSTGATE()
        {
            var e = New(80f);
            Assert.IsFalse(e.TrySpend(80f));
            Assert.AreEqual(80f, e.Value);
        }

        [Test]
        public void TrySpend_SucceedsAbove()
        {
            var e = New(81f);
            Assert.IsTrue(e.TrySpend(80f));
            Assert.AreEqual(1f, e.Value, 1e-4f);
        }

        // T-ESS-FLOOR: held powers stop at 10.
        [Test]
        public void SpendHeld_StopsAtFloor_TESSFLOOR()
        {
            var e = New(12f);
            Assert.IsFalse(e.SpendHeld(5f));
            Assert.AreEqual(10f, e.Value, 1e-4f);
            Assert.IsFalse(e.SpendHeld(5f));
            Assert.AreEqual(10f, e.Value, 1e-4f);
        }

        [Test]
        public void SpendHeld_NeverKills()
        {
            var e = New(10.5f);
            e.SpendHeld(100f);
            Assert.IsFalse(e.IsDead);
        }

        [Test]
        public void Damage_CanKill()
        {
            var e = New(5f);
            e.Damage(8f);
            Assert.IsTrue(e.IsDead);
            Assert.AreEqual(0f, e.Value);
        }

        // T-GAIN-GUARD: +25 clamped to T-ESS-MAX.
        [Test]
        public void Gain_ClampsToMax_TGAINGUARD()
        {
            var e = New(90f);
            e.Gain(25f);
            Assert.AreEqual(100f, e.Value);
        }

        [Test]
        public void Kill_SetsZero()
        {
            var e = New();
            e.Kill();
            Assert.IsTrue(e.IsDead);
        }
    }

    public sealed class FireRadiusTests
    {
        static FireRadius New(float max = 4f) => new FireRadius(1f, max, 2f, 6f, 1f);

        [Test]
        public void Radius_AtImpact_TFIRESTART() => Assert.AreEqual(1f, New().Radius, 1e-4f);

        [Test]
        public void Radius_AfterGrow_TFIRERADIUS_TFIREGROW()
        {
            var f = New();
            f.Tick(2f);
            Assert.AreEqual(4f, f.Radius, 1e-4f);
            f.Tick(5.9f);
            Assert.AreEqual(4f, f.Radius, 1e-4f, "burns at max for T-FIRE-LIFE after growing (AD-011)");
        }

        [Test]
        public void Radius_OilFire_TFIRERADIUSOIL()
        {
            var f = New(5f);
            f.Tick(3f);
            Assert.AreEqual(5f, f.Radius, 1e-4f);
        }

        [Test]
        public void Radius_ShrinksToZero_TFIRESHRINK()
        {
            var f = New();
            f.Tick(8.5f);
            Assert.AreEqual(2f, f.Radius, 1e-4f);
            Assert.IsFalse(f.Done);
        }

        [Test]
        public void Done_AfterTotal_TFIRELIFE()
        {
            var f = New();
            f.Tick(9.01f);
            Assert.IsTrue(f.Done);
            Assert.AreEqual(0f, f.Radius);
        }
    }
}
