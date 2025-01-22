using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateChase : MommottiBaseState
    {
        Vector3 m_TargetDirection;
        public MommottiStateChase(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            m_TargetDirection = GetPlayerPosition() - m_StateMachine.transform.position;
            m_StateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);
            Debug.Log("Mommotti entered Chase State");
        }

        public override void Tick(float deltaTime)
        {
            m_TargetDirection = GetPlayerPosition() - m_StateMachine.transform.position;

            if (m_TargetDirection.magnitude < m_StateMachine.m_WeaponAttributes.reach)
            {
                m_StateMachine.SwitchState(new MommottiStateAttack(m_StateMachine));
            }

            //rotate towards player
            if (Vector3.Angle(m_StateMachine.transform.forward, m_TargetDirection.normalized) > 1f)
            {
                Quaternion startRotation = m_StateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
                m_StateMachine.m_Rigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_StateMachine.m_EntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            //adjust speed depending on distance -> fast when far away, slow when close
            float speedFactor = 1 + m_TargetDirection.magnitude / m_StateMachine.m_MommottiAttributes.m_CircleDistance;
            float currentSpeed = m_StateMachine.m_EntityAttributes.moveSpeed * speedFactor;
            Vector3 movingDirection = m_StateMachine.transform.forward; //generally move forward

            if (m_TargetDirection.magnitude < m_StateMachine.m_MommottiAttributes.m_CircleDistance) //if too close and not attacking go back
            {
                movingDirection = -m_StateMachine.transform.forward; 
            }
            else if (m_TargetDirection.magnitude < m_StateMachine.m_MommottiAttributes.m_CircleDistance + m_StateMachine.m_MommottiAttributes.m_CircleTolerance) //if in tolerance zone circle
            {
                currentSpeed += 0.5f;
                movingDirection = m_StateMachine.transform.right;
            }

            m_StateMachine.m_Rigidbody.MovePosition((m_StateMachine.transform.position + (currentSpeed * deltaTime * movingDirection)));
        }

        public override void Exit()
        {
            Debug.Log("Mommotti exited Chase State");
        }

        private Vector3 GetPlayerPosition()
        {
            return GameObject.Find("Player").transform.position;
        }
    }
}