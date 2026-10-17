using System;
using UnityEngine;
using Nullflame.Logic;

namespace Nullflame
{
    public enum ShadowOpacity { Opaque, Soft }

    // A gameplay shadow: a flat footprint the entity can live in (design doc §7.1, IMPL §7.1).
    // Gameplay reads only these, never lights. The child quad is the visual.
    public sealed class ShadowArea : MonoBehaviour
    {
        [Tooltip("What casts this shadow. Empty for the Player Room floor.")]
        public GameObject owner;
        public ShadowOpacity opacity = ShadowOpacity.Opaque;
        [Tooltip("Floor footprint in metres (local X, local Z).")]
        public Vector2 size = Vector2.one;
        [Tooltip("True for shadows parented to a guard (riding).")]
        public bool isMoving;
        [Tooltip("True only on the breaker lever panel's shadow (facility rewind spot).")]
        public bool isRewindSpot;
        [Tooltip("Optional: what holding E in this shadow eats.")]
        public Consumable consumable;
        [Tooltip("Pipe strips only: the crack point holding E acts on.")]
        public CrackablePipe pipe;
        [Tooltip("Oil drops and void shadows: destroyed when lantern fire reaches them.")]
        public bool destroyedByFire;
        [SerializeField] Transform decal;

        public event Action<ShadowArea> Removed;

        public Vector3 Center => transform.position;
        public float Yaw => transform.eulerAngles.y;
        public float Area => size.x * size.y;

        public bool Contains(Vector3 p) => ShadowMath.Contains(Center, Yaw, size, p);
        public Vector3 ClosestPoint(Vector3 p) => ShadowMath.ClosestPoint(Center, Yaw, size, p);
        public Vector3 ClampInside(Vector3 p, float margin) => ShadowMath.ClampInside(Center, Yaw, size, margin, p);

        void OnEnable()
        {
            if (GameContext.Current != null) GameContext.Current.shadows.Register(this);
        }

        void OnDisable()
        {
            if (GameContext.Current != null) GameContext.Current.shadows.Unregister(this);
            Removed?.Invoke(this);
        }

        public void SetSize(Vector2 newSize)
        {
            size = newSize;
            if (decal != null) decal.localScale = new Vector3(size.x, size.y, 1f);
        }

        void OnValidate() => SetSize(size);

        void OnDrawGizmos()
        {
            Gizmos.color = opacity == ShadowOpacity.Opaque ? Color.white : Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(Center, Quaternion.Euler(0f, Yaw, 0f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.up * 0.02f, new Vector3(size.x, 0f, size.y));
        }
    }
}
