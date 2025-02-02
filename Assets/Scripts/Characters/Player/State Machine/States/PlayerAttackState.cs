using ProjectColombo.Combat;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerAttackState : PlayerBaseState
    {
        Attack attack;


        float previousFrameTime;


        public PlayerAttackState(PlayerStateMachine playerStateMachine, int attackIndex) : base(playerStateMachine)
        {
            attack = playerStateMachine.Attacks[attackIndex];
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Attack);

            // Not All Inputs Should be disabled since we need to enter Combo, and we also need to add so that we can still rotate the player whilst Attacking
            // We also need to add a "Hold Animation at the End" if button is still pressed.

            //m_playerStateMachine.GameInputSO.DisableAllInputsExceptRoll();

            m_playerStateMachine.PlayerAnimatorScript.PlayAttackAnimation(attack.AnimationName, attack.TransitionDuration);

            Vector3 zeroVelocity = m_playerStateMachine.PlayerRigidbody.linearVelocity;

            zeroVelocity.x = 0f;
            zeroVelocity.z = 0f;

            m_playerStateMachine.PlayerRigidbody.linearVelocity = zeroVelocity;

            var targeter = m_playerStateMachine.Targeter;

            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                FaceLockedTargetInstant();
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

            if(m_playerStateMachine.GameInputSO.ParryPressed)
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
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }

            previousFrameTime = normalizedTime;

            FaceLockedTarget(deltaTime);

            ApplyAirPhysics(deltaTime);
        }

        public override void Exit()
        {
            m_playerStateMachine.GameInputSO.EnableInputs();
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


        void ApplyAttackImpulse()
        {
            Vector3 impulseDirection;

            var targeter = m_playerStateMachine.Targeter;

            if (targeter != null && targeter.isTargetingActive && targeter.currentTarget != null)
            {
                impulseDirection = targeter.currentTarget.transform.position - m_playerStateMachine.PlayerRigidbody.position;
                impulseDirection.y = 0f;
                impulseDirection.Normalize();
            }
            else
            {
                impulseDirection = m_playerStateMachine.PlayerRigidbody.transform.forward;
                impulseDirection.y = 0f;
                impulseDirection.Normalize();
            }

            m_playerStateMachine.PlayerRigidbody.AddForce(impulseDirection * m_playerStateMachine.EntityAttributes.attackImpulseForce, ForceMode.Impulse);
        }
    }
}