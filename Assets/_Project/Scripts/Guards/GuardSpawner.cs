using System.Collections.Generic;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Timed guard spawns with caps (design doc §8.4, IMPL §17). Guards spawn at the first waypoint of
    // the next travel route (routes alternate) as children, so a world-state swap destroys them too.
    [DefaultExecutionOrder(-40)]
    public sealed class GuardSpawner : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] SpawnerId id;
        [SerializeField] GuardController guardPrefab;
        [SerializeField] WaypointRoute[] travelRoutes;
        [SerializeField] WaypointRoute postRoute;
        [SerializeField] GuardMood mood = GuardMood.Meltdown;
        [SerializeField] bool despawnAtEnd;
        [Tooltip("Cap counts only guards not on Hold (SP-SIDE-N, EN-007).")]
        [SerializeField] bool capInTransitOnly;
        [Tooltip("Scene guards that count toward the cap (SP-SLEEP-N counts the two routine guards).")]
        [SerializeField] GuardController[] countsExisting;
        [Tooltip("Refill spawners: seconds between the initial spawns (SP-FINAL-N offsets its patrollers by half a loop).")]
        [SerializeField] float initialSpacing;

        readonly List<GuardController> alive = new List<GuardController>(4);
        SpawnTimer timer;
        int next;

        void Awake()
        {
            SpawnerTuning t = tuning.GetSpawner(id);
            timer = new SpawnTimer(t.interval, t.maxAlive, t.refillOnly, initialSpacing);
        }

        void Update()
        {
            if (timer.Tick(Time.deltaTime, CountAlive())) Spawn();
        }

        int CountAlive()
        {
            int n = 0;
            for (int i = alive.Count - 1; i >= 0; i--)
            {
                if (alive[i] == null) { alive.RemoveAt(i); continue; }
                if (!capInTransitOnly || alive[i].Motion.State != GuardMoveState.Hold) n++;
            }
            if (countsExisting != null)
                for (int i = 0; i < countsExisting.Length; i++)
                    if (countsExisting[i] != null) n++;
            return n;
        }

        void Spawn()
        {
            WaypointRoute route = travelRoutes[next++ % travelRoutes.Length];
            Vector3 at = route.transform.GetChild(0).position;
            GuardController g = Instantiate(guardPrefab, at, Quaternion.identity, transform);
            g.Init(route, postRoute, mood, despawnAtEnd);
            g.StartRoute();
            alive.Add(g);
        }
    }
}
