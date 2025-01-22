using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateIdle : MommottiBaseState
    {
        public MommottiStateIdle(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.PATROL);
            Debug.Log("Mommotti entered xxxxxxxxxxxx State");
        }

        public override void Tick(float deltaTime)
        {

        }

        public override void Exit()
        {
        }
    }
}