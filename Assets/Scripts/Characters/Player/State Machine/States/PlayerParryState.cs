using ProjectColombo.GameInputSystem;


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

            m_playerStateMachine.GameInputSO.DisableAllInputsExcept(InputActionType.Roll);
            m_playerStateMachine.PlayerAnimatorScript.TriggerParry();
        }

        public override void Tick(float deltaTime)
        {
            if(!m_playerStateMachine.PlayerAnimatorScript.IsInParry)
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }

            ApplyAirPhysics(deltaTime);
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInputSO.EnableAllInputs();
            m_playerStateMachine.ParryFrameStop();
        }
    }
}
