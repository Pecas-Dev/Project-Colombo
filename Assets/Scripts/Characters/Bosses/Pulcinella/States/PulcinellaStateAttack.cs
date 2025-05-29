using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateAttack : PulcinellaBaseState
    {
        int attackMode;
        float currentDistance;

        public PulcinellaStateAttack(PulcinellaStateMachine stateMachine, int attackMode) : base(stateMachine)
        {
            stateMachine.SetCurrentState(PulcinellaStateMachine.PulcinellaState.ATTACK);
            this.attackMode = attackMode;
        }

        public override void Enter()
        {
            currentDistance = Vector3.Distance(stateMachine.playerRef.transform.position, stateMachine.transform.position);

            switch (attackMode)
            {
                case 0:
                    stateMachine.myAnimator.Play("CursedSlash");
                    break;
                case 1:
                    stateMachine.myAnimator.Play("RagefullImpact");
                    break;
                case 2:
                    stateMachine.myAnimator.Play("CursedLeap");
                    break;
                case 3:
                    stateMachine.myAnimator.Play("CursedNote");
                    break;
            }
        }

        public override void Tick(float deltaTime)
        {
            if (attackMode == 2 && stateMachine.inAir)
            {
                currentDistance = Vector3.Distance(stateMachine.playerRef.transform.position, stateMachine.transform.position);

                if (currentDistance > stateMachine.myPulcinellaAttributes.targetDistance)
                {
                    MoveToTarget(stateMachine.playerRef.transform.position, deltaTime, stateMachine.myEntityAttributes.moveSpeed * 5);
                }

                RotateTowardsTarget(stateMachine.playerRef.position, deltaTime, 120f);
            }
        }

        public override void Exit()
        {
            stateMachine.DisableLeftHand();
        }
    }
}