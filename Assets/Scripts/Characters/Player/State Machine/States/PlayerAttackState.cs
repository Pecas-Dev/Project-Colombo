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

            Debug.Log("Entered Attack State");

            //m_playerStateMachine.GameInputSO.DisableAllInputsExceptRoll();

            m_playerStateMachine.PlayerAnimator.CrossFadeInFixedTime(attack.AnimationName, attack.TransitionDuration);
        }

        public override void Tick(float deltaTime)
        {
            if (m_playerStateMachine.GameInputSO.RollPressed)
            {
                m_playerStateMachine.GameInputSO.ResetRollPressed();
                m_playerStateMachine.SwitchState(new PlayerRollState(m_playerStateMachine));

                return;
            }

            float normalizedTime = GetNormalizedTime();

            if (normalizedTime >= previousFrameTime && normalizedTime < 1.0f)
            {
                if(m_playerStateMachine.GameInputSO.AttackPressed)
                {
                    ComboAttack(normalizedTime);
                }
            }
            else
            {
                m_playerStateMachine.SwitchState(new PlayerMovementState(m_playerStateMachine));
            }

            previousFrameTime = normalizedTime;
        }

        public override void Exit()
        {
            Debug.Log("Exited Attack State");

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
    }
}