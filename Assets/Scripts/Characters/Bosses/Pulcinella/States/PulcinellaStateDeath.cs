using ProjectColombo.GameManagement.Events;
using Unity.VisualScripting;
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
            CustomEvents.EnemyDied(GameGlobals.MusicScale.MAJOR,stateMachine.gameObject);
            stateMachine.myEntityAttributes.Destroy();
        }

        public override void Tick(float deltaTime)
        {
         
        }

        public override void Exit()
        {

        }
    }
}