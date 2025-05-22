using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateAttack : PulcinellaBaseState
    {
        int attackMode;

        public PulcinellaStateAttack(PulcinellaStateMachine stateMachine, int attackMode) : base(stateMachine)
        {
            stateMachine.SetCurrentState(PulcinellaStateMachine.PulcinellaState.ATTACK);
            this.attackMode = attackMode;
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