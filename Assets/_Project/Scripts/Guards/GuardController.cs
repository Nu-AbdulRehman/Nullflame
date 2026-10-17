using System.Collections.Generic;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    public enum GuardMood { Meltdown, Normal }

    // Moves a guard along its routes with deterministic waypoint lerp (design doc §8.1, §8.2, IMPL §10.3).
    // Animation never drives position.
    [DefaultExecutionOrder(-50)]
    public sealed class GuardController : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] Light lanternLight;
        public WaypointRoute travelRoute;
        public WaypointRoute postRoute;
        public GuardMood mood = GuardMood.Meltdown;
        public Color lanternColor = new Color(1f, 0.55f, 0.2f);
        [Tooltip("Off for intro wave guards, which IntroSequence starts.")]
        public bool startOnEnable = true;
        [Tooltip("Spawned transit guards: destroy at the end of the travel route (show-only doors).")]
        public bool despawnAtEnd;

        public bool HasLantern { get; set; } = true;
        public bool IsBeingConsumed { get; set; }
        public GuardMotion Motion { get; private set; }

        static readonly List<RoutePoint> TravelBuffer = new List<RoutePoint>(32);

        void Awake()
        {
            if (lanternLight != null) lanternLight.color = lanternColor;
            if (travelRoute != null) Build(); // spawned guards are built by Init instead
        }

        // Spawners configure a fresh instance before its first frame (IMPL §17).
        public void Init(WaypointRoute travel, WaypointRoute post, GuardMood guardMood, bool despawn)
        {
            travelRoute = travel;
            postRoute = post;
            mood = guardMood;
            despawnAtEnd = despawn;
            Build();
        }

        void Build()
        {
            travelRoute.ToRoutePoints(TravelBuffer);
            var travel = new List<RoutePoint>(TravelBuffer);
            List<RoutePoint> post = null;
            if (postRoute != null)
            {
                postRoute.ToRoutePoints(TravelBuffer);
                post = new List<RoutePoint>(TravelBuffer);
            }
            float speed = mood == GuardMood.Meltdown ? tuning.guardWalkSpeedMeltdown : tuning.guardWalkSpeedNormal;
            Motion = new GuardMotion(travel, post, speed, tuning.reverseSpeedMultiplier, tuning.historyStep);
            Motion.ArrivedAtEnd += () => { if (despawnAtEnd) Destroy(gameObject); };
            Apply();
        }

        void OnEnable()
        {
            if (GameContext.Current != null) GameContext.Current.shadows.Register(this);
        }

        void OnDisable()
        {
            if (GameContext.Current != null) GameContext.Current.shadows.Unregister(this);
        }

        void Start()
        {
            if (startOnEnable) StartRoute();
        }

        public void StartRoute() => Motion.Start();
        public void BeginReverse() => Motion.BeginReverse();
        public void EndReverse() => Motion.EndReverse();
        public void SetPaused(bool paused) => Motion.Paused = paused;

        void Update() => Tick(Time.deltaTime);

        // Also called by IntroSequence to fast-forward guards deterministically.
        public void Tick(float dt)
        {
            if (Motion.Paused) return; // LanternAttack owns the facing while alerting
            Motion.Tick(dt);
            Apply();
        }

        void Apply()
        {
            transform.SetPositionAndRotation(Motion.Position, Quaternion.LookRotation(Motion.Facing, Vector3.up));
        }

        void OnValidate()
        {
            if (lanternLight != null) lanternLight.color = lanternColor;
        }
    }
}
