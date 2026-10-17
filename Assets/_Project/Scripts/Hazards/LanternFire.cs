using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // A growing fire disc that kills on contact (design doc §7.9, IMPL §12.3). The gameplay radius drives
    // the trigger and the VFX scale; the particles never decide contact.
    public sealed class LanternFire : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] AssetRegistry assets;
        [SerializeField] SphereCollider trigger;
        [Tooltip("Calibration: footprint radius of the fire VFX at scale 1.")]
        [SerializeField] float vfxRadiusAtScale1 = 1f;

        FireRadius fire;
        Transform vfx;
        ShadowRegistry registry;

        public float Radius => fire.Radius;

        void Awake()
        {
            fire = new FireRadius(tuning.fireStartRadius, tuning.fireMaxRadius, tuning.fireGrowTime, tuning.fireLifeTime, tuning.fireShrinkTime);
            if (assets.lanternFireVfx != null) vfx = Instantiate(assets.lanternFireVfx, transform).transform;
            Apply();
        }

        void OnEnable()
        {
            registry = GameContext.Current != null ? GameContext.Current.shadows : null;
            if (registry != null) registry.Register(this);
        }

        void OnDisable()
        {
            if (registry != null) registry.Unregister(this);
        }

        void Update()
        {
            fire.Tick(Time.deltaTime);
            Apply();
            if (registry != null) BurnShadows();
            if (fire.Done) Destroy(gameObject);
        }

        // Oil drops and void shadows the fire reaches are destroyed (§7.9 rule 6).
        void BurnShadows()
        {
            var shadows = registry.Shadows;
            for (int i = shadows.Count - 1; i >= 0; i--)
                if (shadows[i].destroyedByFire && Contains(shadows[i].Center, 0f)) Destroy(shadows[i].gameObject);
        }

        public bool Contains(Vector3 point, float extraRadius)
        {
            Vector3 d = point - transform.position;
            d.y = 0f;
            float r = Radius + extraRadius;
            return d.sqrMagnitude <= r * r;
        }

        void Apply()
        {
            trigger.radius = Mathf.Max(0.01f, Radius);
            if (vfx != null) vfx.localScale = Vector3.one * (Radius / vfxRadiusAtScale1);
        }
    }
}
