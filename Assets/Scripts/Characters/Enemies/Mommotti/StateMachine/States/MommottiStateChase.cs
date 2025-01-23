using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateChase : MommottiBaseState
    {
        Vector3 targetDirection; //direction directly to player

        //pathfinding
        bool isPlayerVisable;
        Node lastWalkableNode;
        List<Node> currentPath;
        int pathIndex = 0;
        Vector3 movingDirection; //direction to next Node

        //check for path just every once in a while
        float checkIntervall = 0.5f;
        float timer;

        //randomize circle speed and dircetion for variety
        int rotationDirection;
        float randomSpeedFactor;

        public MommottiStateChase(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            isPlayerVisable = true; //enters from alert State so player is visible
            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            movingDirection = targetDirection;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);
            rotationDirection = Random.Range(0, 2) == 0 ? -1 : 1;
            randomSpeedFactor = Random.Range(0.3f, 0.7f);
            Debug.Log("Mommotti entered Chase State");
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;

            if (timer > checkIntervall)
            {
                timer = 0;

                Node currentNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
                if (currentNode.walkable)
                {
                    lastWalkableNode = currentNode;
                }

                isPlayerVisable = stateMachine.myMommottiAttributes.FieldOfViewCheck();

                if (!isPlayerVisable)
                {
                    SetTarget(stateMachine.myMommottiAttributes.GetPlayerPosition());
                }
            }

            if (targetDirection.magnitude < stateMachine.myWeaponAttributes.reach)
            {
                stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
            }

            movingDirection = targetDirection;

            //adjust speed depending on distance -> fast when far away, slow when close
            float speedFactor = 1 + targetDirection.magnitude / stateMachine.myMommottiAttributes.circleDistance;
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed * speedFactor;

            if (!isPlayerVisable && currentPath != null && pathIndex < currentPath.Count) 
            {
                if (movingDirection.magnitude < currentSpeed * deltaTime)
                {
                    pathIndex++;
                }

                movingDirection = (currentPath[pathIndex].worldPosition - stateMachine.transform.position);
            }

            //rotate towards player
            if (Vector3.Angle(stateMachine.transform.forward, movingDirection.normalized) > 1f)
            {
                Quaternion startRotation = stateMachine.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(movingDirection.normalized);
                stateMachine.myRigidbody.MoveRotation(Quaternion.RotateTowards(startRotation, targetRotation, stateMachine.myEntityAttributes.rotationSpeedPlayer * deltaTime));
            }

            Vector3 relativeMovementDirection = stateMachine.transform.forward; //generally move forward

            if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance) //if too close and not attacking go back
            {
                relativeMovementDirection = -stateMachine.transform.forward; 
            }
            else if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + stateMachine.myMommottiAttributes.circleTolerance) //if in tolerance zone circle
            {
                relativeMovementDirection = rotationDirection * stateMachine.transform.right;

                currentSpeed *= randomSpeedFactor;
            }

            stateMachine.myRigidbody.MovePosition((stateMachine.transform.position + (currentSpeed * deltaTime * relativeMovementDirection)));
        }

        public override void Exit()
        {
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