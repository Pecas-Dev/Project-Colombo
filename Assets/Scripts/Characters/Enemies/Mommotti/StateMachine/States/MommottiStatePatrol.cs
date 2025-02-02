using ProjectColombo.Enemies.Pathfinding;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStatePatrol : MommottiBaseState
    {
        //pathfinding
        List<Node> currentPath;
        int pathIndex = 0;
        Node lastWalkableNode; //in for now if we need it
        Vector3 movingDirection;

        //player detection
        float checkInterval = 0.5f;
        float timer;

        //patrol points
        bool onSpawnPoint;

        public MommottiStatePatrol(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            timer = 0;
            onSpawnPoint = true;

            if (stateMachine.myMommottiAttributes.spawnPointLocation == null)
            {
                stateMachine.myMommottiAttributes.spawnPointLocation = stateMachine.transform.position;
                stateMachine.myMommottiAttributes.patrolAreaDistance = 25f;
            }


            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
            SetTarget(GetNextPatrolPoint());
            pathIndex = 0;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.PATROL);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer > checkInterval)
            {           
                if (stateMachine.myMommottiAttributes.FieldOfViewCheck() || stateMachine.myMommottiAttributes.SoundDetectionCheck())
                {
                    stateMachine.SwitchState(new MommottiStateAlerted(stateMachine));
                    return;
                }

                Node currentNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
                if (currentNode.walkable)
                {
                    lastWalkableNode = currentNode;
                }

                timer = 0;
            }
            
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                lastWalkableNode = currentPath[pathIndex];
                movingDirection = currentPath[pathIndex].worldPosition - stateMachine.transform.position;

                if (movingDirection.magnitude < stateMachine.myEntityAttributes.moveSpeed * deltaTime) // Reached current path node
                {
                    pathIndex++;

                    if (pathIndex == currentPath.Count)
                    {
                        currentPath = null;
                        pathIndex = 0;
                        SetTarget(GetNextPatrolPoint());
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

        private Vector3 GetNextPatrolPoint()
        {
            if (onSpawnPoint)
            {
                onSpawnPoint = false;
                Vector3 nextPosition;
                Node nextNode;

                for (int i = 0; i < 10; i++)
                {
                    Vector2 center = new Vector2(stateMachine.myMommottiAttributes.spawnPointLocation.x, stateMachine.myMommottiAttributes.spawnPointLocation.z);
                    Vector2 randomInCircle = center + (Random.insideUnitCircle.normalized * Random.Range(2f, stateMachine.myMommottiAttributes.patrolAreaDistance));
                    nextPosition = new Vector3(randomInCircle.x, stateMachine.transform.position.y, randomInCircle.y);

                    nextNode = stateMachine.myPathfindingAlgorythm.GetNode(nextPosition);

                    if (nextNode != null && nextNode.walkable) return nextPosition;
                }

                onSpawnPoint = true;
                return stateMachine.myMommottiAttributes.spawnPointLocation;
            }
            else
            {
                onSpawnPoint = true;
                return stateMachine.myMommottiAttributes.spawnPointLocation;
            }
        }

        public void SetTarget(Vector3 newTarget)
        {
            currentPath = stateMachine.myPathfindingAlgorythm.FindPath(stateMachine.transform.position, newTarget);

            if (currentPath == null) //returns null if not walkable
            {
                currentPath = new List<Node>(); // Initialize the list
                currentPath.Add(lastWalkableNode);
            }

            pathIndex = 0;
        }
    }
}