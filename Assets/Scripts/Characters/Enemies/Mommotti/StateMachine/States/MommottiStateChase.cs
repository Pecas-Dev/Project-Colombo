using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
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
        float closestEnemyDistance;
        Vector3 closestEnemyPosition;
        int rotationDirection;
        float randomSpeedFactor;

        //check if should switch to attacking
        float attackCheckTimer = 0;
        float intervallToCheckIfAttacking = 1f;

        public MommottiStateChase(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            Color skinColor = new(1, .5f, 0);
            stateMachine.myColorfullSkin.material.color = skinColor;

            isPlayerVisable = true; //enters from alert State so player is visible

            stateMachine.myMommottiAttributes.rangeFOVDetection /= 3f;

            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            movingDirection = targetDirection;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);
            rotationDirection = Random.Range(0, 2) == 0 ? -1 : 1;
            randomSpeedFactor = Random.Range(0.3f, 0.7f);
            CheckClosestEnemy();
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;
            targetDirection.y = 0;

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

                CheckClosestEnemy();
            }


            attackCheckTimer += deltaTime;

            if (attackCheckTimer >= intervallToCheckIfAttacking)
            {
                attackCheckTimer = 0;

                if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + 1f)
                { 
                    CheckIfShouldAttack(); 
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
            RotateTowardsTarget(stateMachine.myMommottiAttributes.GetPlayerPosition(), deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);


            Vector3 relativeMovementDirection = stateMachine.transform.forward;

            //move away if too close
            if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance) 
            {
                relativeMovementDirection = -stateMachine.transform.forward; 
            }
            //if withing circle area circle
            else if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + stateMachine.myMommottiAttributes.circleTolerance) //if in tolerance zone circle
            {
                //spread out if to close to other
                if (closestEnemyDistance < stateMachine.myMommottiAttributes.circleDistance / 2)
                {
                    Vector3 spreadDirection = (stateMachine.transform.position - closestEnemyPosition).normalized;
                    relativeMovementDirection = spreadDirection;
                    currentSpeed = stateMachine.myEntityAttributes.moveSpeed * randomSpeedFactor;
                }
                //stand and wait
                else
                {
                    currentSpeed = 0f;
                }
            }

            Vector3 targetPosition = stateMachine.transform.position + relativeMovementDirection;
            MoveToTarget(targetPosition, deltaTime, currentSpeed);
        }

        public override void Exit()
        {
            stateMachine.myMommottiAttributes.rangeFOVDetection *= 3f;
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

        private void CheckClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            closestEnemyDistance = Mathf.Infinity;

            foreach (GameObject m in enemies)
            {
                MommottiStateMachine otherStateMachine = m.GetComponent<MommottiStateMachine>();

                //check other is mommotti
                if (otherStateMachine == null) continue;
                if (m == stateMachine.gameObject) continue;
                if (otherStateMachine.currentState == MommottiStateMachine.MommottiState.ATTACK) continue;

                float distance = (m.transform.position - stateMachine.transform.position).magnitude;
                
                if (distance < closestEnemyDistance)
                {
                    closestEnemyDistance = distance;
                    closestEnemyPosition = m.transform.position;
                }
            }
        }

        private void CheckIfShouldAttack()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float currentDistanceToPlayer = (stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position).magnitude;
            int currentAttackers = 0;

            if (currentDistanceToPlayer >= stateMachine.myMommottiAttributes.circleDistance + 1f)
            {
                return;
            }

            foreach (GameObject m in enemies)
            {
                MommottiAttributes otherAttributes = m.GetComponent<MommottiAttributes>();

                //check other is mommotti
                if (otherAttributes == null) continue;

                //check if they are attacking currently
                if (m.GetComponent<MommottiStateMachine>().currentState == MommottiStateMachine.MommottiState.ATTACK)
                {
                    currentAttackers++;
                }
            }

            //check limit of attackers
            if (currentAttackers >= stateMachine.myMommottiAttributes.attackersAtTheSameTime)
            {
                return;
            }

            foreach (GameObject m in enemies)
            {
                MommottiAttributes otherAttributes = m.GetComponent<MommottiAttributes>();

                //check other is mommotti and attacking
                if (otherAttributes == null) continue;
                if (m.GetComponent<MommottiStateMachine>().currentState == MommottiStateMachine.MommottiState.ATTACK) continue;

                //check if they are closer to player
                if (currentDistanceToPlayer <= (otherAttributes.playerPosition.position - m.transform.position).magnitude)
                {
                    //switch to attacking
                    stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
                }
            }
        }
    }
}