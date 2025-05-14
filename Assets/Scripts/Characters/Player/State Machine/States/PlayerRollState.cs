using ProjectColombo.GameInputSystem;
using System.Collections;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerRollState : PlayerBaseState
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        int weaponLayer = LayerMask.NameToLayer("Weapon");

        float rollSpeed;

        float rollDistance = 1.8f;
        float rollDuration = 0.6f;
        float rollCooldown = 0.125f;

        Vector3 rollDirection;
        Matrix4x4 isometricMatrix;

        private float groundCheckDistance = 0.1f;
        private LayerMask groundMask;

        public static bool CanQueueRoll = true;


        public PlayerRollState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, stateMachine.Angle, 0));

            groundMask = LayerMask.GetMask("Ground", "Default");
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Roll);
            stateMachine.myRigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            SetIgnoreLayers();

            if (!CanQueueRoll)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            if (!stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.staminaToRoll))
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            stateMachine.gameInputSO.DisableAllInputsExcept(InputActionType.Pause);
            stateMachine.myPlayerAnimator.TriggerRoll();

            rollSpeed = rollDistance / rollDuration;

            Vector2 movementInput = stateMachine.gameInputSO.MovementInput;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 inputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

                rollDirection = TransformDirectionToIsometric(inputDirection) * rollSpeed;
            }
            else
            {
                rollDirection = stateMachine.myRigidbody.transform.forward * rollSpeed;
            }

            ApplyRollImpulse();

            CanQueueRoll = false;
        }

        public override void Tick(float deltaTime)
        {
            KeepPlayerGrounded();

            stateMachine.myRigidbody.angularVelocity = Vector3.zero;
            Vector3 velocity = stateMachine.myRigidbody.linearVelocity;

            velocity.x = rollDirection.x * rollSpeed;
            velocity.z = rollDirection.z * rollSpeed;

            velocity.y = Mathf.Min(velocity.y, 0);

            stateMachine.myRigidbody.linearVelocity = velocity;


            if (!stateMachine.myPlayerAnimator.IsInRoll)
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.RollInvincibleFrameStop();
            stateMachine.myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            ResetIgnoreLayers();

            if (stateMachine.gameInputSO.MovementInput.sqrMagnitude < 0.01f)
            {
                Vector3 zeroVelocity = stateMachine.myRigidbody.linearVelocity;

                zeroVelocity.x = 0;
                zeroVelocity.z = 0;

                stateMachine.myRigidbody.linearVelocity = zeroVelocity;
            }

            stateMachine.StartCoroutine(RollCooldown());
        }

        void ApplyRollImpulse()
        {
            float impulseStrength = 0.03f;

            stateMachine.myRigidbody.AddForce(rollDirection * impulseStrength, ForceMode.Impulse);
        }

        void KeepPlayerGrounded()
        {
            RaycastHit hit;

            Vector3 rayStart = stateMachine.myRigidbody.position + Vector3.up * 0.1f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundMask))
            {
                float downwardForce = 20f;
                stateMachine.myRigidbody.AddForce(Vector3.down * downwardForce, ForceMode.Force);
            }
        }

        private Vector3 TransformDirectionToIsometric(Vector3 direction)
        {
            return isometricMatrix.MultiplyVector(direction).normalized;
        }

        IEnumerator RollCooldown()
        {
            yield return new WaitForSeconds(rollCooldown);
            CanQueueRoll = true;
        }

        public void SetIgnoreLayers()
        {
            // Disable collision between the Player and enemy/weapon layers
            //Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, true);
        }

        public void ResetIgnoreLayers()
        {
            // Disable collision between the Player and enemy/weapon layers
            //Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, false);
        }
    }
}