using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public class PulcinellaStateMovement : PulcinellaBaseState
    {
        float currentDistance;
        float currentAttackTimer;

        public PulcinellaStateMovement(PulcinellaStateMachine stateMachine) : base(stateMachine)
        {
            stateMachine.SetCurrentState(PulcinellaStateMachine.PulcinellaState.MOVE);
        }

        public override void Enter()
        {
            currentAttackTimer = Random.Range(stateMachine.myPulcinellaAttributes.minTimeBetweenAttacks, stateMachine.myPulcinellaAttributes.maxTimeBetweenAttacks);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            currentDistance = Vector3.Distance(stateMachine.playerRef.transform.position, stateMachine.transform.position);

            if (currentDistance > stateMachine.myPulcinellaAttributes.targetDistance)
            {
                MoveToTarget(stateMachine.playerRef.transform.position, deltaTime, stateMachine.myEntityAttributes.moveSpeed);
            }

            RotateTowardsTarget(stateMachine.playerRef.position, deltaTime, 120f);

            if (timer >= currentAttackTimer)
            {
                if (currentDistance <= stateMachine.myPulcinellaAttributes.distanceToSlash)
                {
                    stateMachine.SwitchState(new PulcinellaStateAttack(stateMachine, 0));
                }
                else if (currentDistance <= stateMachine.myPulcinellaAttributes.distanceToRageImpact)
                {
                    stateMachine.SwitchState(new PulcinellaStateAttack(stateMachine, 1));
                }
                else
                {
                    int rand = Random.Range(0, 101);

                    if (rand < stateMachine.myPulcinellaAttributes.chanceToLeap)
                    {
                        stateMachine.SwitchState(new PulcinellaStateAttack(stateMachine, 2));
                    }
                    else
                    {
                        stateMachine.SwitchState(new PulcinellaStateAttack(stateMachine, 3));
                    }
                }
            }
        }

        public override void Exit()
        {

        }
    }
}