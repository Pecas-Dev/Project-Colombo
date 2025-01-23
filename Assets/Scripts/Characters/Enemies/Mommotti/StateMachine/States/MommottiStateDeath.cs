using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateDeath : MommottiBaseState
    {
        public MommottiStateDeath(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.DEAD);
            Debug.Log("Mommotti entered Death State");
        }

        public override void Tick(float deltaTime)
        {

        }

        public override void Exit()
        {
        }
    }
}