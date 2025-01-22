using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStatePatrol : MommottiBaseState
    {
        //public Vector3 m_Target;
        List<Node> currentPath;
        int pathIndex = 0;
        Node lastWalkableNode; //in for now if we need it
        Vector3 movingDirection;
        float detectionChecksRate = 0.5f;
        float timer;

        public MommottiStatePatrol(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            timer = 0;
            SetTarget(GameObject.Find("Player").transform.position);
            pathIndex = 0;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.PATROL);
            Debug.Log("Mommotti entered Patrol State");
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer > detectionChecksRate)
            {           
                if (stateMachine.myMommottiAttributes.FieldOfViewCheck() || stateMachine.myMommottiAttributes.SoundDetectionCheck())
                {
                    stateMachine.SwitchState(new MommottiStateAlerted(stateMachine));
                    return;
                }

                timer = 0;
            }
            
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                //m_lastWalkableNode = m_CurrentPath[m_PathIndex];
                movingDirection = currentPath[pathIndex].worldPosition - stateMachine.transform.position;

                if (movingDirection.magnitude < stateMachine.myEntityAttributes.moveSpeed * deltaTime) // Reached current path node
                {
                    pathIndex++;

                    if (pathIndex == currentPath.Count)
                    {
                        currentPath = null;
                        pathIndex = 0;
                    }
                }
                else
                {
                    if (Vector3.Angle(stateMachine.transform.forward, movingDirection) > 1f)
                    {
                        Quaternion startRotation = stateMachine.transform.rotation;
                        Quaternion targetRotation = Quaternion.LookRotation(movingDirection);
                        stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime));
                    }

                    Vector3 movement = stateMachine.transform.forward * stateMachine.myEntityAttributes.moveSpeed * deltaTime;
                    stateMachine.myRigidbody.MovePosition(stateMachine.transform.position + movement);
                }
            }
        }

        public override void Exit()
        {
        }

        public void SetTarget(Vector3 newTarget)
        {
            //m_Target = newTarget;
            currentPath = stateMachine.myPathfindingAlgorythm.FindPath(stateMachine.transform.position, newTarget);
            pathIndex = 0;
        }
    }
}