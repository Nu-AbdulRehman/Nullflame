using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    // Drives Art/Guard.controller on the guard's visual (IMPL §10.6). Animation never moves the guard:
    // root motion is off and position comes from GuardMotion (CLAUDE.md §5.2). No Animator, no-op.
    public sealed class GuardAnimation : MonoBehaviour
    {
        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int DirectionId = Animator.StringToHash("Direction");
        static readonly int AlertId = Animator.StringToHash("Alert");
        static readonly int ThrowId = Animator.StringToHash("Throw");
        static readonly int ConsumedId = Animator.StringToHash("Consumed");

        GuardController guard;
        Animator animator;
        Vector3 last;

        void Awake()
        {
            guard = GetComponent<GuardController>();
            animator = GetComponentInChildren<Animator>();
            last = transform.position;
        }

        void Update()
        {
            if (animator == null) return;
            Vector3 p = transform.position;
            float speed = Time.deltaTime > 0f ? Vector3.Distance(p, last) / Time.deltaTime : 0f;
            last = p;
            animator.SetFloat(SpeedId, speed);
            animator.SetFloat(DirectionId, guard.Motion.State == GuardMoveState.Reversing ? -1f : 1f);
            animator.SetBool(ConsumedId, guard.IsBeingConsumed);
        }

        public void Alert()
        {
            if (animator != null) animator.SetTrigger(AlertId);
        }

        public void Throw()
        {
            if (animator != null) animator.SetTrigger(ThrowId);
        }
    }
}
