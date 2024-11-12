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

        Health health;

        Rigidbody playerRigidbody;
        Animator animator;

        InputSystem_Actions playerInputActions;


        Vector2 movementInput;
        Vector3 currentVelocity = Vector3.zero;

        float timeSinceLastInput = 0f;

        bool isAttacking = false;

        void Awake()
        {
            health = GetComponent<Health>();
            playerRigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();

            playerInputActions = new InputSystem_Actions();
        }

        void OnEnable()
        {
            playerInputActions.Player.Enable();
            playerInputActions.Player.Move.performed += OnMovePerformed;
            playerInputActions.Player.Move.canceled += OnMoveCanceled;
            playerInputActions.Player.Attack.performed += OnAttackPerformed;
        }

        void OnDisable()
        {
            playerInputActions.Player.Move.performed -= OnMovePerformed;
            playerInputActions.Player.Move.canceled -= OnMoveCanceled;
            playerInputActions.Player.Attack.performed -= OnAttackPerformed;
            playerInputActions.Player.Disable();
        }

        void FixedUpdate()
        {
            if (health.GetIsDead()) return;

            Move();
            Rotate();
        }

        void Update()
        {
            UpdateAnimator();

            // Check if we are in the attack animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsTag("Attack"))
            {
                isAttacking = true;
            }
            else
            {
                isAttacking = false;
            }

            if(Input.GetKey(KeyCode.P))
            {
                GetComponent<Health>().TakeDamage(health.GetCurrentHealth());
            }
        }

        void Move()
        {
            // Update time since last input
            if (movementInput.sqrMagnitude < 0.01f)
            {
                timeSinceLastInput += Time.fixedDeltaTime;
            }
            else
            {
                timeSinceLastInput = 0f;
            }

            // Calculate desired velocity based on input or maintain current velocity during grace period
            Vector3 desiredVelocity;

            if (movementInput.sqrMagnitude > 0.01f && !isAttacking)
            {
                desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y).normalized * moveSpeed;
            }
            else if (timeSinceLastInput < graceTime && !isAttacking)
            {
                desiredVelocity = currentVelocity.normalized * moveSpeed;
            }
            else
            {
                desiredVelocity = Vector3.zero;
            }

            // Determine acceleration or deceleration
            float speedDifference = desiredVelocity.magnitude - currentVelocity.magnitude;
            float accelerationRate;

            if (isAttacking)
            {
                // Increase deceleration when attacking to stop quickly
                accelerationRate = deceleration * 2f;
            }
            else
            {
                accelerationRate = (Mathf.Abs(speedDifference) > 0.01f) ? acceleration : deceleration;
            }

            float maxSpeedChange = accelerationRate * Time.fixedDeltaTime;

            // Smoothly adjust current velocity towards desired velocity
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);

            // Move the player
            playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
        }

        void Rotate()
        {
            // Rotate only if moving
            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                playerRigidbody.MoveRotation(newRotation);
            }
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();

            // Normalize input for consistent diagonal movement
            if (movementInput.magnitude > 1f)
            {
                movementInput = movementInput.normalized;
            }

            // If we are attacking, cancel the attack
            if (isAttacking)
            {
                GetComponent<Fight>().CancelAction();
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
            timeSinceLastInput = 0f; // Start grace period
        }

        void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!isAttacking)
            {
                GetComponent<Fight>().Attack();
            }
        }

        void UpdateAnimator()
        {
            // Use current velocity magnitude for animation speed
            float speed = currentVelocity.magnitude;

            animator.SetFloat("speed", speed);
        }
    }
}
