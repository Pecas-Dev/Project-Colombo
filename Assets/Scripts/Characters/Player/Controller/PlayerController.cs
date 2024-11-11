using UnityEngine;
using ProjectColombo.Combat;
using ProjectColombo.Core;
using UnityEngine.InputSystem;

namespace ProjectColombo.Control
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float rotationSpeed = 720f; // Degrees per second
        [SerializeField] float acceleration = 10f;
        [SerializeField] float deceleration = 10f;
        [SerializeField] float graceTime = 0.1f; // Duration of grace period in seconds

        private Health health;
        private Rigidbody playerRigidbody;

        private InputSystem_Actions playerInputActions;

        private Vector2 movementInput;
        private Vector3 currentVelocity = Vector3.zero;
        private float timeSinceLastInput = 0f;

        void Awake()
        {
            health = GetComponent<Health>();
            playerRigidbody = GetComponent<Rigidbody>();
            playerInputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            playerInputActions.Player.Enable();
            playerInputActions.Player.Move.performed += OnMovePerformed;
            playerInputActions.Player.Move.canceled += OnMoveCanceled;
            playerInputActions.Player.Attack.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;
            playerInputActions.Player.Attack.performed -= OnAttackPerformed;
            playerInputActions.Player.Disable();
        }

        private void FixedUpdate()
        {
            if (health.GetIsDead()) return;

            Move();
            Rotate();
        }

        void Update()
        {
            UpdateAnimator();
        }

        private void Move()
        {
            // Update time since last input
            if (movementInput.sqrMagnitude < 0.01f)
            {
                timeSinceLastInput += Time.fixedDeltaTime;
            }
            else
            {
                timeSinceLastInput = 0f; // Reset timer if there's input
            }

            // Calculate desired velocity based on input or maintain current velocity during grace period
            Vector3 desiredVelocity;
            if (movementInput.sqrMagnitude > 0.01f)
            {
                desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y).normalized * moveSpeed;
            }
            else if (timeSinceLastInput < graceTime)
            {
                desiredVelocity = currentVelocity.normalized * moveSpeed;
            }
            else
            {
                desiredVelocity = Vector3.zero;
            }

            // Determine acceleration or deceleration
            float speedDifference = desiredVelocity.magnitude - currentVelocity.magnitude;
            float accelerationRate = (Mathf.Abs(speedDifference) > 0.01f) ? acceleration : deceleration;
            float maxSpeedChange = accelerationRate * Time.fixedDeltaTime;

            // Smoothly adjust current velocity towards desired velocity
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);

            // Move the player
            playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
        }

        private void Rotate()
        {
            // Rotate only if moving
            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                playerRigidbody.MoveRotation(newRotation);
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();

            // Normalize input for consistent diagonal movement
            if (movementInput.magnitude > 1f)
            {
                movementInput = movementInput.normalized;
            }
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
            timeSinceLastInput = 0f; // Start grace period
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            // Implement attack logic here
            GetComponent<Fight>().Attack();
        }

        void UpdateAnimator()
        {
            // Use current velocity magnitude for animation speed
            float speed = currentVelocity.magnitude;
            GetComponent<Animator>().SetFloat("speed", speed);
        }
    }
}
