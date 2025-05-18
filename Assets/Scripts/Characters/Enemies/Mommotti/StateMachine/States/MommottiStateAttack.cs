using ProjectColombo.Combat;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.GameManagement.Events;
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
            // If the player is no longer visible, transition to chasing state
            if (!stateMachine.myMommottiAttributes.FieldOfViewCheck())
            {
                stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                return;
            }


            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ATTACK);
        }

        public override void Tick(float deltaTime)
        {
            attackCheckTimer += deltaTime;

            if (attackCheckTimer >= intervalToCheckAttack)
            {
                attackCheckTimer = 0;

                if (!stateMachine.myMommottiAttributes.FieldOfViewCheck())
                {
                    stateMachine.SwitchState(new MommottiStateChase(stateMachine));
                    return;
                }
            }

            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            targetDirection.y = 0;

            float distanceToPlayer = targetDirection.magnitude;

            if (distanceToPlayer < 2)
            {                       
                // If too close to another enemy, spread out directly
                float spreadThreshold = distanceToPlayer;
                if (GetSpreadOutTarget(spreadThreshold, out Vector3 spreadTarget))
                {
                    float moveSpeed = stateMachine.myEntityAttributes.moveSpeed * 0.8f;
                    MoveToTarget(spreadTarget, deltaTime, moveSpeed);
                    RotateTowardsTarget(stateMachine.myMommottiAttributes.GetPlayerPosition(), deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);
                    return;
                }
            }

            // Attack logic
            if (!stateMachine.myWeaponAttributes.onCooldown && distanceToPlayer < stateMachine.myWeaponAttributes.reach)
            {
                Attack();
            }

            // Rotate toward player
            RotateTowardsTarget(stateMachine.myMommottiAttributes.GetPlayerPosition(), deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);

            // Move directly toward player if too far
            if (distanceToPlayer > stateMachine.myWeaponAttributes.reach)
            {
                MoveTowardsPlayer(deltaTime);
            }

            // Move backwards slightly if too close and not attacking
            if (distanceToPlayer < 0.25f * stateMachine.myMommottiAttributes.circleDistance && !stateMachine.myWeaponAttributes.isAttacking)
            {
                MoveBackwards(deltaTime);
            }
        }



        public override void Exit()
        {
            stateMachine.myAnimator.ResetTrigger("Attack");
            stateMachine.myWeaponAttributes.DisableWeaponHitbox();
        }

        private void Attack()
        {
            CustomEvents.EnemyAttacked(stateMachine.gameObject);
            stateMachine.myAnimator.SetTrigger("Attack");
            stateMachine.myWeaponAttributes.onCooldown = true;
            stateMachine.myWeaponAttributes.isAttacking = true;
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