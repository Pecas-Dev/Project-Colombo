

namespace ProjectColombo.StateMachine.Mommotti
{
    public abstract class MommottiBaseState : State
    {
        protected MommottiStateMachine m_StateMachine;

        public MommottiBaseState(MommottiStateMachine stateMachine)
        {
            m_StateMachine = stateMachine;
        }
    }
}