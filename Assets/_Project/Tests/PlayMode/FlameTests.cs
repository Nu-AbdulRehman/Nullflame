using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    // Shared setup: Facility loaded, every Meltdown guard but G-MAIN switched off so lanterns don't stack.
    public abstract class HazardFixture
    {
        protected ShadowEntity entity;
        protected Essence essence;
        protected GuardController guard;
        protected GameTuning tuning;

        protected IEnumerator Setup()
        {
            yield return TestScenes.LoadFacility();
            tuning = AssetLoad.Tuning();
            entity = Object.FindAnyObjectByType<ShadowEntity>();
            essence = Object.FindAnyObjectByType<Essence>();
            guard = GameObject.Find("G-MAIN").GetComponent<GuardController>();
            foreach (Transform g in guard.transform.parent)
                if (g != guard.transform) g.gameObject.SetActive(false);
        }

        protected static ShadowArea MakeShadow(Vector3 at, ShadowOpacity opacity = ShadowOpacity.Opaque)
        {
            ShadowArea s = Object.Instantiate(GameObject.Find("CartShadow").GetComponent<ShadowArea>(), at, Quaternion.identity);
            s.opacity = opacity;
            s.SetSize(new Vector2(1.2f, 1.2f));
            return s;
        }

        protected void PlaceAt(Vector3 at, ShadowOpacity opacity = ShadowOpacity.Opaque)
        {
            ShadowArea s = MakeShadow(at, opacity);
            entity.Place(s, s.Center);
        }

        protected static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }

        protected static LanternFire SpawnFire(Vector3 at)
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<LanternFire>("Assets/_Project/Prefabs/P_LanternFire.prefab");
            return Object.Instantiate(prefab, at, Quaternion.identity);
#else
            return null;
