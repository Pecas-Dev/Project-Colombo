namespace ProjectColombo.StateMachine.Player
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine m_playerStateMachine;

        public PlayerBaseState(PlayerStateMachine playerStateMachine)
        {
            this.m_playerStateMachine = playerStateMachine;
        }
    }
}

