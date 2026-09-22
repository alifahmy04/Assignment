using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    [RequireComponent(typeof(Camera))]
    public sealed class PrototypeThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Orbit")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(0f)] private float gamepadSensitivity = 120f;
        [SerializeField, Range(-80f, 0f)] private float minimumPitch = -30f;
        [SerializeField, Range(0f, 80f)] private float maximumPitch = 70f;

        [Header("Framing")]
        [Tooltip("Moves the camera and its aim point to the right of the player, keeping the reticle clear.")]
        [SerializeField] private float shoulderOffset = 0.9f;
        [SerializeField, Min(1f)] private float distance = 5f;
        [SerializeField, Min(1f)] private float minimumDistance = 2f;
        [SerializeField, Min(2f)] private float maximumDistance = 8f;
        [SerializeField, Min(0f)] private float positionSmoothTime = 0.06f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField, Min(0.01f)] private float collisionRadius = 0.2f;
        [SerializeField, Min(0f)] private float collisionPadding = 0.1f;

        private float yaw;
        private float pitch = 20f;
        private Vector3 smoothVelocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            HandleCursor();
            HandleOrbitInput();
            HandleZoomInput();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            // Offset the aim point too: looking back at the player would re-center them.
            Vector3 pivot = target.position + orbitRotation * Vector3.right * shoulderOffset;
            Vector3 backwards = orbitRotation * Vector3.back;
            float correctedDistance = GetCollisionCorrectedDistance(pivot, backwards);
            Vector3 desiredPosition = pivot + backwards * correctedDistance;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                positionSmoothTime);
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        }

        private void HandleCursor()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandleOrbitInput()
        {
            Vector2 lookDelta = Vector2.zero;

            if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
            {
                lookDelta += Mouse.current.delta.ReadValue() * mouseSensitivity;
            }

            if (Gamepad.current != null)
            {
                lookDelta += Gamepad.current.rightStick.ReadValue()
                    * gamepadSensitivity
                    * Time.unscaledDeltaTime;
            }

            yaw += lookDelta.x;
            pitch = Mathf.Clamp(pitch - lookDelta.y, minimumPitch, maximumPitch);
        }

        private void HandleZoomInput()
        {
            if (Mouse.current == null)
            {
                return;
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance - scroll * 0.01f, minimumDistance, maximumDistance);
            }
        }

        private float GetCollisionCorrectedDistance(Vector3 pivot, Vector3 backwards)
        {
            if (Physics.SphereCast(
                    pivot,
                    collisionRadius,
                    backwards,
                    out RaycastHit hit,
                    distance,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(minimumDistance, hit.distance - collisionPadding);
            }

            return distance;
        }
    }
}
