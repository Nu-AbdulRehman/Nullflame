using UnityEngine;

namespace Nullflame
{
    // Alert -> throw -> lanternless -> relight (design doc §7.9, §8.2, IMPL §12.1).
    public sealed class LanternAttack : MonoBehaviour
    {
        enum Phase { Ready, Alert, Lanternless }

        [SerializeField] GameTuning tuning;
        [SerializeField] GuardController guard;
        [SerializeField] GuardVision vision;
        [Tooltip("The held lantern: light + damage volume. Hidden while lanternless.")]
        [SerializeField] GameObject lantern;
        [SerializeField] GameObject alertIcon;
        [SerializeField] ThrownLantern thrownPrefab;

        Phase phase;
        float timer;
        GuardAnimation anim;

        void Awake() => anim = GetComponent<GuardAnimation>();

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
            if (phase == Phase.Alert) TickAlert();
            else if (timer <= 0f) Relight();
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
            if (phase == Phase.Lanternless || !guard.HasLantern) return;
            Throw(point, oil);
        }

        void Throw(Vector3 aim, bool oil)
        {
            Instantiate(thrownPrefab, lantern.transform.position, Quaternion.identity).Launch(lantern.transform.position, aim, oil);
            lantern.SetActive(false);
            anim.Throw();
            guard.HasLantern = false;
            guard.SetPaused(false);
            if (alertIcon != null) alertIcon.SetActive(false);
            phase = Phase.Lanternless;
            timer = tuning.lanternRelightTime;
        }

        void Relight()
        {
            lantern.SetActive(true);
            guard.HasLantern = true;
            phase = Phase.Ready;
        }
    }
}
