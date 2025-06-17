using UnityEditor;
using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateIdle : PulcinellaBaseState
    {
        public PulcinellaStateIdle(PulcinellaStateMachine stateMachine) : base(stateMachine)
        {
            stateMachine.SetCurrentState(PulcinellaStateMachine.PulcinellaState.IDLE);
        }

        public override void Enter()
        {
            stateMachine.myAnimator.Play("Idle");
            timer = 0;
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            RotateTowardsTarget(stateMachine.playerRef.position, deltaTime, 10f);

            float speed = 0;
            stateMachine.myAnimator.SetFloat("Speed", speed);


            if (timer >= stateMachine.myPulcinellaAttributes.idleTimeAfterAttack)
            {
                stateMachine.SwitchState(new PulcinellaStateMovement(stateMachine));
                return;
            }
        }

        public override void Exit()
        {

        }
    }
}