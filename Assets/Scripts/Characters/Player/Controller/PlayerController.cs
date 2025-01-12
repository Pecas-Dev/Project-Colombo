using ProjectColombo.Combat;
using ProjectColombo.Input;

using UnityEngine;


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
        Vector3 rollDirection = Vector3.zero;


        //float rollCooldown = 1f;
        //float timeSinceLastRoll = Mathf.Infinity;
        //float rollEndTime = 0f; 


        bool isAttacking = false;
        //bool isRolling = false;
        //bool canQueueRoll = true; 

        void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            entityAttributes = GetComponent<EntityAttributes>();
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        void FixedUpdate()
        {
            /*if (!isRolling)
            {
                Move();
                Rotate();
            }
            else
            {*/
                //RollMove();
            //}
        }

        void Update()
        {
            //movementInput = gameInput.MovementInput;
            //timeSinceLastRoll += Time.deltaTime;

            if (gameInput.AttackPressed && !isAttacking) //&& !isRolling)
            {
                GetComponent<Fight>().Attack();
                gameInput.ResetAttackPressed();
            }

            /*if (gameInput.RollPressed && CanRoll())
            {
                StartRoll();
                gameInput.ResetRollPressed();
            }*/

            //playerAnimator.UpdateAnimator(currentVelocity.magnitude, isRolling, movementInput.sqrMagnitude > 0.01f);
        }

        /*bool CanRoll()
        {
            return timeSinceLastRoll >= rollCooldown && Time.time >= rollEndTime + entityAttributes.rollDelay && !isRolling && canQueueRoll;
        }*/

        /*void Move()
        {
            //if (movementInput.sqrMagnitude < 0.01f)
            //{
            //    timeSinceLastInput += Time.fixedDeltaTime;
            //}
            //else
            //{
            //    timeSinceLastInput = 0f;
            //}

            Vector3 desiredVelocity = movementInput.sqrMagnitude > 0.01f ? new Vector3(movementInput.x, 0, movementInput.y).normalized * entityAttributes.moveSpeed : (timeSinceLastInput < entityAttributes.graceTime ? currentVelocity.normalized * entityAttributes.moveSpeed : Vector3.zero);

            float accelerationRate = entityAttributes.acceleration * (movementInput.sqrMagnitude > 0.01f ? 1.5f : 1.0f);
            float decelerationRate = entityAttributes.deceleration * 1.8f;

            float maxSpeedChange = (movementInput.sqrMagnitude > 0.01f ? accelerationRate : decelerationRate) * Time.fixedDeltaTime;
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);

            playerRigidbody.MovePosition(playerRigidbody.position + currentVelocity * Time.fixedDeltaTime);
        }

        void Rotate()
        {
            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 targetDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, entityAttributes.rotationSpeedPlayer * Time.fixedDeltaTime * 2f);
            }
        }*/

        /*void RollMove()
        {
            playerRigidbody.MovePosition(playerRigidbody.position + rollDirection * Time.fixedDeltaTime);
        }

        void StartRoll()
        {
            isRolling = true;
            canQueueRoll = false;
            playerAnimator.TriggerRoll();
            timeSinceLastRoll = 0f;

            float rollDuration = 1f;
            float rollDistance = 5f;
            float rollSpeed = rollDistance / rollDuration;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                rollDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized * rollSpeed;
            }
            else if (currentVelocity.sqrMagnitude > 0.01f)
            {
                rollDirection = currentVelocity.normalized * rollSpeed;
            }
            else
            {
                rollDirection = transform.forward * rollSpeed;
            }

            rollEndTime = Time.time + rollDuration;
        }

        public void EndRoll()
        {
            isRolling = false;
            currentVelocity = Vector3.zero;
            rollDirection = Vector3.zero;
            canQueueRoll = true;
        }*/

        public Vector3 GetFacingDirection()
        {
            return transform.forward;
        }

    }
}
