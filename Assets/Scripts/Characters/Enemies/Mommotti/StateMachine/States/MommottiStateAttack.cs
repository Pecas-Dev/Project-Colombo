using ProjectColombo.Combat;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAttack : MommottiBaseState
    {
        private Vector3 targetDirection;
        private float attackCheckTimer = 0;
        private float intervalToCheckAttack = 2f;

        public MommottiStateAttack(MommottiStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ATTACK);
            stateMachine.canAttack = false;
        }

        public override void Tick(float deltaTime)
        {
            attackCheckTimer += deltaTime;

            if (attackCheckTimer >= intervalToCheckAttack)
            {
                attackCheckTimer = 0;

                // If the player is no longer visible, transition to chasing state
                if (!stateMachine.myMommottiAttributes.FieldOfViewCheck())
                {
                    stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                    return;
                }
            }

            stateMachine.canAttack = !stateMachine.myWeaponAttributes.onCooldown;

            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            targetDirection.y = 0;  // Prevents vertical movements

            float distanceToPlayer = targetDirection.magnitude;

            // Perform attack if in range and can attack
            if (stateMachine.canAttack && !stateMachine.myWeaponAttributes.onCooldown && distanceToPlayer < stateMachine.myWeaponAttributes.reach)
            {
                Attack();
            }

            // Rotate towards the player for the attack
            RotateTowardsTarget(stateMachine.myMommottiAttributes.GetPlayerPosition(), deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);

            // Move closer if too far for the attack
            if (stateMachine.canAttack && distanceToPlayer > stateMachine.myWeaponAttributes.reach)
            {
                MoveTowardsPlayer(deltaTime);
            }

            // Move backwards if cannot attack and too close
            if (!stateMachine.canAttack && distanceToPlayer < 0.25f * stateMachine.myMommottiAttributes.circleDistance && !stateMachine.myWeaponAttributes.isAttacking)
            {
                MoveBackwards(deltaTime);
            }
        }

        public override void Exit()
        {
            stateMachine.myWeaponAttributes.DisableWeaponHitbox();
        }

        private void Attack()
        {
            stateMachine.myAnimator.SetTrigger("Attack");
            stateMachine.myWeaponAttributes.onCooldown = true;
            stateMachine.myWeaponAttributes.isAttacking = true;
            stateMachine.canAttack = false;
            CheckIfShouldStillAttack();
        }

        private void CheckIfShouldStillAttack()
        {
            float currentDistanceToPlayer = (stateMachine.myMommottiAttributes.playerPosition.position - stateMachine.transform.position).magnitude;

            // If the player is far enough away, switch to chase
            if (currentDistanceToPlayer > stateMachine.myMommottiAttributes.circleDistance + 5f)
            {
                stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                return;
            }

            // Check for other enemies attacking and update state accordingly
            foreach (GameObject m in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                MommottiAttributes otherAttributes = m.GetComponent<MommottiAttributes>();
                if (otherAttributes == null) continue;

                MommottiStateMachine otherStateMachine = m.GetComponent<MommottiStateMachine>();

                // Skip enemies that are currently attacking
                if (otherStateMachine.currentState == MommottiStateMachine.MommottiState.ATTACK)
                {
                    if (!otherStateMachine.canAttack && !stateMachine.myWeaponAttributes.onCooldown)
                    {
                        stateMachine.canAttack = true;
                    }
                    continue;
                }

                // If they are closer to the player, switch to chase
                if (currentDistanceToPlayer >= (otherAttributes.playerPosition.position - m.transform.position).magnitude)
                {
                    stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                }
            }
        }

        private void MoveTowardsPlayer(float deltaTime)
        {
            // Move towards the player
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;
            Vector3 targetPosition = stateMachine.transform.position + stateMachine.transform.forward;
            MoveToTarget(targetPosition, deltaTime, currentSpeed);
        }

        private void MoveBackwards(float deltaTime)
        {
            // Move away from the player if too close
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed / 30f; // Slow retreat
            Vector3 targetPosition = stateMachine.transform.position - stateMachine.transform.forward;
            MoveToTarget(targetPosition, deltaTime, currentSpeed);
        }
    }
}
