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
        float previousFrameTime;
        Matrix4x4 isometricMatrix;

        public PlayerAttackState(PlayerStateMachine playerStateMachine, int attackIndex) : base(playerStateMachine)
        {
            attack = playerStateMachine.Attacks[attackIndex];


            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, m_playerStateMachine.Angle, 0));
        }

        public override void Enter()
        {
            if (!m_playerStateMachine.StaminaSystem.TryConsumeStamina(m_playerStateMachine.StaminaSystem.StaminaConfig.ComboStaminaCost))
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
                return;
            }

            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            // We also need to add a "Hold Animation at the End" if button is still pressed.

            m_playerStateMachine.GameInputSO.DisableAllInputsExcept(InputActionType.Roll, InputActionType.Parry, InputActionType.Attack);

            m_playerStateMachine.PlayerAnimatorScript.PlayAttackAnimation(attack.AnimationName, attack.TransitionDuration);

            Vector3 zeroVelocity = m_playerStateMachine.PlayerRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            m_playerStateMachine.PlayerRigidbody.linearVelocity = zeroVelocity;

            var targeter = m_playerStateMachine.Targeter;

            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                //FaceLockedTargetInstant();
            }

            ApplyAttackImpulse();
        }

        public override void Tick(float deltaTime)
        {
            if (m_playerStateMachine.GameInputSO.RollPressed)
            {
                m_playerStateMachine.GameInputSO.ResetRollPressed();
                m_playerStateMachine.SwitchState(new PlayerRollState(m_playerStateMachine));

                return;
            }

            if (m_playerStateMachine.GameInputSO.ParryPressed)
            {
                m_playerStateMachine.GameInputSO.ResetParryPressed();
                m_playerStateMachine.SwitchState(new PlayerParryState(m_playerStateMachine));

                return;
            }

            float normalizedTime = GetNormalizedTime();

            if (normalizedTime >= previousFrameTime && normalizedTime < 1.0f)
            {
                if (m_playerStateMachine.GameInputSO.AttackPressed)
                {
                    ComboAttack(normalizedTime);
                }
            }
            else
            {
                if (attack.ComboStateIndex == -1)
                {
                    m_playerStateMachine.StaminaSystem.TryConsumeStamina(m_playerStateMachine.StaminaSystem.StaminaConfig.ComboStaminaCost);
                }

                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }

            previousFrameTime = normalizedTime;

            //HandleAirPhysicsIfNeeded(deltaTime);

            FaceLockedTarget(deltaTime);
            ApplyAttackImpulse();
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInputSO.EnableAllInputs();
        }

        void ComboAttack(float normalizedTime)
        {
            if (attack.ComboStateIndex == -1)
            {
                m_playerStateMachine.GameInputSO.ResetAttackPressed();
                return;
            }
            if (normalizedTime < attack.ComboAttackTime)
            {
                m_playerStateMachine.GameInputSO.ResetAttackPressed();
                return;
            }

            m_playerStateMachine.GameInputSO.ResetAttackPressed();
            m_playerStateMachine.SwitchState(new PlayerAttackState(m_playerStateMachine, attack.ComboStateIndex));
        }

        private float GetNormalizedTime()
        {
            AnimatorStateInfo currentInfo = m_playerStateMachine.PlayerAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextInfo = m_playerStateMachine.PlayerAnimator.GetNextAnimatorStateInfo(0);

            if (m_playerStateMachine.PlayerAnimator.IsInTransition(0) && nextInfo.IsTag("Attack"))
            {
                return nextInfo.normalizedTime;
            }
            else if (!m_playerStateMachine.PlayerAnimator.IsInTransition(0) && currentInfo.IsTag("Attack"))
            {
                return currentInfo.normalizedTime;
            }
            else
            {
                return 0.0f;
            }
        }

        void FaceLockedTarget(float deltaTime)
        {
            var targeter = m_playerStateMachine.Targeter;

            if (targeter == null || !targeter.isTargetingActive || targeter.currentTarget == null)
            {
                return;
            }

            Vector3 toTarget = targeter.currentTarget.transform.position - m_playerStateMachine.PlayerRigidbody.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            toTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);

            m_playerStateMachine.PlayerRigidbody.rotation = Quaternion.RotateTowards(m_playerStateMachine.PlayerRigidbody.rotation, targetRotation, m_playerStateMachine.EntityAttributes.rotationSpeedPlayer * deltaTime);
        }

        void FaceLockedTargetInstant()
        {
            var targeter = m_playerStateMachine.Targeter;

            if (targeter == null || !targeter.isTargetingActive || targeter.currentTarget == null)
            {
                return;
            }

            Vector3 toTarget = targeter.currentTarget.transform.position - m_playerStateMachine.PlayerRigidbody.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            toTarget.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            m_playerStateMachine.PlayerRigidbody.rotation = targetRotation;
        }

        Vector3 LookForClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                return Vector3.forward;
            }

            Vector3 myPosition = m_playerStateMachine.transform.position;
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
            var targeter = m_playerStateMachine.Targeter;

            // These could come from weapon attributes
            WeaponAttributes playerWeapon = m_playerStateMachine.GetComponentInChildren<WeaponAttributes>();
            float activationDistance = playerWeapon.distanceToActivateForwardImpulse;   // player should be close already
            float maxDistance = playerWeapon.maxDistanceAfterImpulse;                   // furthest away
            float minDistance = playerWeapon.minDistanceAfterImpulse;                   // closest

            // Check if the player is targeting an enemy
            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                targetPosition = targeter.currentTarget.transform.position;
                targetPosition.y = m_playerStateMachine.PlayerRigidbody.position.y;
            }
            else
            {
               targetPosition = LookForClosestEnemy();
               targetPosition.y = m_playerStateMachine.PlayerRigidbody.position.y;
               //return; // If no target, do nothing
            }

            //turn to target
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - m_playerStateMachine.transform.position);
            m_playerStateMachine.PlayerRigidbody.rotation = targetRotation;

            // Calculate the direction to the target
            Vector3 directionToTarget = targetPosition - m_playerStateMachine.PlayerRigidbody.position;
            directionToTarget.y = 0f;

            float distanceToTarget = directionToTarget.magnitude;

            // If the player is too far from the target, do nothing
            if (distanceToTarget > activationDistance) return;

            // Move towards the target if the distance is greater than the desired target distance
            if (distanceToTarget > maxDistance)
            {
                directionToTarget.Normalize();

                float speed = m_playerStateMachine.EntityAttributes.attackImpulseForce;
                m_playerStateMachine.PlayerRigidbody.MovePosition(m_playerStateMachine.PlayerRigidbody.position + directionToTarget * speed * Time.deltaTime);
            }

            // Move away from the target if the player is too close
            else if (distanceToTarget < minDistance)
            {
                Vector3 directionAwayFromTarget = -directionToTarget;
                directionAwayFromTarget.Normalize();

                float retreatSpeed = m_playerStateMachine.EntityAttributes.attackImpulseForce;
                m_playerStateMachine.PlayerRigidbody.MovePosition(m_playerStateMachine.PlayerRigidbody.position + directionAwayFromTarget * retreatSpeed * Time.deltaTime);
            }
        }

        private Vector3 TransformDirectionToIsometric(Vector3 direction)
        {
            return isometricMatrix.MultiplyVector(direction).normalized;
        }
    }
}