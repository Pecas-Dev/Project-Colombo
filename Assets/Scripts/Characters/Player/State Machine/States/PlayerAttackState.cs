using ProjectColombo.GameInputSystem;
using ProjectColombo.Combat;

using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerAttackState : PlayerBaseState
    {
        Attack attack;
        Matrix4x4 isometricMatrix;

        public PlayerAttackState(PlayerStateMachine playerStateMachine, int attackIndex) : base(playerStateMachine)
        {
            attack = playerStateMachine.attacks[attackIndex];


            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, stateMachine.Angle, 0));
        }

        public override void Enter()
        {
            if (!stateMachine.myStamina.TryConsumeStamina(stateMachine.myStamina.StaminaConfig.ComboStaminaCost))
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }

            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            // We also need to add a "Hold Animation at the End" if button is still pressed.

            stateMachine.gameInputSO.DisableAllInputsExcept(InputActionType.Roll, InputActionType.MajorParry, InputActionType.MajorAttack);

            stateMachine.myPlayerAnimator.PlayAttackAnimation(attack.AnimationName, attack.TransitionDuration);

            Vector3 zeroVelocity = stateMachine.myRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            stateMachine.myRigidbody.linearVelocity = zeroVelocity;

            var targeter = stateMachine.myTargeter;

            ApplyAttackImpulse();
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.gameInputSO.RollPressed)
            {
                stateMachine.gameInputSO.ResetRollPressed();
                stateMachine.SwitchState(new PlayerRollState(stateMachine));

                return;
            }

            if (stateMachine.gameInputSO.MajorParryPressed)
            {
                stateMachine.gameInputSO.ResetMajorParryPressed();
                stateMachine.SwitchState(new PlayerParryState(stateMachine));

                return;
            }

            if (stateMachine.myPlayerAnimator.FinishedAttack())
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
            }

            //TODO: new combo logic
 

            FaceLockedTarget(deltaTime);
            ApplyAttackImpulse();
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
        }

        void ComboAttack(float normalizedTime)
        {
            if (attack.ComboStateIndex == -1)
            {
                stateMachine.gameInputSO.ResetMajorAttackPressed();
                return;
            }
            if (normalizedTime < attack.ComboAttackTime)
            {
                stateMachine.gameInputSO.ResetMajorAttackPressed();
                return;
            }

            stateMachine.gameInputSO.ResetMajorAttackPressed();
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, attack.ComboStateIndex));
        }

        void FaceLockedTarget(float deltaTime)
        {
            var targeter = stateMachine.myTargeter;

            if (targeter == null || !targeter.isTargetingActive || targeter.currentTarget == null)
            {
                return;
            }

            Vector3 toTarget = targeter.currentTarget.transform.position - stateMachine.myRigidbody.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            toTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            stateMachine.myRigidbody.rotation = Quaternion.RotateTowards(stateMachine.myRigidbody.rotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime);
        }

        void FaceLockedTargetInstant()
        {
            var targeter = stateMachine.myTargeter;

            if (targeter == null || !targeter.isTargetingActive || targeter.currentTarget == null)
            {
                return;
            }

            Vector3 toTarget = targeter.currentTarget.transform.position - stateMachine.myRigidbody.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            toTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            stateMachine.myRigidbody.rotation = targetRotation;
        }

        Vector3 LookForClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                return Vector3.forward;
            }

            Vector3 myPosition = stateMachine.transform.position;
            Vector3 closestPosition = myPosition;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject e in enemies)
            {
                if ((e.transform.position - myPosition).magnitude < closestDistance)
                {
                    closestPosition = e.transform.position;
                    closestDistance = (e.transform.position - myPosition).magnitude;
                }
            }

            return closestPosition;
        }

        void ApplyAttackImpulse()
        {
            Vector3 targetPosition;
            var targeter = stateMachine.myTargeter;

            // These could come from weapon attributes
            WeaponAttributes playerWeapon = stateMachine.GetComponentInChildren<WeaponAttributes>();
            float activationDistance = playerWeapon.distanceToActivateForwardImpulse;   // player should be close already
            float maxDistance = playerWeapon.maxDistanceAfterImpulse;                   // furthest away
            float minDistance = playerWeapon.minDistanceAfterImpulse;                   // closest

            // Check if the player is targeting an enemy
            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                targetPosition = targeter.currentTarget.transform.position;
                targetPosition.y = stateMachine.myRigidbody.position.y;
            }
            else
            {
               targetPosition = LookForClosestEnemy();
               targetPosition.y = stateMachine.myRigidbody.position.y;
               //return; // If no target, do nothing
            }

            //turn to target
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - stateMachine.transform.position);
            stateMachine.myRigidbody.rotation = targetRotation;

            // Calculate the direction to the target
            Vector3 directionToTarget = targetPosition - stateMachine.myRigidbody.position;
            directionToTarget.y = 0f;

            float distanceToTarget = directionToTarget.magnitude;

            // If the player is too far from the target, do nothing
            if (distanceToTarget > activationDistance) return;

            // Move towards the target if the distance is greater than the desired target distance
            if (distanceToTarget > maxDistance)
            {
                directionToTarget.Normalize();

                float speed = stateMachine.myEntityAttributes.attackImpulseForce;
                stateMachine.myRigidbody.MovePosition(stateMachine.myRigidbody.position + directionToTarget * speed * Time.deltaTime);
            }

            // Move away from the target if the player is too close
            else if (distanceToTarget < minDistance)
            {
                Vector3 directionAwayFromTarget = -directionToTarget;
                directionAwayFromTarget.Normalize();

                float retreatSpeed = stateMachine.myEntityAttributes.attackImpulseForce;
                stateMachine.myRigidbody.MovePosition(stateMachine.myRigidbody.position + directionAwayFromTarget * retreatSpeed * Time.deltaTime);
            }
        }

        private Vector3 TransformDirectionToIsometric(Vector3 direction)
        {
            return isometricMatrix.MultiplyVector(direction).normalized;
        }
    }
}