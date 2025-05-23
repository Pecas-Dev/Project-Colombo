using ProjectColombo.Enemies.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public abstract class MommottiBaseState : State
    {
        protected MommottiStateMachine stateMachine;
        private Vector3 currentVelocity = Vector3.zero;

        //pathfinding
        protected List<Node> currentPath;
        protected int pathIndex = 0;
        protected Node lastWalkableNode; //in for now if we need it

        public MommottiBaseState(MommottiStateMachine stateMachine)
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

            stateMachine.myAnimator.SetFloat("Speed", currentVelocity.magnitude);
        }

        protected void RotateTowardsTarget(Vector3 targetPosition, float deltaTime, float rotationSpeed)
        {
            Vector3 direction = (targetPosition - stateMachine.transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f) // Prevent jittering when already aligned
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }


        protected bool FollowPath(float deltaTime, float speed, float nodeReachThreshold = 0.2f)
        {
            if (currentPath == null || pathIndex >= currentPath.Count)
            {
                stateMachine.myAnimator.SetFloat("Speed", 0);
                return false;
            }

            Vector3 nextNodePos = currentPath[pathIndex].worldPosition;
            Vector3 toNextNode = nextNodePos - stateMachine.transform.position;
            toNextNode.y = 0;

            if (toNextNode.magnitude <= nodeReachThreshold)
            {
                pathIndex++;
                if (pathIndex >= currentPath.Count)
                {
                    stateMachine.myAnimator.SetFloat("Speed", 0);
                    return false;
                }

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

        protected bool GetSpreadOutTarget(float threshold, out Vector3 spreadTarget)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float closestDist = Mathf.Infinity;
            Vector3 closestPos = Vector3.zero;

            foreach (GameObject m in enemies)
            {
                if (m == stateMachine.gameObject) continue;

                MommottiStateMachine otherSM = m.GetComponent<MommottiStateMachine>();
                if (otherSM == null) continue;

                float dist = Vector3.Distance(stateMachine.transform.position, m.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPos = m.transform.position;
                }
            }

            if (closestDist < threshold)
            {
                Vector3 away = (stateMachine.transform.position - closestPos).normalized;
                spreadTarget = stateMachine.transform.position + away * threshold;
                return true;
            }

            spreadTarget = Vector3.zero;
            return false;
        }

    }
}
