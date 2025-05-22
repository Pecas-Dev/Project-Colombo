using NUnit.Framework.Constraints;
using ProjectColombo.Combat;
using System.Collections;
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
            }
        }

        public override void Tick(float deltaTime)
        {
            if (attackMode == 2)
            {
                currentDistance = Vector3.Distance(stateMachine.playerRef.transform.position, stateMachine.transform.position);

                if (currentDistance > stateMachine.myPulcinellaAttributes.targetDistance)
                {
                    MoveToTarget(stateMachine.playerRef.transform.position, deltaTime, stateMachine.myEntityAttributes.moveSpeed);
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