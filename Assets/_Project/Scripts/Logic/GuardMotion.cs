using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nullflame.Logic
{
    public enum GuardMoveState { Idle, Travel, Post, Reversing, Hold, Detour }

    public struct RoutePoint
    {
        public Vector3 Position;
        public float Dwell;
        public bool HasLook;
        public Vector3 LookAt;
    }

    // Deterministic guard movement and history (design doc §8.1, §8.2, §7.4; IMPL §4.2).
    // Travel records history every historyStep seconds of walking plus every waypoint reached, so the
    // recorded polyline holds every corner and reversing retraces the exact path. Post never records.
    public sealed class GuardMotion
    {
        const float Epsilon = 0.001f;

        readonly IReadOnlyList<RoutePoint> travel, post;
        readonly float walkSpeed, reverseSpeed, historyStep;
        readonly List<Vector3> history = new List<Vector3>(256);

        int next;             // index of the point being walked to (travel or post)
        float dwellLeft;
        float sampleTimer;
        List<RoutePoint> detour; // out to a point, dwell, back to where the guard left its route (AD-021)
        int detourNext;
        GuardMoveState resumeState;

        public GuardMotion(IReadOnlyList<RoutePoint> travel, IReadOnlyList<RoutePoint> post,
            float walkSpeed, float reverseMultiplier, float historyStep)
        {
            this.travel = travel;
            this.post = post ?? Array.Empty<RoutePoint>();
            this.walkSpeed = walkSpeed;
            reverseSpeed = walkSpeed * reverseMultiplier;
            this.historyStep = historyStep;
            Position = travel[0].Position;
            Facing = Vector3.forward;
        }

        public GuardMoveState State { get; private set; } = GuardMoveState.Idle;
        public Vector3 Position { get; private set; }
        public Vector3 Facing { get; private set; }
        public bool Paused { get; set; }
        public bool ReachedEnd { get; private set; }
        public int HistoryCount => history.Count;
        public IReadOnlyList<Vector3> History => history;
        public event Action ArrivedAtEnd;

        public void Start()
        {
            if (State != GuardMoveState.Idle) return;
            State = GuardMoveState.Travel;
            Position = travel[0].Position;
            AddHistory(Position);
            dwellLeft = travel[0].Dwell;
            next = 1;
            if (travel.Count > 1) Facing = FlatDir(travel[1].Position - Position, Facing);
        }

        public void BeginReverse()
        {
            if (history.Count == 0) return; // never started, or reversed back to spawn
            State = GuardMoveState.Reversing;
        }

        public void EndReverse()
        {
            if (State == GuardMoveState.Reversing) State = GuardMoveState.Hold;
        }

        // Leaves the route along `path` (the last point is the errand), waits `dwell` there, walks the same
        // points back and resumes the route from where it left. Travel detours are recorded in history.
        public bool Detour(IReadOnlyList<Vector3> path, float dwell)
        {
            if ((State != GuardMoveState.Travel && State != GuardMoveState.Post) || path.Count == 0) return false;
            detour = new List<RoutePoint>(path.Count * 2 + 1);
            for (int i = 0; i < path.Count; i++) detour.Add(new RoutePoint { Position = path[i] });
            RoutePoint end = detour[detour.Count - 1];
            end.Dwell = dwell;
            detour[detour.Count - 1] = end;
            for (int i = path.Count - 2; i >= 0; i--) detour.Add(new RoutePoint { Position = path[i] });
            detour.Add(new RoutePoint { Position = Position });
            detourNext = 0;
            dwellLeft = 0f;
            resumeState = State;
            State = GuardMoveState.Detour;
            return true;
        }

        public void Tick(float dt)
        {
            if (Paused) return;
            switch (State)
            {
                case GuardMoveState.Travel: TickTravel(dt); break;
                case GuardMoveState.Post: TickPost(dt); break;
                case GuardMoveState.Reversing: TickReverse(dt); break;
                case GuardMoveState.Detour: TickDetour(dt); break;
            }
        }

        void TickTravel(float dt)
        {
            if (Walk(travel, ref next, dt, record: true)) EndTravel();
        }

        void TickDetour(float dt)
        {
            if (!Walk(detour, ref detourNext, dt, record: resumeState == GuardMoveState.Travel)) return;
            detour = null;
            State = resumeState;
        }

        // Walks pts from idx; true once past the last point with time to spare (the caller moves on).
        bool Walk(IReadOnlyList<RoutePoint> pts, ref int idx, float dt, bool record)
        {
            while (dt > 0f)
            {
                if (dwellLeft > 0f)
                {
                    dt = Dwell(pts[idx - 1], dt);
                    continue;
                }
                if (idx >= pts.Count) return true;
                Vector3 target = pts[idx].Position;
                Vector3 to = target - Position;
                float timeToTarget = to.magnitude / walkSpeed;
                float chunk = record ? Mathf.Min(dt, historyStep - sampleTimer, timeToTarget) : Mathf.Min(dt, timeToTarget);
                Facing = FlatDir(to, Facing);
                dt -= chunk;
                if (record) sampleTimer += chunk;
                if (chunk >= timeToTarget)
                {
                    Position = target;
                    if (record) AddHistory(Position); // corner
                    dwellLeft = pts[idx].Dwell;
                    idx++;
                }
                else Position += to * (chunk / timeToTarget);
                if (record && sampleTimer >= historyStep - 1e-6f)
                {
                    sampleTimer = 0f;
                    AddHistory(Position);
                }
            }
            return false;
        }

        void EndTravel()
        {
            ReachedEnd = true;
            if (post.Count > 0)
            {
                State = GuardMoveState.Post;
                next = 0;
                dwellLeft = 0f;
            }
            else State = GuardMoveState.Idle;
            ArrivedAtEnd?.Invoke();
        }

        void TickPost(float dt)
        {
            int guard = 0;
            while (dt > 0f && guard++ < 64)
            {
                if (dwellLeft > 0f)
                {
                    dt = Dwell(post[(next + post.Count - 1) % post.Count], dt);
                    continue;
                }
                Vector3 to = post[next].Position - Position;
                float timeToTarget = to.magnitude / walkSpeed;
                Facing = FlatDir(to, Facing);
                if (dt >= timeToTarget)
                {
                    dt -= timeToTarget;
                    Position = post[next].Position;
                    dwellLeft = post[next].Dwell;
                    next = (next + 1) % post.Count;
                }
                else
                {
                    Position += to * (dt / timeToTarget);
                    dt = 0f;
                }
            }
        }

        // Walks to the last history point, then back through history, removing each point when reached.
        // The guard faces opposite its motion, i.e. the way it originally walked.
        void TickReverse(float dt)
        {
            while (dt > 0f && history.Count > 0)
            {
                Vector3 to = history[history.Count - 1] - Position;
                float timeToTarget = to.magnitude / reverseSpeed;
                Facing = FlatDir(-to, Facing);
                if (dt >= timeToTarget)
                {
                    dt -= timeToTarget;
                    Position = history[history.Count - 1];
                    history.RemoveAt(history.Count - 1);
                }
                else
                {
                    Position += to * (dt / timeToTarget);
                    dt = 0f;
                }
            }
            if (history.Count == 0) State = GuardMoveState.Hold;
        }

        float Dwell(RoutePoint p, float dt)
        {
            if (p.HasLook) Facing = FlatDir(p.LookAt - Position, Facing);
            float used = Mathf.Min(dt, dwellLeft);
            dwellLeft -= used;
            return dt - used;
        }

        void AddHistory(Vector3 p)
        {
            if (history.Count > 0 && (history[history.Count - 1] - p).sqrMagnitude < Epsilon * Epsilon) return;
            history.Add(p);
        }

        static Vector3 FlatDir(Vector3 v, Vector3 fallback)
        {
            v.y = 0f;
            float m = v.magnitude;
            return m > Epsilon ? v / m : fallback;
        }
    }
}
