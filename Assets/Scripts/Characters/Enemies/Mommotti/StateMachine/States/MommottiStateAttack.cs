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

        public MommottiStateAttack(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            Color skinColor = new(1, 0, 0);
            stateMachine.myColorfullSkin.material.color = skinColor;

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
            targetDirection.y = 0;
            float distanceToPlayer = targetDirection.magnitude;

            if (stateMachine.canAttack && !stateMachine.myWeaponAttributes.onCooldown && distanceToPlayer < stateMachine.myWeaponAttributes.reach)
            {
                Attack();
            }

            //rotate towards player
            RotateTowardsTarget(stateMachine.myMommottiAttributes.GetPlayerPosition(), deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);


            //move to player if to far away
            if (stateMachine.canAttack && distanceToPlayer > stateMachine.myWeaponAttributes.reach)
            {
                float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
                Vector3 targetPosition = stateMachine.transform.position + stateMachine.transform.forward;

                MoveToTarget(targetPosition, deltaTime, currentSpeed);
            }

            //step back if cannot attack
            if (!stateMachine.canAttack && distanceToPlayer < 0.25 * stateMachine.myMommottiAttributes.circleDistance && !stateMachine.myWeaponAttributes.isAttacking)
            {
                float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
                Vector3 targetPosition = stateMachine.transform.position - stateMachine.transform.forward; ;

                MoveToTarget(targetPosition, deltaTime, currentSpeed);
            }
        }

        public override void Exit()
        {
        }

        private void Attack()
        {
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