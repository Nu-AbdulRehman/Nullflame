using UnityEngine;

namespace Nullflame
{
    // Picks the shadow under the reticle, checks range and line of sight, draws the outline,
    // and starts the jump on left click (design doc §5.3, §12, IMPL §7.5).
    [DefaultExecutionOrder(5)]
    public sealed class JumpTargeting : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] ShadowEntity entity;
        [SerializeField] Camera cam;
        [SerializeField] ShadowRegistry registry;
        [SerializeField] LineRenderer outline;

        const float OutlineHeight = 0.018f; // IMPL §2.8 draw order
        static readonly Color Valid = Color.white, Invalid = Color.red;

        int solidMask;

        public bool HasValidTarget { get; private set; }

        void Awake() => solidMask = LayerMask.GetMask("LevelSolid");

        void Update()
        {
            HasValidTarget = false;
            if (entity.State != EntityState.InShadow || !entity.HasControl || !AimPoint(out Vector3 aim))
            {
                outline.enabled = false;
                return;
            }
            bool valid = TryGetTarget(aim, out ShadowArea target, out Vector3 landing);
            HasValidTarget = valid;
            if (target != null) DrawOutline(target, valid ? Valid : Invalid);
            else outline.enabled = false; // a floor jump has no outline
            if (valid && input.JumpPressed) entity.BeginJump(target, landing);
        }

        // target is the shadow under (or snapped to) floorPoint, or null for bare floor (AD-023). Returns
        // whether the jump is allowed: in range, line of sight, not into fire, and for floor, floor the entity may stand on.
        public bool TryGetTarget(Vector3 floorPoint, out ShadowArea target, out Vector3 landing)
        {
            target = registry.FindAt(floorPoint, entity.Current);
            if (target == null) target = registry.FindNearest(floorPoint, tuning.reticleSnapDistance, entity.Current);
            landing = floorPoint;
            if (target == null && !entity.CanLandOnFloor(floorPoint)) return false;

            if (target != null) landing = target.isMoving ? target.Center : target.ClampInside(floorPoint, tuning.entitySize * 0.5f);
            if (registry.IsInsideAnyFire(landing, tuning.entitySize * 0.5f)) return false;
            Vector3 from = entity.transform.position;
            Vector3 closest = target != null ? target.ClosestPoint(from) : landing;
            Vector3 d = closest - from;
            d.y = 0f;
            if (d.sqrMagnitude > tuning.jumpRange * tuning.jumpRange) return false;

            Vector3 up = Vector3.up * tuning.jumpRayHeight;
            from.y = 0f;
            closest.y = 0f;
            return !Physics.Linecast(from + up, closest + up, solidMask, QueryTriggerInteraction.Ignore);
        }

        bool AimPoint(out Vector3 point)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            point = default;
            if (ray.direction.y > -1e-4f) return false; // looking at or above the horizon
            point = ray.origin + ray.direction * (-ray.origin.y / ray.direction.y);
            return true;
        }

        void DrawOutline(ShadowArea s, Color c)
        {
            outline.enabled = true;
            outline.startColor = outline.endColor = c;
            Quaternion rot = Quaternion.Euler(0f, s.Yaw, 0f);
            float hx = s.size.x * 0.5f, hz = s.size.y * 0.5f;
            Vector3 center = s.Center;
            center.y = OutlineHeight;
            outline.SetPosition(0, center + rot * new Vector3(-hx, 0f, -hz));
            outline.SetPosition(1, center + rot * new Vector3(-hx, 0f, hz));
            outline.SetPosition(2, center + rot * new Vector3(hx, 0f, hz));
            outline.SetPosition(3, center + rot * new Vector3(hx, 0f, -hz));
        }
    }
}
