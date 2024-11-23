using UnityEngine;
using ProjectColombo.Combat;
using ProjectColombo.Core;

namespace ProjectColombo.Control
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EntityAttributes))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the GameInput script.")]
        [SerializeField] GameInput gameInput;

        EntityAttributes entityAttributes;

        Rigidbody playerRigidbody;
        Animator playerAnimator;
        CapsuleCollider capsuleCollider;

        Vector2 movementInput;
        Vector3 currentVelocity = Vector3.zero;

        float timeSinceLastInput = 0f;
        float rollCooldown = 1f;
        float timeSinceLastRoll = Mathf.Infinity;

        bool isAttacking = false;
        bool isRolling = false;

        int originalLayer;

        void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            playerAnimator = GetComponent<Animator>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            if (entityAttributes == null)
            {
                entityAttributes = GetComponent<EntityAttributes>();
            }

            // Store the original layer
            originalLayer = gameObject.layer;
        }

        void FixedUpdate()
        {
            Move();

            if (!isRolling)
            {
                Rotate();
            }
        }

        void Update()
        {
            UpdateAnimator();

            timeSinceLastRoll += Time.deltaTime;

            movementInput = gameInput.MovementInput;

            if (gameInput.attackPressed && !isAttacking && !isRolling)
            {
                GetComponent<Fight>().Attack();
                gameInput.ResetAttackPressed();
            }

            if (isAttacking && movementInput.sqrMagnitude > 0.01f)
            {
                GetComponent<Fight>().CancelAction();
            }

            if (Input.GetKeyDown(KeyCode.Space) && timeSinceLastRoll >= rollCooldown && !isRolling)
            {
                Roll();
            }

            PlayerAnimationChecks();
        }

        void PlayerAnimationChecks()
        {
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsTag("Attack"))
            {
                isAttacking = true;
            }
            else
            {
                isAttacking = false;
            }

            bool wasRolling = isRolling;

            if (stateInfo.IsTag("Roll"))
            {
                isRolling = true;
            }
            else
            {
                isRolling = false;
            }

            if (!wasRolling && isRolling)
            {
                OnRollStart();
            }
            else if (wasRolling && !isRolling)
            {
                OnRollEnd();
            }
        }

        void OnRollStart()
        {
            // Change the player's layer to NoHit
            gameObject.layer = LayerMask.NameToLayer("NoHit");
        }

        void OnRollEnd()
        {
            // Revert the player's layer to the original layer
            gameObject.layer = originalLayer;
        }

        void Roll()
        {
            playerAnimator.SetTrigger("roll");
            timeSinceLastRoll = 0f;

            float rollDuration = 1f;
            float rollDistance = 5f; // Adjust based on desired roll distance
            float rollSpeed = rollDistance / rollDuration;

            // Determine roll direction
            Vector3 rollDirection;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                rollDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
            }
            else if (currentVelocity.sqrMagnitude > 0.01f)
            {
                rollDirection = currentVelocity.normalized;
            }
            else
            {
                rollDirection = transform.forward;
            }

            currentVelocity = rollDirection * rollSpeed;
        }

        void Move()
        {
            if (isRolling)
            {
                // Move the player based on currentVelocity
                playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
            }
            else
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
                    desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y).normalized * entityAttributes.moveSpeed;
                }
                else if (timeSinceLastInput < entityAttributes.graceTime && !isAttacking)
                {
                    desiredVelocity = currentVelocity.normalized * entityAttributes.moveSpeed;
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
                    accelerationRate = entityAttributes.deceleration * 2f;
                }
                else
                {
                    accelerationRate = (Mathf.Abs(speedDifference) > 0.01f) ? entityAttributes.acceleration : entityAttributes.deceleration;
                }

                float maxSpeedChange = accelerationRate * Time.fixedDeltaTime;

                // Smoothly adjust currentVelocity towards desiredVelocity
                currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);

                // Move the player
                playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
            }
        }

        void Rotate()
        {
            // Rotate only if moving
            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, entityAttributes.rotationSpeedPlayer * Time.fixedDeltaTime);
                playerRigidbody.MoveRotation(newRotation);
            }
        }

        void UpdateAnimator()
        {
            // Use current velocity magnitude for animation speed
            float speed = currentVelocity.magnitude;

            playerAnimator.SetFloat("speed", speed);
        }
    }
}
