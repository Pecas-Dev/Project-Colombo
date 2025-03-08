using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerStaggerState : PlayerBaseState
    {
        float timer;


        public PlayerStaggerState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            m_playerStateMachine.PlayerAnimator.SetBool("Impact", true);
            m_playerStateMachine.GameInputSO.DisableAllInputs();
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            //HandleAirPhysicsIfNeeded(deltaTime);

            if (timer >= m_playerStateMachine.EntityAttributes.stunnedTime)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }
        }

        public override void Exit()
        {
            m_playerStateMachine.PlayerAnimator.SetBool("Impact", false);
            m_playerStateMachine.GameInputSO.EnableAllInputs();
        }
    }
}