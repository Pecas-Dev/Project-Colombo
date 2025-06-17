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
                    stateMachine.myAnimator.CrossFadeInFixedTime("CursedSlash", 0.1f);
                    break;
                case 1:
                    stateMachine.myAnimator.CrossFadeInFixedTime("RagefullImpact", 0.1f);
                    break;
                case 2:
                    stateMachine.myAnimator.CrossFadeInFixedTime("CursedLeap", 0.1f);
                    break;
                case 3:
                    stateMachine.myAnimator.CrossFadeInFixedTime("CursedNote", 0.1f);
                    break;
            }
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.canMove)
            {
                currentDistance = Vector3.Distance(stateMachine.playerRef.transform.position, stateMachine.transform.position);

                if (currentDistance > stateMachine.myPulcinellaAttributes.targetDistance)
                {
                    MoveToTarget(stateMachine.playerRef.transform.position, deltaTime, stateMachine.myEntityAttributes.moveSpeed * 10);
                }
            }

            if (stateMachine.canRotate)
            {
                RotateTowardsTarget(stateMachine.playerRef.position, deltaTime, 10f);
            }
        }

        public override void Exit()
        {
            stateMachine.DisableLeftHand();
        }
    }
}