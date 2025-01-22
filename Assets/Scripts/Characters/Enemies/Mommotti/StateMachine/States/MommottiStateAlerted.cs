using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateAlerted : MommottiBaseState
    {
        Vector3 m_alertedPosition;
        float m_Timer;
        public MommottiStateAlerted(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            m_Timer = 0;
            m_alertedPosition = GetPlayerPosition();
            m_StateMachine.SetCurrentState(MommottiStateMachine.MommottiState.ALERTED);
            Debug.Log("Mommotti entered Alerted State");
        }

        public override void Tick(float deltaTime)
        {
            if (m_StateMachine.m_MommottiAttributes.FieldOfViewCheck() || m_StateMachine.m_MommottiAttributes.SoundDetectionCheck())
            {
                m_alertedPosition = GetPlayerPosition();

                if (m_Timer > m_StateMachine.m_MommottiAttributes.m_AlertedBufferTime)
                {
                    m_StateMachine.SwitchState(new MommottiStateChase(m_StateMachine));
                }
            }
            else if (m_Timer > m_StateMachine.m_MommottiAttributes.m_AlertedBufferTime)
            {
                m_StateMachine.SwitchState(new MommottiStatePatrol(m_StateMachine));
            }

            Vector3 targetDirection = (m_alertedPosition - m_StateMachine.transform.position).normalized;

            if (Vector3.Angle(m_StateMachine.transform.forward, targetDirection) > 1f)
            {
                Quaternion startRotation = m_StateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                m_StateMachine.m_Rigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_StateMachine.m_EntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            m_Timer += deltaTime;
        }

        public override void Exit()
        {
            Debug.Log("Mommotti exited Alerted State");
        }

        private Vector3 GetPlayerPosition()
        {
            return GameObject.Find("Player").transform.position;
        }
    }
}