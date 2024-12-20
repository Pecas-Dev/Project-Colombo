using UnityEngine;
using System.Collections.Generic;

public class MovementAlgorithm : MonoBehaviour
{
    EnemyAttributes myEnemyAttributes;
    Rigidbody myRigidbody;
    GameObject currentTarget;

    Vector3 movingDirection;
    Pathfinding pathfinding;
    List<Node> path;
    int pathIndex = 0;
    Node lastWalkableNode = null;

    private void Start()
    {
        myEnemyAttributes = GetComponent<EnemyAttributes>();
        myRigidbody = GetComponent<Rigidbody>();
        currentTarget = myEnemyAttributes.currentTarget;

        pathfinding = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (myEnemyAttributes.currentTarget != null)
        {
            currentTarget = myEnemyAttributes.currentTarget;
        }
        else
        {
            currentTarget = this.gameObject;
        }



        // Check line of sight (LOS)
        if (HasLineOfSight())
        {
            // Direct movement
            movingDirection = currentTarget.transform.position - transform.position;
            movingDirection.y = 0;
            path = null; // Clear the path since we don't need it anymore
        }
        else
        {
            // Use A* to calculate path
            if ((path == null || pathIndex >= path.Count) && currentTarget != this.gameObject)
            {
                path = pathfinding.FindPath(transform.position, currentTarget.transform.position);
                pathIndex = 0;
            }

            // Follow the calculated path
            if (path != null && pathIndex < path.Count)
            {
                Node currentNode = path[pathIndex];

                if (currentNode.walkable)
                {
                    lastWalkableNode = currentNode;
                    movingDirection = path[pathIndex].worldPosition - transform.position;

                    if (movingDirection.magnitude < myEnemyAttributes.distanceToSwitchPatrolPoints) // Reached current path node
                    {
                        pathIndex++;
                    }
                }
                else
                {
                    GoToLastWalkable();

                    pathIndex++;
                }
            }
            else
            {
                GoToLastWalkable();
            }
        }
    }

    private void FixedUpdate()
    {
        if (movingDirection.magnitude > 0.1f) // Prevent jitter when idle
        {
            if (!myEnemyAttributes.onCheckpoint)
            {            
                if (Vector3.Angle(transform.forward, movingDirection) > 1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movingDirection);
                    myRigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, myEnemyAttributes.currentRotationSpeed * Time.fixedDeltaTime));
                }
            }

            if (Vector3.Angle(transform.forward, movingDirection) < 90f && movingDirection.magnitude > myEnemyAttributes.distanceToSwitchPatrolPoints / 2)
            {
                myRigidbody.MovePosition(transform.position + transform.forward * myEnemyAttributes.currentSpeed * Time.fixedDeltaTime);
            }
        }
    }

    private bool HasLineOfSight()
    {
        // Cast a ray to detect obstacles between the enemy and the target
        Vector3 directionToTarget = currentTarget.transform.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToTarget.normalized, out hit, directionToTarget.magnitude))
        {
            // Return false if the ray hits anything other than the target
            return hit.collider.gameObject == currentTarget;
        }

        return true; // No obstacles, clear line of sight
    }

    private void GoToLastWalkable()
    {
        if (lastWalkableNode != null)
        {
            movingDirection = lastWalkableNode.worldPosition - transform.position;
        }
    }
}