#endif
        }
    }

    public sealed class FlameTests : HazardFixture
    {
        // T-DMG-LANTERN: 8 per second while another guard's lantern touches the entity.
        [UnityTest]
        public IEnumerator LanternDrains_TDMGLANTERN()
        {
            yield return Setup();
            PlaceAt(new Vector3(25f, 0f, 39f));
            yield return null;
            float before = essence.Value;
            yield return Wait(1f);
            Assert.AreEqual(tuning.lanternDps, before - essence.Value, 1f);
        }

        // §7.2 / O1: a guard's own lantern never hurts the shadow it casts.
        [UnityTest]
        public IEnumerator OwnLantern_DoesNotHurtRider()
        {
            yield return Setup();
            ShadowArea shadow = guard.GetComponentInChildren<ShadowArea>();
            entity.Place(shadow, shadow.Center);
            yield return Wait(1f);
            Assert.AreEqual(tuning.essenceStart, essence.Value, 1e-3f);
        }

        // T-DMG-ALARM: 5 per second under an alarm light (Meltdown).
        [UnityTest]
        public IEnumerator Alarm_TDMGALARM()
        {
            yield return Setup();
            PlaceAt(new Vector3(8f, 0f, 8f));
            yield return null;
            float before = essence.Value;
            yield return Wait(1f);
            Assert.AreEqual(tuning.alarmDps, before - essence.Value, 1f);
        }

        // §7.7: wall fire lanterns kill instantly, even in shadow.
        [UnityTest]
        public IEnumerator WallFire_Kills()
        {
            yield return Setup();
            PlaceAt(new Vector3(18.75f, 0f, 48f));
            yield return Wait(0.2f);
            Assert.AreEqual(0f, essence.Value);
            Assert.AreEqual(EntityState.Dead, entity.State);
        }

        // §7.7: turning a flame light off stops its damage.
        [UnityTest]
        public IEnumerator LightOff_StopsDamage()
        {
            yield return Setup();
            PlaceAt(new Vector3(8f, 0f, 8f));
            yield return Wait(0.2f);
            foreach (var f in Object.FindObjectsByType<FlameVolume>(FindObjectsSortMode.None))
                if (f.kind == FlameKind.Alarm) f.gameObject.SetActive(false);
            yield return null;
            float before = essence.Value;
            yield return Wait(0.5f);
            Assert.AreEqual(before, essence.Value, 1e-3f);
        }
    }

    public sealed class AttackTests : HazardFixture
    {
        // T-GRD-ALERT + T-THROW-TIME: spotted -> 0.6 s alert -> 0.6 s flight -> fire.
        [UnityTest]
        public IEnumerator Spotted_ThrowsAfterAlert_TGRDALERT_TTHROWTIME()
        {
            yield return Setup();
            PlaceAt(new Vector3(25f, 0f, 39f), ShadowOpacity.Soft);
            yield return Wait(tuning.viewGrace + tuning.alertTime * 0.5f);
            Assert.IsNull(Object.FindAnyObjectByType<LanternFire>(), "no fire during the alert");
            yield return Wait(tuning.alertTime + tuning.throwTime);
            Assert.IsNotNull(Object.FindAnyObjectByType<LanternFire>(), "fire after alert + flight");
            Assert.IsFalse(guard.HasLantern, "lanternless after throwing");
        }

        // §7.9 rule 4: fire contact sets Essence to 0.
        [UnityTest]
        public IEnumerator FireContact_Kills()
        {
            yield return Setup();
            PlaceAt(new Vector3(10f, 0f, 26f));
            SpawnFire(new Vector3(10f, 0f, 26f));
            yield return Wait(0.2f);
            Assert.AreEqual(EntityState.Dead, entity.State);
        }

        // T-FIRE-RADIUS: beyond max radius + entity radius the entity survives.
        [UnityTest]
        public IEnumerator EscapeBeyondRadius_Survives_TFIRERADIUS()
        {
            yield return Setup();
            float safe = tuning.fireMaxRadius + tuning.entitySize * 0.5f + 0.3f;
            PlaceAt(new Vector3(9f + safe, 0f, 26f));
            SpawnFire(new Vector3(9f, 0f, 26f));
            yield return Wait(tuning.fireGrowTime + 0.5f);
            Assert.AreEqual(EntityState.InShadow, entity.State);
            Assert.Greater(essence.Value, 0f);
        }

        // AD-011: flames never touch an airborne entity.
        [UnityTest]
        public IEnumerator FireDoesNotKillAirborne()
        {
            yield return Setup();
            LanternFire fire = SpawnFire(new Vector3(12f, 0f, 26f));
            yield return Wait(tuning.fireGrowTime);
            PlaceAt(new Vector3(12f - 5f, 0f, 24f));
            ShadowArea far = MakeShadow(new Vector3(12f + 5f, 0f, 24f));
            float before = essence.Value;
            entity.BeginJump(far, far.Center);
            yield return Wait(tuning.jumpTime * 0.5f);
            Assert.IsTrue(fire.Contains(entity.transform.position, 0f), "mid-jump inside the fire's radius");
            Assert.AreEqual(before, essence.Value, 1e-3f);
            Assert.AreNotEqual(EntityState.Dead, entity.State);
        }

        // §7.9 rule 6: fire destroys void shadows and oil drops it reaches.
        [UnityTest]
        public IEnumerator FireDestroysOilAndVoid()
        {
            yield return Setup();
            PlaceAt(new Vector3(12f, 0f, 26f));
            ShadowArea victim = MakeShadow(new Vector3(9f, 0f, 24f));
            victim.destroyedByFire = true;
            SpawnFire(new Vector3(9.5f, 0f, 24f));
            yield return Wait(0.2f);
            Assert.IsTrue(victim == null, "destroyed by the fire");
        }

        // T-LANTERN-RELIGHT: the guard relights after 10 s (time scaled up for the test).
        [UnityTest]
        public IEnumerator Relight_After_TLANTERNRELIGHT()
        {
            yield return Setup();
            PlaceAt(new Vector3(25f, 0f, 39f), ShadowOpacity.Soft);
            yield return Wait(tuning.viewGrace + tuning.alertTime + 0.1f);
            Assert.IsFalse(guard.HasLantern);
            ShadowArea away = MakeShadow(new Vector3(10f, 0f, 26f));
            entity.Place(away, away.Center);
            Time.timeScale = 10f;
            yield return Wait(tuning.lanternRelightTime + 0.5f);
            Time.timeScale = 1f;
            Assert.IsTrue(guard.HasLantern);
        }
    }

    public sealed class EssenceTests : HazardFixture
    {
        [UnityTest]
        public IEnumerator Died_FiresOnZero()
        {
            yield return Setup();
            bool died = false;
            essence.Died += () => died = true;
            essence.Damage(1000f);
            yield return null;
            Assert.IsTrue(died);
            Assert.AreEqual(EntityState.Dead, entity.State);
            Assert.IsTrue(GameObject.Find("Canvas").transform.Find("DeathScreen").gameObject.activeSelf, "death screen");
        }
    }
}
