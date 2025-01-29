using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerParryState : PlayerBaseState
    {
        public PlayerParryState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Parry);
            Debug.Log("Entered Parry State");

            m_playerStateMachine.PlayerAnimatorScript.TriggerParry();
        }

        public override void Tick(float deltaTime)
        {
            if(!m_playerStateMachine.PlayerAnimatorScript.IsInParry)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }
        }

        public override void Exit()
        {
            Debug.Log("Exited Parry State");
        }
    }
}
