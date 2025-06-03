using System.Collections.Generic;
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

            // Estimate speed from distance to target (or use your own velocity logic if available)
            float speed = 0f;

            if (currentDistance > stateMachine.myPulcinellaAttributes.targetDistance)
            {
                speed = stateMachine.myEntityAttributes.moveSpeed;
                MoveToTarget(stateMachine.playerRef.transform.position, deltaTime, stateMachine.myEntityAttributes.moveSpeed);
            }

            if (currentDistance < stateMachine.myPulcinellaAttributes.targetDistance + 1f)
            {
                speed = stateMachine.myEntityAttributes.moveSpeed / 2f;
                Vector3 directionAway = (stateMachine.transform.position - stateMachine.playerRef.transform.position).normalized;
                Vector3 target = stateMachine.transform.position + directionAway;
                MoveToTarget(target, deltaTime, speed);

            }

            stateMachine.myAnimator.SetFloat("Speed", speed);

            RotateTowardsTarget(stateMachine.playerRef.position, deltaTime, 120f);

            if (timer >= currentAttackTimer)
            {
                if (currentDistance <= stateMachine.myPulcinellaAttributes.distanceToSlash)
                {
                    PerformAttack(0); // Exception attack
                    return;
                }

                int nextAttack = -1;

                if (currentDistance <= stateMachine.myPulcinellaAttributes.distanceToRageImpact)
                {
                    nextAttack = 1;
                }
                else
                {
                    int rand = Random.Range(0, 101);
                    nextAttack = (rand < stateMachine.myPulcinellaAttributes.chanceToLeap) ? 2 : 3;
                }

                // Check if same attack has been used twice already
                if (nextAttack != 0 && nextAttack == stateMachine.lastAttack && stateMachine.consecutiveAttackCount >= 2)
                {
                    // Force a different attack
                    nextAttack = GetDifferentAttack(nextAttack);
                }

                PerformAttack(nextAttack);
            }
        }

        public override void Exit()
        {
            timer = 0;
        }

        void PerformAttack(int attackIndex)
        {
            if (attackIndex == stateMachine.lastAttack)
            {
                stateMachine.consecutiveAttackCount++;
            }
            else
            {
                stateMachine.consecutiveAttackCount = 1;
                stateMachine.lastAttack = attackIndex;
            }

            stateMachine.SwitchState(new PulcinellaStateAttack(stateMachine, attackIndex));
        }

        int GetDifferentAttack(int last)
        {
            List<int> options = new List<int> { 0, 1, 2, 3 };
            options.Remove(last);
            return options[Random.Range(0, options.Count)];
        }

    }
}