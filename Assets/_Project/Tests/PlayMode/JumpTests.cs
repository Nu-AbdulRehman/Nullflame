using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nullflame.Tests
{
    public sealed class JumpTests
    {
        ShadowEntity entity;
        JumpTargeting targeting;

        static ShadowArea Shadow(string name) => GameObject.Find(name).GetComponent<ShadowArea>();

        IEnumerator Setup()
        {
            yield return TestScenes.LoadFacility();
            entity = Object.FindAnyObjectByType<ShadowEntity>();
            targeting = entity.GetComponent<JumpTargeting>();
        }

        // T-JMP-RANGE: a shadow farther than 6 m is shown but can't be jumped to (red outline).
        [UnityTest]
        public IEnumerator Jump_OutOfRange_IsRed_TJMPRANGE()
        {
            yield return Setup();
            ShadowArea pedestal = Shadow("PedestalShadow");
            entity.Place(pedestal, pedestal.Center);

            Assert.IsTrue(targeting.TryGetTarget(Shadow("CabinetShadow").Center, out ShadowArea near, out _), "cabinet in range");
            Assert.AreEqual("CabinetShadow", near.name);
            Assert.IsFalse(targeting.TryGetTarget(Shadow("LanternRackShadow").Center, out ShadowArea far, out _), "rack > 6 m");
            Assert.AreEqual("LanternRackShadow", far.name, "out-of-range target still returned for the red outline");
        }

        // §5.3: no wall between source and target (raycast at T-JMP-RAY-Y).
        [UnityTest]
        public IEnumerator Jump_ThroughWall_IsBlocked()
        {
            yield return Setup();
            ShadowArea cart = Shadow("CartShadow");
            entity.Place(cart, cart.Center);
            Assert.IsFalse(targeting.TryGetTarget(Shadow("CabinetShadow").Center, out ShadowArea t, out _));
            Assert.IsNotNull(t);
        }

        // §5.3: the arc homes on a moving target and lands in it (Beat 1 basis).
        [UnityTest]
        public IEnumerator Jump_HomesOnMovingTarget()
        {
            yield return Setup();
            ShadowArea pedestal = Shadow("PedestalShadow");
            entity.Place(pedestal, pedestal.Center);
            ShadowArea moving = Object.Instantiate(pedestal, new Vector3(5.5f, 0f, 3f), Quaternion.identity);
            moving.isMoving = true;
            moving.SetSize(new Vector2(1.5f, 1f));

            entity.BeginJump(moving, moving.Center);
            float limit = AssetLoad.Tuning().jumpTime * 3f, t = 0f;
            while (entity.State == EntityState.Jumping && t < limit)
            {
                moving.transform.position += Vector3.forward * (2f * Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }
            yield return null;

            Assert.AreEqual(EntityState.InShadow, entity.State);
            Assert.AreSame(moving, entity.Current);
            Assert.AreEqual(moving.Center.x, entity.transform.position.x, 1e-3f);
            Assert.AreEqual(moving.Center.z, entity.transform.position.z, 1e-3f);
        }
    }

    public sealed class EntityTests
    {
        static ShadowArea Shadow(string name) => GameObject.Find(name).GetComponent<ShadowArea>();

        // T-PLY-SIZE: sliding is clamped to the shadow shrunk by the entity radius.
        [UnityTest]
        public IEnumerator Slide_ClampedToBounds_TPLYSIZE()
        {
            yield return TestScenes.LoadFacility();
            var entity = Object.FindAnyObjectByType<ShadowEntity>();
            ShadowArea room = Shadow("PlayerRoomFloor");
            entity.Place(room, room.Center + new Vector3(50f, 0f, 0f));
            float margin = AssetLoad.Tuning().entitySize * 0.5f;
            Assert.AreEqual(room.Center.x + room.size.x * 0.5f - margin, entity.transform.position.x, 1e-3f);
        }

        // §7.5: when the current shadow disappears the entity stays put in a void shadow.
        [UnityTest]
        public IEnumerator ShadowRemoved_FloorWhereAllowed_VoidElsewhere_TSHDVOID()
        {
            yield return TestScenes.LoadFacility();
            var entity = Object.FindAnyObjectByType<ShadowEntity>();
            ShadowArea temp = Object.Instantiate(Shadow("PedestalShadow"), new Vector3(5f, 0f, 2f), Quaternion.identity);
            entity.Place(temp, temp.Center);
            Vector3 floorSpot = entity.transform.position;
            Object.Destroy(temp.gameObject);
            yield return null;
            yield return null;
            Assert.IsTrue(entity.OnFloor, "Time Room floor is allowed (AD-023): no void");
            Assert.AreEqual(floorSpot, entity.transform.position);

            yield return Flow.ToNormal();
            temp = Object.Instantiate(Shadow("PedestalShadow"), new Vector3(18.6f, 0f, 46.5f), Quaternion.identity);
            entity.Place(temp, temp.Center);
            Vector3 before = entity.transform.position;

            Object.Destroy(temp.gameObject);
            yield return null;
            yield return null;

            Assert.IsNotNull(entity.Current, "Final Corridor allows no floor: a void");
            Assert.IsTrue(entity.Current.destroyedByFire, "void shadow");
            Assert.AreEqual(AssetLoad.Tuning().voidShadowSize, entity.Current.size.x, 1e-4f, "T-SHD-VOID");
            Assert.AreEqual(before.x, entity.transform.position.x, 1e-3f);
            Assert.AreEqual(before.z, entity.transform.position.z, 1e-3f);
        }
    }

    public static class AssetLoad
    {
        public static GameTuning Tuning()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameTuning>("Assets/_Project/Settings/GameTuning.asset");
#else
            return null;
#endif
        }

        public static T Prefab<T>(string name) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/_Project/Prefabs/{name}.prefab");
#else
            return null;
#endif
        }
    }
}
