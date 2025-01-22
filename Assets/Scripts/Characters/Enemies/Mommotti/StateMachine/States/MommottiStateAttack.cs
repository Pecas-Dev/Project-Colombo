using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAttack : MommottiBaseState
    {
        Vector3 m_TargetDirection;
        public MommottiStateAttack(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            m_StateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ATTACK);
            Debug.Log("Mommotti entered Attack State");
        }

        public override void Tick(float deltaTime)
        {
            m_TargetDirection = GetPlayerPosition() - m_StateMachine.transform.position;

            if (!m_StateMachine.m_WeaponAttributes.attack && m_TargetDirection.magnitude < m_StateMachine.m_WeaponAttributes.reach)
            {
                m_StateMachine.m_Animator.SetTrigger("Attack");
                m_StateMachine.m_WeaponAttributes.attack = true;
            }

            //rotate towards player
            if (Vector3.Angle(m_StateMachine.transform.forward, m_TargetDirection.normalized) > 1f)
            {
                Quaternion startRotation = m_StateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
                m_StateMachine.m_Rigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_StateMachine.m_EntityAttributes.rotationSpeed * deltaTime));
            }

            //move to player if to far away
            if (m_TargetDirection.magnitude > m_StateMachine.m_WeaponAttributes.reach)
            {
                float currentSpeed = m_StateMachine.m_EntityAttributes.walkSpeed;
                Vector3 movingDirection = m_StateMachine.transform.forward;

                m_StateMachine.m_Rigidbody.MovePosition((m_StateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
            }
        }

        public override void Exit()
        {
            Debug.Log("Mommotti exited Attack State");
        }

        private Vector3 GetPlayerPosition()
        {
            return GameObject.Find("Player").transform.position;
        }

        public void Hit()
        {
            Debug.Log("Hit event triggered!");
            // Add logic for handling the hit, e.g., dealing damage
        }
    }
}