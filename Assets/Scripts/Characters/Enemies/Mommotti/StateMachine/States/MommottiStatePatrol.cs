using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStatePatrol : MommottiBaseState
    {
        //public Vector3 m_Target;
        List<Node> m_CurrentPath;
        int m_PathIndex = 0;
        Node m_lastWalkableNode; //in for now if we need it
        Vector3 m_MovingDirection;
        float m_DetectionChecksRate = 0.5f;
        float m_Timer;

        public MommottiStatePatrol(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            m_Timer = 0;
            SetTarget(GameObject.Find("Player").transform.position);
            m_PathIndex = 0;
            m_StateMachine.SetCurrentState(MommottiStateMachine.MommottiState.PATROL);
            Debug.Log("Mommotti entered Patrol State");
        }

        public override void Tick(float deltaTime)
        {
            m_Timer += deltaTime;

            if (m_Timer > m_DetectionChecksRate)
            {           
                if (m_StateMachine.m_MommottiAttributes.FieldOfViewCheck() || m_StateMachine.m_MommottiAttributes.SoundDetectionCheck())
                {
                    m_StateMachine.SwitchState(new MommottiStateAlerted(m_StateMachine));
                    return;
                }

                m_Timer = 0;
            }
            
            if (m_CurrentPath != null && m_PathIndex < m_CurrentPath.Count)
            {
                //m_lastWalkableNode = m_CurrentPath[m_PathIndex];
                m_MovingDirection = m_CurrentPath[m_PathIndex].worldPosition - m_StateMachine.transform.position;

                if (m_MovingDirection.magnitude < m_StateMachine.m_EntityAttributes.walkSpeed * deltaTime) // Reached current path node
                {
                    Debug.Log("swith Node ID: " + m_PathIndex);
                    m_PathIndex++;

                    if (m_PathIndex == m_CurrentPath.Count)
                    {
                        m_CurrentPath = null;
                        m_PathIndex = 0;
                    }
                }
                else
                {
                    if (Vector3.Angle(m_StateMachine.transform.forward, m_MovingDirection) > 1f)
                    {
                        Quaternion startRotation = m_StateMachine.transform.rotation;
                        Quaternion targetRotation = Quaternion.LookRotation(m_MovingDirection);
                        m_StateMachine.m_Rigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, m_StateMachine.m_EntityAttributes.rotationSpeed * deltaTime));
                    }

                    Vector3 movement = m_StateMachine.transform.forward * m_StateMachine.m_EntityAttributes.walkSpeed * deltaTime;
                    m_StateMachine.m_Rigidbody.MovePosition(m_StateMachine.transform.position + movement);
                }
            }
        }

        public override void Exit()
        {
            Debug.Log("Mommotti exited Patrol State");
        }

        public void SetTarget(Vector3 newTarget)
        {
            //m_Target = newTarget;
            m_CurrentPath = m_StateMachine.m_PathfindingAlgorythm.FindPath(m_StateMachine.transform.position, newTarget);
            m_PathIndex = 0;
        }
    }
}