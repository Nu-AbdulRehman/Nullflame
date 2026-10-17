using NUnit.Framework;
using UnityEngine;

namespace Nullflame.Tests
{
    // Asserts every design doc §13 default, one per T-* row.
    public sealed class GameTuningTests
    {
        [Test]
        public void Defaults_MatchDesignTable()
        {
            var t = ScriptableObject.CreateInstance<GameTuning>();
            try
            {
                Assert.AreEqual(4f, t.ceilingHeight, "T-LVL-CEIL");
                Assert.AreEqual(1.2f, t.entitySize, "T-PLY-SIZE");
                Assert.AreEqual(6f, t.jumpRange, "T-JMP-RANGE");
                Assert.AreEqual(1.5f, t.jumpArcHeight, "T-JMP-ARC");
                Assert.AreEqual(0.45f, t.jumpTime, "T-JMP-TIME");
                Assert.AreEqual(100f, t.essenceMax, "T-ESS-MAX");
                Assert.AreEqual(100f, t.essenceStart, "T-ESS-START");
                Assert.AreEqual(10f, t.introGrazeDamage, "T-ESS-INTRO");
                Assert.AreEqual(10f, t.essenceFloor, "T-ESS-FLOOR");
                Assert.AreEqual(8f, t.lanternDps, "T-DMG-LANTERN");
                Assert.AreEqual(5f, t.alarmDps, "T-DMG-ALARM");
                Assert.AreEqual(3f, t.lanternRadius, "T-LANTERN-RADIUS");
                Assert.AreEqual(0.6f, t.throwTime, "T-THROW-TIME");
                Assert.AreEqual(1f, t.fireStartRadius, "T-FIRE-START");
                Assert.AreEqual(4f, t.fireMaxRadius, "T-FIRE-RADIUS");
                Assert.AreEqual(5f, t.fireOilRadius, "T-FIRE-RADIUS-OIL");
                Assert.AreEqual(2f, t.fireGrowTime, "T-FIRE-GROW");
                Assert.AreEqual(6f, t.fireLifeTime, "T-FIRE-LIFE");
                Assert.AreEqual(10f, t.lanternRelightTime, "T-LANTERN-RELIGHT");
                Assert.AreEqual(5f, t.pipeCost, "T-COST-PIPE");
                Assert.AreEqual(20f, t.pedestalCost, "T-COST-PEDESTAL");
                Assert.AreEqual(80f, t.gateCost, "T-COST-GATE");
                Assert.AreEqual(5f, t.reverseCostPerSecond, "T-COST-REVERSE");
                Assert.AreEqual(30f, t.rewindCost, "T-COST-REWIND");
                Assert.AreEqual(25f, t.guardGain, "T-GAIN-GUARD");
                Assert.AreEqual(2f, t.guardConsumeTime, "T-CON-GUARD");
                Assert.AreEqual(3f, t.pedestalConsumeTime, "T-CON-PEDESTAL");
                Assert.AreEqual(5f, t.gateConsumeTime, "T-CON-GATE");
                Assert.AreEqual(2f, t.rewindChannelTime, "T-REWIND-CHANNEL");
                Assert.AreEqual(2f, t.reverseSpeedMultiplier, "T-REV-SPEED");
                Assert.AreEqual(1.5f, t.pipeBurstTime, "T-PIPE-BURST");
                Assert.AreEqual(2f, t.guardWalkSpeedNormal, "T-GRD-WALK");
                Assert.AreEqual(2.4f, t.guardWalkSpeedMeltdown, "T-GRD-WALK-MELT");
                Assert.AreEqual(0.6f, t.alertTime, "T-GRD-ALERT");
                Assert.AreEqual(70f, t.viewAngle, "T-VIEW-ANGLE");
                Assert.AreEqual(8f, t.viewRange, "T-VIEW-RANGE");
                Assert.AreEqual(0.2f, t.viewGrace, "T-VIEW-GRACE");
            }
            finally
            {
                Object.DestroyImmediate(t);
            }
        }

        [Test]
        public void GetSpawner_ReturnsDesignValues_TSPN()
        {
            var t = ScriptableObject.CreateInstance<GameTuning>();
            try
            {
                Assert.AreEqual(20f, t.GetSpawner(SpawnerId.SleepMeltdown).interval, "T-SPN-SLEEP-M");
                Assert.AreEqual(3, t.GetSpawner(SpawnerId.SleepNormal).maxAlive, "T-SPN-SLEEP-N");
                Assert.AreEqual(15f, t.GetSpawner(SpawnerId.SideNormal).interval, "T-SPN-SIDE-N");
                Assert.IsTrue(t.GetSpawner(SpawnerId.FinalNormal).refillOnly, "T-SPN-FINAL-N");
            }
            finally
            {
                Object.DestroyImmediate(t);
            }
        }
    }
}
