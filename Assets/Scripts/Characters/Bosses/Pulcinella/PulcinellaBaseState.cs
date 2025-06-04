using ProjectColombo.Enemies.Pathfinding;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.StateMachine.Pulcinella
{
    public abstract class PulcinellaBaseState : State
    {
        protected PulcinellaStateMachine stateMachine;
        private Vector3 currentVelocity = Vector3.zero;
        protected float timer = 0;


        //pathfinding
        protected List<Node> currentPath;
        protected int pathIndex = 0;
        protected Node lastWalkableNode; //in for now if we need it

        public PulcinellaBaseState(PulcinellaStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        protected void MoveToTarget(Vector3 targetPosition, float deltaTime, float speed)
        {
            Vector3 direction = targetPosition - stateMachine.transform.position;

            if (direction.magnitude > 0.1f) // Ensure we don’t overshoot or start lerping if already close
            {
                // Use SmoothDamp for smoother, damped movement
                stateMachine.transform.position = Vector3.SmoothDamp(
                    stateMachine.transform.position,  // Current position
                    targetPosition,                    // Target position
                    ref currentVelocity,               // Reference to the current velocity
                    0.3f,                              // Smooth time (higher value = slower movement)
                    speed,                             // Speed of the movement (max speed)
                    deltaTime                          // DeltaTime for frame-rate independence
                );
            }
        }

        protected void RotateTowardsTarget(Vector3 targetPosition, float deltaTime, float rotationSpeed)
        {
            Vector3 direction = (targetPosition - stateMachine.transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Preserve current Y rotation
                Vector3 currentEuler = stateMachine.transform.rotation.eulerAngles;
                Vector3 targetEuler = targetRotation.eulerAngles;

                targetEuler.y = currentEuler.y;

                Quaternion adjustedRotation = Quaternion.Euler(targetEuler);
                stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, adjustedRotation, rotationSpeed * deltaTime);
            }
        }



        protected bool FollowPath(float deltaTime, float speed, float nodeReachThreshold = 0.2f)
        {
            if (currentPath == null || pathIndex >= currentPath.Count)
                return false;

            Vector3 nextNodePos = currentPath[pathIndex].worldPosition;
            Vector3 toNextNode = nextNodePos - stateMachine.transform.position;
            toNextNode.y = 0;

            if (toNextNode.magnitude <= nodeReachThreshold)
            {
                pathIndex++;
                if (pathIndex >= currentPath.Count)
                    return false;
                nextNodePos = currentPath[pathIndex].worldPosition;
            }

            //RotateTowardsTarget(nextNodePos, deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);
            MoveToTarget(nextNodePos, deltaTime, speed);

            return true;
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
