using UnityEngine;
using ProjectColombo.Combat;
using ProjectColombo.Core;

namespace ProjectColombo.Control
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EntityAttributes))]
    [RequireComponent(typeof(PlayerAnimator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the GameInput script.")]
        [SerializeField] GameInput gameInput;

        EntityAttributes entityAttributes;
        PlayerAnimator playerAnimator;

        Rigidbody playerRigidbody;
        CapsuleCollider capsuleCollider;

        Vector2 movementInput;
        Vector3 currentVelocity = Vector3.zero;

        float timeSinceLastInput = 0f;
        float rollCooldown = 1f;
        float timeSinceLastRoll = Mathf.Infinity;

        bool isAttacking = false;
        bool isRolling = false;

        void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            entityAttributes = GetComponent<EntityAttributes>();
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        void FixedUpdate()
        {
            if (!isRolling)
            {
                Move();
                Rotate();
            }
            else
            {
                RollMove();
            }
        }

        void Update()
        {
            movementInput = gameInput.MovementInput;
            timeSinceLastRoll += Time.deltaTime;

            // Combat-related input
            if (gameInput.AttackPressed && !isAttacking && !isRolling)
            {
                GetComponent<Fight>().Attack();
                gameInput.ResetAttackPressed();
            }

            // Roll input
            if (gameInput.RollPressed && timeSinceLastRoll >= rollCooldown && !isRolling)
            {
                StartRoll();
                gameInput.ResetRollPressed();
            }

            playerAnimator.UpdateAnimator(currentVelocity.magnitude, isRolling, movementInput.sqrMagnitude > 0.01f);
        }

        void Move()
        {
            if (movementInput.sqrMagnitude < 0.01f)
            {
                timeSinceLastInput += Time.fixedDeltaTime;
            }
            else
            {
                timeSinceLastInput = 0f;
            }

            Vector3 desiredVelocity = movementInput.sqrMagnitude > 0.01f ? new Vector3(movementInput.x, 0, movementInput.y).normalized * entityAttributes.moveSpeed : (timeSinceLastInput < entityAttributes.graceTime ? currentVelocity.normalized * entityAttributes.moveSpeed : Vector3.zero);

            float speedDifference = desiredVelocity.magnitude - currentVelocity.magnitude;
            float accelerationRate = Mathf.Abs(speedDifference) > 0.01f ? entityAttributes.acceleration : entityAttributes.deceleration;
            float maxSpeedChange = accelerationRate * Time.fixedDeltaTime;

            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);
            playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
        }

        void Rotate()
        {
            if (currentVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
                Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, entityAttributes.rotationSpeedPlayer * Time.fixedDeltaTime);
                playerRigidbody.MoveRotation(newRotation);
            }
        }

        void RollMove()
        {
            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 newDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
                currentVelocity = newDirection * currentVelocity.magnitude;

                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);

                float rotationSpeed = Mathf.Lerp(0.1f, entityAttributes.rotationSpeedPlayer, Time.fixedDeltaTime);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
            }

            playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
        }

        void StartRoll()
        {
            isRolling = true;
            playerAnimator.TriggerRoll();
            timeSinceLastRoll = 0f;

            float rollDuration = 1f;
            float rollDistance = 5f;
            float rollSpeed = rollDistance / rollDuration;

            Vector3 rollDirection = movementInput.sqrMagnitude > 0.01f ? new Vector3(movementInput.x, 0, movementInput.y).normalized : currentVelocity.sqrMagnitude > 0.01f ? currentVelocity.normalized : transform.forward;

            currentVelocity = rollDirection * rollSpeed;
        }

        public void EndRoll()
        {
            isRolling = false;
            currentVelocity = Vector3.zero;
        }
    }
}
