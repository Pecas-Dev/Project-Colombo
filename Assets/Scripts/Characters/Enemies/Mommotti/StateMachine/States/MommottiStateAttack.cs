using ProjectColombo.Combat;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAttack : MommottiBaseState
    {
        Vector3 targetDirection;
        float attackCheckTimer = 0;
        float intervallToCheckAttack = 1f;
        bool doHeavyAttack = false;
        bool decided = false;

        public MommottiStateAttack(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ATTACK);
            stateMachine.canAttack = false;
        }

        public override void Tick(float deltaTime)
        {
            attackCheckTimer += deltaTime;

            if (attackCheckTimer >= intervallToCheckAttack)
            {
                attackCheckTimer = 0;
                CheckIfShouldStillAttack();
            }

            

            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            float distanceToPlayer = targetDirection.magnitude;

            //decide if heavy or light attack once
            if (stateMachine.canAttack && !decided)
            {
                if (distanceToPlayer > 0.75 * stateMachine.myMommottiAttributes.circleDistance) //this could be adjusted to a heavy attack range
                {
                    // Player is further away, use heavy attack
                    doHeavyAttack = true;
                }
                else
                {
                    // Player is close enough, use light attack
                    doHeavyAttack = false;
                }

                decided = true;
            }

            if (stateMachine.canAttack && !stateMachine.myWeaponAttributes.onCooldown && distanceToPlayer < stateMachine.myWeaponAttributes.reach)
            {
                if (doHeavyAttack)
                {
                    HeavyAttack();
                }
                else
                {
                    LightAttack();
                }

                decided = false;
            }

            //rotate towards player
            if (Vector3.Angle(stateMachine.transform.forward, targetDirection.normalized) > 1f)
            {
                Quaternion startRotation = stateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            //move to player if to far away
            if (stateMachine.canAttack && distanceToPlayer > stateMachine.myWeaponAttributes.reach)
            {
                float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
                Vector3 movingDirection = stateMachine.transform.forward;

                stateMachine.myRigidbody.MovePosition((stateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
            }

            //step back if cannot attack
            if (!stateMachine.canAttack && distanceToPlayer < 0.75 * stateMachine.myMommottiAttributes.circleDistance && !stateMachine.myWeaponAttributes.isAttacking)
            {
                float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
                Vector3 movingDirection = -stateMachine.transform.forward;

                stateMachine.myRigidbody.MovePosition((stateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
            }
        }

        public override void Exit()
        {
        }

        private void LightAttack()
        {
            Debug.Log("light attack");
            stateMachine.myWeaponAttributes.heavyAttack = false; //no impact on light attack
            stateMachine.myAnimator.SetTrigger("Attack");
            stateMachine.myWeaponAttributes.onCooldown = true;
            stateMachine.myWeaponAttributes.isAttacking = true;
            stateMachine.canAttack = false;
        }

        private void HeavyAttack()
        {
            Debug.Log("heavy attack");
            stateMachine.myWeaponAttributes.heavyAttack = true; //deal impact on hard attack
            stateMachine.myAnimator.SetTrigger("Attack");
            stateMachine.myWeaponAttributes.onCooldown = true;
            stateMachine.myWeaponAttributes.isAttacking = true;
            stateMachine.canAttack = false;
        }

        private void CheckIfShouldStillAttack()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float currentDistanceToPlayer = (stateMachine.myMommottiAttributes.playerPosition.position - stateMachine.transform.position).magnitude;

            if (currentDistanceToPlayer > stateMachine.myMommottiAttributes.circleDistance + 5f)
            {
                //switch to chasing
                stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                return;
            }


            foreach (GameObject m in enemies)
            {
                MommottiAttributes otherAttributes = m.GetComponent<MommottiAttributes>();

                //check other is mommotti and attacking
                if (otherAttributes == null) continue;

                //check if there is an enemy currently able to attack
                if (m.GetComponent<MommottiStateMachine>().currentState == MommottiStateMachine.MommottiState.ATTACK)
                {
                    if (!m.GetComponent<MommottiStateMachine>().canAttack && !stateMachine.myWeaponAttributes.onCooldown)
                    {
                        stateMachine.canAttack = true;
                    }

                    continue;
                }

                //check if they are closer to player
                if (currentDistanceToPlayer >= (otherAttributes.playerPosition.position - m.transform.position).magnitude)
                {
                    //switch to chasing
                    stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                }
            }
        }
    }
}