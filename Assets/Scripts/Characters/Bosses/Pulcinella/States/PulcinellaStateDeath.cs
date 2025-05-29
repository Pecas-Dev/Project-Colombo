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
            stateMachine.myAnimator.Play("Death");
            stateMachine.GetComponent<Collider>().enabled = false;
            stateMachine.tag = "Untagged";
            CustomEvents.EnemyDied(GameGlobals.MusicScale.MAJOR,stateMachine.gameObject);
            CustomEvents.EndBossfight();
            //stateMachine.myEntityAttributes.Destroy();
        }

        public override void Tick(float deltaTime)
        {
         
        }

        public override void Exit()
        {

        }
    }
}