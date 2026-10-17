using System.Collections.Generic;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Alert -> throw -> walk to the lantern rack for a new lantern -> back on route (design doc §7.9, §8.2,
    // IMPL §12.1, AD-021). The guard stands still through the alert and the throw animation.
    public sealed class LanternAttack : MonoBehaviour
    {
        enum Phase { Ready, Alert, Throwing, Lanternless }

        [SerializeField] GameTuning tuning;
        [SerializeField] FacilityLayout layout;
        [SerializeField] GuardController guard;
        [SerializeField] GuardVision vision;
        [Tooltip("The held lantern: light + damage volume. Hidden while lanternless.")]
        [SerializeField] GameObject lantern;
        [SerializeField] GameObject alertIcon;
        [SerializeField] ThrownLantern thrownPrefab;
        [Tooltip("Look only: how long the guard stands through the throw animation (s).")]
        [SerializeField] float throwAnimTime = 1.2f;
        [Tooltip("Look only: pause at the rack while taking a lantern (s).")]
        [SerializeField] float rackPickupTime = 1f;

        const float RackStandOff = 0.6f; // in front of the rack's south face
        const float RelightDistance = 0.5f;
        static readonly List<Vector3> PathBuffer = new List<Vector3>(32);

        Phase phase;
        float timer;
        GuardAnimation anim;
        Vector3 rackPoint;
        bool walkingToRack;

        void Awake()
        {
            anim = GetComponent<GuardAnimation>();
            rackPoint = RackPoint();
        }

        void OnEnable() => vision.Spotted += OnSpotted;
        void OnDisable() => vision.Spotted -= OnSpotted;

        void OnSpotted(GuardVision _)
        {
            if (phase != Phase.Ready || !guard.HasLantern) return;
            phase = Phase.Alert;
            timer = tuning.alertTime;
            guard.SetPaused(true);
            if (alertIcon != null) alertIcon.SetActive(true);
            SfxPlayer.Play(SfxId.AlertSting);
            anim.Alert();
        }

        void Update()
        {
            if (phase == Phase.Ready) return;
            timer -= Time.deltaTime;
            switch (phase)
            {
                case Phase.Alert: TickAlert(); break;
                case Phase.Throwing: if (timer <= 0f) LeaveForRack(); break;
                case Phase.Lanternless:
                    if (timer <= 0f || (walkingToRack && (transform.position - rackPoint).sqrMagnitude < RelightDistance * RelightDistance)) Relight();
                    break;
            }
        }

        void TickAlert()
        {
            Vector3 aim = GameContext.Current.entity.transform.position;
            aim.y = 0f;
            Vector3 to = aim - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(to); // turn to the entity
            if (timer > 0f) return;

            // Aims where the entity's shadow is when the throw starts; the lantern doesn't home (§7.9 rule 2).
            Throw(aim, false);
        }

        // Pipe burst (§7.6): throw at once at the burst point, with no alert. Ignored while lanternless.
        public void ThrowAt(Vector3 point, bool oil)
        {
            if (phase != Phase.Ready || !guard.HasLantern) return;
            guard.SetPaused(true);
            Throw(point, oil);
        }

        void Throw(Vector3 aim, bool oil)
        {
            Instantiate(thrownPrefab, lantern.transform.position, Quaternion.identity).Launch(lantern.transform.position, aim, oil);
            lantern.SetActive(false);
            anim.Throw();
            guard.HasLantern = false;
            if (alertIcon != null) alertIcon.SetActive(false);
            phase = Phase.Throwing;
            timer = throwAnimTime;
        }

        // Walks to the rack by NavMesh path and back. With no path, relights in place after T-LANTERN-RELIGHT.
        void LeaveForRack()
        {
            guard.SetPaused(false);
            phase = Phase.Lanternless;
            walkingToRack = NavRoute.TryPath(transform.position, rackPoint, PathBuffer) && guard.Motion.Detour(PathBuffer, rackPickupTime);
            timer = walkingToRack ? float.PositiveInfinity : tuning.lanternRelightTime;
        }

        void Relight()
        {
            lantern.SetActive(true);
            guard.HasLantern = true;
            walkingToRack = false;
            phase = Phase.Ready;
        }

        Vector3 RackPoint()
        {
            foreach (PropSpec p in layout.props)
                if (p.kind == PropKind.LanternRack) return new Vector3(p.center.x, 0f, p.center.z - p.size.z * 0.5f - RackStandOff);
            return transform.position;
        }
    }
}
