using System.Collections;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerRollState : PlayerBaseState
    {
        float rollSpeed;
        float rollDuration;
        float rollEndTime;
        float rollCooldown = 0.125f;


        Vector3 rollDirection;


        public static bool CanQueueRoll = true;


        public PlayerRollState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

        public override void Enter()
        {
            if (!CanQueueRoll)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
                return;
            }

            Debug.Log("Entered Roll State");

            m_playerStateMachine.GameInput.DisableInputs();
            m_playerStateMachine.PlayerAnimator.TriggerRoll();

            rollDuration = 0.8f;
            float rollDistance = 2.5f;
            rollSpeed = rollDistance / rollDuration;

            Vector2 movementInput = m_playerStateMachine.GameInput.MovementInput;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                rollDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized * rollSpeed;
            }
            else
            {
                rollDirection = m_playerStateMachine.PlayerRigidbody.transform.forward * rollSpeed;
            }

            ApplyRollImpulse();

            rollEndTime = Time.time + rollDuration;

            CanQueueRoll = false;
        }

        public override void Tick(float deltaTime)
        {
            m_playerStateMachine.PlayerRigidbody.MovePosition(m_playerStateMachine.PlayerRigidbody.position + rollDirection * rollSpeed * deltaTime);

            if (Time.time >= rollEndTime)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }
        }

        public override void Exit()
        {
            Debug.Log("Exited Roll State");
            m_playerStateMachine.GameInput.EnableInputs();
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
    }
}
