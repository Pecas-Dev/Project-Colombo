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

        float rollDistance = 2.5f;  
        float rollDuration = 0.8f;
        float rollCooldown = 0.125f;


        Vector3 rollDirection;


        public static bool CanQueueRoll = true;


        public PlayerRollState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Roll);
            SetInvunerable();

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
                rollDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized * rollSpeed;
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
            m_playerStateMachine.PlayerRigidbody.angularVelocity = Vector3.zero;
            Vector3 velocity = m_playerStateMachine.PlayerRigidbody.linearVelocity;

            velocity.x = rollDirection.x * rollSpeed;
            velocity.z = rollDirection.z * rollSpeed;

            m_playerStateMachine.PlayerRigidbody.linearVelocity = velocity;

            if (!m_playerStateMachine.PlayerAnimatorScript.IsInRoll)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }

            ApplyAirPhysics(deltaTime);
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInputSO.EnableAllInputs();
            SetVunerable();

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
            float impulseStrength = 0.05f;

            m_playerStateMachine.PlayerRigidbody.AddForce(rollDirection * impulseStrength, ForceMode.Impulse);
        }

        IEnumerator RollCooldown()
        {
            yield return new WaitForSeconds(rollCooldown);
            CanQueueRoll = true;
        }

        public void SetInvunerable()
        {
            m_playerStateMachine.isInvunerable = true;

            // Disable collision between the Player and enemy/weapon layers
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, true);
        }

        public void SetVunerable()
        {
            m_playerStateMachine.isInvunerable = false;

            // Disable collision between the Player and enemy/weapon layers
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, weaponLayer, false);
        }
    }
}
