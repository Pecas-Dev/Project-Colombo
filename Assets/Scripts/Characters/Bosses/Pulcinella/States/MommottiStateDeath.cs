using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateDeath : PulcinellaBaseState
    {
        public PulcinellaStateDeath(PulcinellaStateMachine stateMachine) : base(stateMachine)
        {
            stateMachine.SetCurrentState(PulcinellaStateMachine.PulcinellaState.DEAD);
        }

        public override void Enter()
        {

        }

        public override void Tick(float deltaTime)
        {
         
        }

        public override void Exit()
        {

        }
    }
}