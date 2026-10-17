using System;
using UnityEngine;
using UnityEngine.AI;
using Nullflame.Logic;

namespace Nullflame
{
    public enum EntityState { Inactive, InShadow, Jumping, Consuming, Dead }

    // The player: in a shadow, on bare floor where the layout allows it (AD-023), jumping, or dead
    // (design doc §5, IMPL §7.3). Riding is just being InShadow in a moving shadow.
    [DefaultExecutionOrder(0)]
    public sealed class ShadowEntity : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] Transform cameraTransform;
        [SerializeField] ShadowArea startShadow;
        [SerializeField] ShadowArea voidPrefab;
        [SerializeField] Essence essence;
        [SerializeField] FacilityLayout layout;

        public EntityState State { get; private set; } = EntityState.Inactive;
        public ShadowArea Current { get; private set; }
        public bool IsRiding => Current != null && Current.isMoving;
        public bool OnFloor => State == EntityState.InShadow && Current == null; // bare floor, no cover (AD-023)
        public bool HasControl { get; private set; } = true;
        public event Action<EntityState> StateChanged;

        ShadowArea jumpTarget;
        ShadowArea spawnedVoid;
        Vector3 jumpFrom, jumpLanding;
        float jumpT;
        bool currentLost;

        float Margin => tuning.entitySize * 0.5f;
        const float FloorSnap = 0.3f; // how far off the guard NavMesh a floor step may land

        void Start()
        {
            if (startShadow != null && State == EntityState.Inactive) Place(startShadow, transform.position);
        }

        void OnEnable() => essence.Died += Die;

        void OnDisable()
        {
            essence.Died -= Die;
            if (Current != null) Current.Removed -= OnCurrentRemoved;
            if (jumpTarget != null) jumpTarget.Removed -= OnTargetRemoved;
        }

        public void SetControl(bool enabled) => HasControl = enabled;

        public void Place(ShadowArea shadow, Vector3 position)
        {
            Attach(shadow);
            transform.position = shadow.isMoving ? shadow.Center : shadow.ClampInside(position, Margin);
            SetState(EntityState.InShadow);
        }

        public void BeginJump(ShadowArea target, Vector3 landing)
        {
            Attach(null);
            jumpTarget = target;
            jumpTarget.Removed += OnTargetRemoved;
            jumpFrom = transform.position;
            jumpLanding = landing;
            jumpT = 0f;
            SetState(EntityState.Jumping);
            SfxPlayer.Play(SfxId.JumpWhoosh);
        }

        public void BeginConsume()
        {
            if (State == EntityState.InShadow) SetState(EntityState.Consuming);
        }

        public void EndConsume()
        {
            if (State == EntityState.Consuming) SetState(EntityState.InShadow);
        }

        public void Die()
        {
            if (State == EntityState.Dead) return;
            HasControl = false;
            SetState(EntityState.Dead);
        }

        void Update()
        {
            switch (State)
            {
                case EntityState.InShadow: TickInShadow(Time.deltaTime); break;
                case EntityState.Jumping: TickJump(Time.deltaTime); break;
            }
        }

        void TickInShadow(float dt)
        {
            if (currentLost)
            {
                // Our shadow vanished (consumed owner, despawn, fire). Stay put in a void (design doc §7.5).
                currentLost = false;
                SpawnVoid(transform.position);
            }
            if (Current != null && Current.isMoving)
            {
                transform.position = Current.Center;
                return;
            }
            if (!HasControl || input == null || input.Move == Vector2.zero) return;

            Vector3 fwd = cameraTransform.forward;
            fwd.y = 0f;
            fwd.Normalize();
            Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);
            Move((right * input.Move.x + fwd * input.Move.y) * (tuning.slideSpeed * dt));
        }

        // A step inside the current shadow is clamped to it as before. A step that leaves it, or any step
        // on the floor, is allowed where the layout permits floor and the guard NavMesh is walkable (walls
        // and props stop it); it attaches to whatever shadow it lands in (AD-023).
        public void Move(Vector3 delta)
        {
            Vector3 target = transform.position + delta;
            Vector3 clamped = Current != null ? Current.ClampInside(target, Margin) : target;
            if (Current != null && (clamped - target).sqrMagnitude < 1e-6f)
            {
                transform.position = clamped;
                return;
            }
            if (!FloorStep(transform.position, ref target))
            {
                if (Current != null) transform.position = clamped;
                return;
            }
            transform.position = target;
            ShadowArea s = GameContext.Current != null ? GameContext.Current.shadows.FindAt(target, null) : null;
            if (s == Current) return;
            Attach(s);
            if (s != null && s.isMoving) transform.position = s.Center;
        }

        bool FloorStep(Vector3 from, ref Vector3 target)
        {
            if (layout != null && !layout.FloorAllowed(target)) return false;
            if (!NavMesh.SamplePosition(from, out NavMeshHit start, FloorSnap, NavMesh.AllAreas)) return false;
            if (NavMesh.Raycast(start.position, target, out NavMeshHit hit, NavMesh.AllAreas)) return false; // wall or prop
            if (!NavMesh.SamplePosition(target, out NavMeshHit end, FloorSnap, NavMesh.AllAreas)) return false;
            target = new Vector3(end.position.x, target.y, end.position.z);
            return true;
        }

        void TickJump(float dt)
        {
            jumpT += dt / tuning.jumpTime;
            if (jumpTarget != null && jumpTarget.isMoving) jumpLanding = jumpTarget.Center; // home on moving shadows
            transform.position = JumpArc.Evaluate(jumpFrom, jumpLanding, tuning.jumpArcHeight, jumpT);
            if (jumpT < 1f) return;

            ShadowArea landedOn = jumpTarget;
            if (landedOn != null) landedOn.Removed -= OnTargetRemoved;
            jumpTarget = null;
            if (landedOn != null) Place(landedOn, jumpLanding);
            else
            {
                transform.position = jumpLanding;
                SetState(EntityState.InShadow);
                SpawnVoid(jumpLanding);
            }
        }

        void SpawnVoid(Vector3 position)
        {
            position.y = 0f;
            ShadowArea v = Instantiate(voidPrefab, position, Quaternion.identity);
            v.SetSize(new Vector2(tuning.voidShadowSize, tuning.voidShadowSize));
            Attach(v);
            spawnedVoid = v;
            transform.position = position;
        }

        void Attach(ShadowArea shadow)
        {
            if (Current != null) Current.Removed -= OnCurrentRemoved;
            Current = shadow;
            if (Current != null) Current.Removed += OnCurrentRemoved;
            // A void shadow despawns when the entity leaves it (design doc §7.5).
            if (spawnedVoid != null && spawnedVoid != shadow)
            {
                Destroy(spawnedVoid.gameObject);
                spawnedVoid = null;
            }
        }

        // Shadows fire Removed from OnDisable, which also runs during scene unload, so these only set
        // flags; the void is spawned on the next Update.
        void OnCurrentRemoved(ShadowArea s)
        {
            s.Removed -= OnCurrentRemoved;
            if (s != Current) return;
            Current = null;
            if (s == spawnedVoid) spawnedVoid = null;
            currentLost = true;
        }

        void OnTargetRemoved(ShadowArea s)
        {
            s.Removed -= OnTargetRemoved;
            if (s == jumpTarget) jumpTarget = null;
        }

        void SetState(EntityState s)
        {
            if (State == s) return;
            State = s;
            StateChanged?.Invoke(s);
        }
    }
}
