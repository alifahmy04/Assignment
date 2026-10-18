using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace ElementalAnomaly.Prototype
{
    // Input changes an independent orbit target; Cinemachine owns the rendering camera.
    [DefaultExecutionOrder(-75)]
    public sealed class PrototypeCinemachineExploration : MonoBehaviour
    {
        public PrototypePlayerController player;
        public Transform playerCameraTarget;
        public Transform orbitTarget;
        public CinemachineCamera explorationCamera;
        public CinemachineThirdPersonFollow follow;
        public CinemachineCamera aimingCamera;

        [Header("Aiming")]
        [SerializeField, Range(0.1f, 1f)] private float aimingSensitivityMultiplier = 0.65f;
        public bool IsAiming { get; private set; }
        public bool CinematicActive { get; private set; }

        [Header("Orbit Input")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(0f)] private float gamepadSensitivity = 120f;
        [SerializeField, Range(-80f, 0f)] private float minimumPitch = -30f;
        [SerializeField, Range(0f, 80f)] private float maximumPitch = 70f;
        [SerializeField, Range(-80f, 80f)] private float initialPitch = 20f;

        [Header("Scroll Zoom")]
        [SerializeField, Min(0f)] private float scrollZoomSensitivity = 0.5f;
        [SerializeField, Min(0.5f)] private float minimumDistance = 2f;
        [SerializeField, Min(0.5f)] private float maximumDistance = 8f;
        [SerializeField, Min(0.5f)] private float distance = 5f;
        [Tooltip("Time for scroll zoom to ease toward the requested distance. Zero gives immediate zoom.")]
        [SerializeField, Min(0f)] private float zoomSmoothTime = 0.15f;

        private float yaw, pitch;
        private float smoothedDistance, zoomVelocity;

        private void Start()
        {
            if (player == null || playerCameraTarget == null || orbitTarget == null || explorationCamera == null || follow == null)
            {
                Debug.LogError("Exploration camera rig has a missing reference.", this);
                enabled = false;
                return;
            }
            yaw = player.transform.eulerAngles.y;
            pitch = Mathf.Clamp(initialPitch, minimumPitch, maximumPitch);
            smoothedDistance = Mathf.Clamp(distance, minimumDistance, maximumDistance);
            zoomVelocity = 0f;
            SetAiming(false);
            ApplyOrbit();
            explorationCamera.PreviousStateIsValid = false;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SetAiming(false);
                return;
            }
            if (CinematicActive) return;
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            bool aimRequested = mouse != null && Cursor.lockState == CursorLockMode.Locked && mouse.rightButton.isPressed;
            if (Gamepad.current != null) aimRequested |= Gamepad.current.leftTrigger.isPressed;
            SetAiming(aimRequested);

            Vector2 look = Vector2.zero;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                look += mouse.delta.ReadValue() * mouseSensitivity;
            if (Gamepad.current != null)
                look += Gamepad.current.rightStick.ReadValue() * gamepadSensitivity * Time.unscaledDeltaTime;
            if (IsAiming) look *= aimingSensitivityMultiplier;
            yaw = Mathf.Repeat(yaw + look.x, 360f);
            pitch = Mathf.Clamp(pitch - look.y, minimumPitch, maximumPitch);

            if (mouse != null && !IsAiming)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance -= Mathf.Clamp(scroll, -1f, 1f) * scrollZoomSensitivity;
            }
            distance = Mathf.Clamp(distance, minimumDistance, maximumDistance);
            ApplyOrbit();
        }

        private void LateUpdate()
        {
            // This runs before the Brain's LateUpdate, after CharacterController movement.
            smoothedDistance = zoomSmoothTime > 0f
                ? Mathf.SmoothDamp(smoothedDistance, distance, ref zoomVelocity, zoomSmoothTime, Mathf.Infinity, Time.deltaTime)
                : distance;
            ApplyOrbit();
        }

        private void ApplyOrbit()
        {
            if (orbitTarget == null || playerCameraTarget == null || follow == null) return;
            orbitTarget.SetPositionAndRotation(playerCameraTarget.position, Quaternion.Euler(pitch, yaw, 0f));
            follow.CameraDistance = smoothedDistance;
            if (player != null && !player.InputLocked) player.FaceAimDirection(orbitTarget.forward);
        }

        public void SetCinematicActive(bool active)
        {
            CinematicActive = active;
            if (active) SetAiming(false);
        }

        private void SetAiming(bool aiming)
        {
            IsAiming = aiming && aimingCamera != null && !CinematicActive;
            if (aimingCamera != null) aimingCamera.Priority = IsAiming ? 20 : 0;
        }

        private void OnDisable()
        {
            SetAiming(false);
        }
    }
}
