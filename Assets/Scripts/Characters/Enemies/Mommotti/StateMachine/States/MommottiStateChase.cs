using ProjectColombo.Enemies;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateChase : MommottiBaseState
    {
        private Vector3 targetDirection;

        private float checkInterval = 0.5f;
        private float timer;

        private float closestEnemyDistance;
        private Vector3 closestEnemyPosition;
        private float randomSpeedFactor;

        //private float attackCheckTimer = 0;
        //private float intervalToCheckIfAttacking = 0.5f;

        public MommottiStateChase(MommottiStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);

            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;

            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);

            randomSpeedFactor = Random.Range(0.2f, 0.5f);
            CheckClosestEnemy();
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            Vector3 playerPosition = stateMachine.myMommottiAttributes.GetPlayerPosition();
            targetDirection = playerPosition - stateMachine.transform.position;
            targetDirection.y = 0;

            // Always rotate to face player
            RotateTowardsTarget(playerPosition, deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);

            // Speed adaptation based on distance
            //float speedFactor = Mathf.Min(1 + targetDirection.magnitude / stateMachine.myMommottiAttributes.circleDistance, 1.5f);
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed;

            if (timer >= checkInterval)
            {
                timer = 0f;

                Node currentNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
                if (currentNode.walkable)
                {
                    lastWalkableNode = currentNode;
                }

                CheckClosestEnemy();

                float circleDistance = stateMachine.myMommottiAttributes.circleDistance;
                float tolerance = stateMachine.myMommottiAttributes.circleTolerance;
                float distanceToPlayer = Vector3.Distance(stateMachine.transform.position, playerPosition);


                // First check if too close to another enemy
                if (GetSpreadOutTarget(tolerance * 2f, out Vector3 spreadTarget))
                {
                    SetTarget(spreadTarget);
                }
                else
                {
                    Vector3 desiredPosition = stateMachine.transform.position;

                    if (distanceToPlayer > circleDistance + tolerance)
                    {
                        // Move toward player
                        desiredPosition = stateMachine.transform.position + targetDirection.normalized * tolerance;

                        //faster if further away
                        float speedFactor = Mathf.Min(1 + targetDirection.magnitude / stateMachine.myMommottiAttributes.circleDistance, 1.5f);
                        currentSpeed *= speedFactor;
                    }
                    else if (distanceToPlayer < circleDistance - tolerance)
                    {
                        // Move away from player
                        desiredPosition = stateMachine.transform.position - targetDirection.normalized * tolerance;
                        currentSpeed /= 2f; //slowly move away from player
                    }
                    else
                    {
                        // Inside circle and not too close to others: hold position
                        desiredPosition = stateMachine.transform.position;
                    }

                    SetTarget(desiredPosition);
                }
            }

            FollowPath(deltaTime, currentSpeed);
        }



        public override void Exit()
        {
        }

        private void CheckClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            closestEnemyDistance = Mathf.Infinity;

            foreach (GameObject m in enemies)
            {
                if (m == stateMachine.gameObject) continue;

                MommottiStateMachine otherSM = m.GetComponent<MommottiStateMachine>();
                if (otherSM == null || otherSM.currentStateEnum == MommottiStateMachine.MommottiState.ATTACK) continue;

                float dist = Vector3.Distance(stateMachine.transform.position, m.transform.position);
                if (dist < closestEnemyDistance)
                {
                    closestEnemyDistance = dist;
                    closestEnemyPosition = m.transform.position;
                }
            }
        }
    }
}
