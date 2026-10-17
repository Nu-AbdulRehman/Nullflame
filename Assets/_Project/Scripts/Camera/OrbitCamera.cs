using UnityEngine;

namespace Nullflame
{
    // Third-person orbit rig (design doc §11, IMPL §8). Sits on CameraRig; the Main Camera is its child.
    public sealed class OrbitCamera : MonoBehaviour
    {
        [SerializeField] GameTuning tuning;
        [SerializeField] InputReader input;
        [SerializeField] Transform target;
        [SerializeField] Transform cameraTransform;

        const float PivotHeight = 0.5f; // look point above the floor decal

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        Vector3 pivot;
        Vector3 pivotVelocity;
        int solidMask;
        Transform blendFrom;
        float blendTime, blendElapsed;

        void Awake()
        {
            solidMask = LayerMask.GetMask("LevelSolid");
            Pitch = tuning.cameraPitchStart;
            Yaw = transform.eulerAngles.y;
            if (target != null) pivot = target.position + Vector3.up * PivotHeight;
        }

        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Intro (IMPL �21): hold the camera on a scripted pose while the rig is off.
        public void ShowPose(Transform pose) => cameraTransform.SetPositionAndRotation(pose.position, pose.rotation);

        // Turns the rig on and eases from the pose to the gameplay camera over `time` seconds.
        public void BlendFrom(Transform pose, float time)
        {
            blendFrom = pose;
            blendTime = time;
            blendElapsed = 0f;
            enabled = true;
        }

        public void Rotate(Vector2 lookDelta)
        {
            Yaw += lookDelta.x * tuning.mouseSensitivity;
            Pitch = Mathf.Clamp(Pitch - lookDelta.y * tuning.mouseSensitivity, tuning.cameraPitchMin, tuning.cameraPitchMax);
        }

        void LateUpdate()
        {
            if (input != null && blendFrom == null) Rotate(input.Look);
            if (target == null) return;

            pivot = Vector3.SmoothDamp(pivot, target.position + Vector3.up * PivotHeight, ref pivotVelocity, tuning.cameraFollowDamping);
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0f);
            Vector3 back = rotation * Vector3.back;
            float distance = tuning.cameraDistance;
            if (Physics.SphereCast(pivot, tuning.cameraCollisionRadius, back, out RaycastHit hit, distance, solidMask, QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            transform.position = pivot;
            transform.rotation = rotation;
            Vector3 position = pivot + back * distance;
            if (blendFrom != null)
            {
                blendElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, blendElapsed / blendTime);
                position = Vector3.Lerp(blendFrom.position, position, t);
                rotation = Quaternion.Slerp(blendFrom.rotation, rotation, t);
                if (blendElapsed >= blendTime) blendFrom = null;
            }
            cameraTransform.SetPositionAndRotation(position, rotation);
        }
    }
}
