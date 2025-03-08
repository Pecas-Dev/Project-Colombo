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
            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, m_playerStateMachine.Angle, 0));

            groundMask = LayerMask.GetMask("Ground", "Default");
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Roll);
            m_playerStateMachine.PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            SetIgnoreLayers();

            if (!CanQueueRoll)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
                return;
            }

            if (!m_playerStateMachine.StaminaSystem.TryConsumeStamina(m_playerStateMachine.StaminaSystem.StaminaConfig.RollStaminaCost))
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
                return;
            }

            m_playerStateMachine.GameInputSO.DisableAllInputs();
            m_playerStateMachine.PlayerAnimatorScript.TriggerRoll();

            rollSpeed = rollDistance / rollDuration;

            Vector2 movementInput = m_playerStateMachine.GameInputSO.MovementInput;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 inputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

                rollDirection = TransformDirectionToIsometric(inputDirection) * rollSpeed;
            }
            else
            {
                rollDirection = m_playerStateMachine.PlayerRigidbody.transform.forward * rollSpeed;
            }

            ApplyRollImpulse();

            CanQueueRoll = false;
        }

        public override void Tick(float deltaTime)
        {
            KeepPlayerGrounded();

            m_playerStateMachine.PlayerRigidbody.angularVelocity = Vector3.zero;
            Vector3 velocity = m_playerStateMachine.PlayerRigidbody.linearVelocity;

            velocity.x = rollDirection.x * rollSpeed;
            velocity.z = rollDirection.z * rollSpeed;

            velocity.y = Mathf.Min(velocity.y, 0);

            m_playerStateMachine.PlayerRigidbody.linearVelocity = velocity;

            //HandleAirPhysicsIfNeeded(deltaTime);

            if (!m_playerStateMachine.PlayerAnimatorScript.IsInRoll)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInputSO.EnableAllInputs();
            m_playerStateMachine.RollInvincibleFrameStop();
            m_playerStateMachine.PlayerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            ResetIgnoreLayers();

            if (m_playerStateMachine.GameInputSO.MovementInput.sqrMagnitude < 0.01f)
            {
                Vector3 zeroVelocity = m_playerStateMachine.PlayerRigidbody.linearVelocity;

                zeroVelocity.x = 0;
                zeroVelocity.z = 0;

                m_playerStateMachine.PlayerRigidbody.linearVelocity = zeroVelocity;
            }

            m_playerStateMachine.StartCoroutine(RollCooldown());
        }

        void ApplyRollImpulse()
        {
            float impulseStrength = 0.03f;

            m_playerStateMachine.PlayerRigidbody.AddForce(rollDirection * impulseStrength, ForceMode.Impulse);
        }

        void KeepPlayerGrounded()
        {
            RaycastHit hit;

            Vector3 rayStart = m_playerStateMachine.PlayerRigidbody.position + Vector3.up * 0.1f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundMask))
            {
                float downwardForce = 20f;
                m_playerStateMachine.PlayerRigidbody.AddForce(Vector3.down * downwardForce, ForceMode.Force);
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
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, true);
        }

        public void ResetIgnoreLayers()
        {
            // Disable collision between the Player and enemy/weapon layers
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, false);
        }
    }
}