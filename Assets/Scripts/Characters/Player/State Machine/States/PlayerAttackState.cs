using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerAttackState : PlayerBaseState
    {
        float attackDuration = 0.6875f;
        float attackEndTime;

        public PlayerAttackState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            Debug.Log("Entered Attack State");

            m_playerStateMachine.GameInput.DisableAllInputsExceptRoll();

            m_playerStateMachine.PlayerAnimator.TriggerAttack();

            attackEndTime = Time.time + attackDuration;
        }

        public override void Tick(float deltaTime)
        {
            if (m_playerStateMachine.GameInput.RollPressed)
            {
                m_playerStateMachine.GameInput.ResetRollPressed();
                m_playerStateMachine.SwitchState(new PlayerRollState(m_playerStateMachine));

                return;
            }

            if (Time.time >= attackEndTime)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }
        }

        public override void Exit()
        {
            Debug.Log("Exited Attack State");

            m_playerStateMachine.GameInput.EnableInputs();
        }
    }
}