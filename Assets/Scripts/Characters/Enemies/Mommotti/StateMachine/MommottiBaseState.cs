

namespace ProjectColombo.StateMachine.Mommotti
{
    public abstract class MommottiBaseState : State
    {
        protected MommottiStateMachine stateMachine;

        public MommottiBaseState(MommottiStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }
}