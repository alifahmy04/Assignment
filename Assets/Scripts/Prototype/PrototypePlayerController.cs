using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalAnomaly.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PrototypePlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(1f)] private float sprintMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;

        [Header("Airborne")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -25f;

        private CharacterController characterController;
        private Transform movementCamera;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private float verticalVelocity;

        public Transform CameraTarget { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            movementCamera = Camera.main != null ? Camera.main.transform : null;

            CameraTarget = transform.Find("Camera Target");
            if (CameraTarget == null)
            {
                var targetObject = new GameObject("Camera Target");
                CameraTarget = targetObject.transform;
                CameraTarget.SetParent(transform, false);
                CameraTarget.localPosition = new Vector3(0f, 1.4f, 0f);
            }

            CreateInputActions();
        }

        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
        }

        private void OnDestroy()
        {
            moveAction.Dispose();
            jumpAction.Dispose();
            sprintAction.Dispose();
        }

        private void Update()
        {
            if (movementCamera == null && Camera.main != null)
            {
                movementCamera = Camera.main.transform;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 desiredDirection = GetCameraRelativeDirection(input);

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }

            float speed = moveSpeed * (sprintAction.IsPressed() ? sprintMultiplier : 1f);
            UpdateVerticalVelocity();

            Vector3 velocity = desiredDirection * speed;
            velocity.y = verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            Vector3 forward = movementCamera != null ? movementCamera.forward : Vector3.forward;
            Vector3 right = movementCamera != null ? movementCamera.right : Vector3.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }

        private void UpdateVerticalVelocity()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (characterController.isGrounded && jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private void CreateInputActions()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");

            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            sprintAction.AddBinding("<Gamepad>/leftStickPress");
        }
    }
}
