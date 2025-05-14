using ProjectColombo.Enemies;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateChase : MommottiBaseState
    {
        private Vector3 targetDirection;

        private bool isPlayerVisible;

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
            stateMachine.GetComponentInParent<EnemyAttackPriority>().AddToEnemies(stateMachine.gameObject);

            isPlayerVisible = true;
            stateMachine.myMommottiAttributes.rangeFOVDetection /= 3f;

            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);

            targetDirection = stateMachine.myMommottiAttributes.GetPlayerPosition() - stateMachine.transform.position;

            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.CHASE);

            randomSpeedFactor = Random.Range(0.2f, 0.5f);
            CheckClosestEnemy();
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            //attackCheckTimer += deltaTime;

            Vector3 playerPosition = stateMachine.myMommottiAttributes.GetPlayerPosition();
            targetDirection = playerPosition - stateMachine.transform.position;
            targetDirection.y = 0;

            // Pathfinding and visibility update
            if (timer >= checkInterval)
            {
                timer = 0f;

                Node currentNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
                if (currentNode.walkable)
                {
                    lastWalkableNode = currentNode;
                }

                isPlayerVisible = stateMachine.myMommottiAttributes.FieldOfViewCheck();

                if (!isPlayerVisible)
                {
                    SetTarget(playerPosition);
                }

                CheckClosestEnemy();
            }



            // Speed adaptation based on distance
            float speedFactor = Mathf.Min(1 + targetDirection.magnitude / stateMachine.myMommottiAttributes.circleDistance, 1.5f);
            float currentSpeed = stateMachine.myEntityAttributes.moveSpeed * speedFactor;

            // Update movement direction
            if (!isPlayerVisible)
            {
                if (!FollowPath(deltaTime, currentSpeed))
                {
                    SetTarget(playerPosition); // Recalculate if stuck
                }
            }

            // Rotation always faces player
            RotateTowardsTarget(playerPosition, deltaTime, stateMachine.myEntityAttributes.rotationSpeedPlayer);

            // Behavior within circle
            Vector3 relativeMovementDirection = stateMachine.transform.forward;

            if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance - stateMachine.myMommottiAttributes.circleTolerance)
            {
                relativeMovementDirection = -stateMachine.transform.forward;
                currentSpeed /= 2f;
            }
            else if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + stateMachine.myMommottiAttributes.circleTolerance)
            {
                if (closestEnemyDistance < stateMachine.myMommottiAttributes.circleTolerance * 2f)
                {
                    Vector3 spreadDirection = (stateMachine.transform.position - closestEnemyPosition).normalized;
                    relativeMovementDirection = spreadDirection;
                    currentSpeed = stateMachine.myEntityAttributes.moveSpeed * randomSpeedFactor;
                }
                else
                {
                    currentSpeed = 0f;
                }
            }

            // Final move step
            Vector3 targetPosition = stateMachine.transform.position + relativeMovementDirection;
            MoveToTarget(targetPosition, deltaTime, currentSpeed);
        }

        public override void Exit()
        {
            stateMachine.myMommottiAttributes.rangeFOVDetection *= 3f;
        }

        private void CheckClosestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            closestEnemyDistance = Mathf.Infinity;

            foreach (GameObject m in enemies)
            {
                if (m == stateMachine.gameObject) continue;

                MommottiStateMachine otherSM = m.GetComponent<MommottiStateMachine>();
                if (otherSM == null || otherSM.currentState == MommottiStateMachine.MommottiState.ATTACK) continue;

                float dist = Vector3.Distance(stateMachine.transform.position, m.transform.position);
                if (dist < closestEnemyDistance)
                {
                    closestEnemyDistance = dist;
                    closestEnemyPosition = m.transform.position;
                }
            }
        }

        //// Attacking decisions
        //if (attackCheckTimer >= intervalToCheckIfAttacking)
        //{
        //    attackCheckTimer = 0f;

        //    if (targetDirection.magnitude < stateMachine.myMommottiAttributes.circleDistance + 1f)
        //    {
        //        CheckIfShouldAttack();
        //    }
        //}

        //if (targetDirection.magnitude < stateMachine.myWeaponAttributes.reach)
        //{
        //    stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
        //    return;
        //}


        //private void CheckIfShouldAttack()
        //{
        //    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //    int currentAttackers = 0;

        //    float myDist = Vector3.Distance(stateMachine.myMommottiAttributes.GetPlayerPosition(), stateMachine.transform.position);
        //    if (myDist > stateMachine.myMommottiAttributes.circleDistance + 1f) return;

        //    foreach (GameObject m in enemies)
        //    {
        //        if (m.GetComponent<MommottiAttributes>() == null) continue;
        //        if (m.GetComponent<MommottiStateMachine>().currentState == MommottiStateMachine.MommottiState.ATTACK)
        //        {
        //            currentAttackers++;
        //        }
        //    }

        //    if (currentAttackers >= stateMachine.myMommottiAttributes.attackersAtTheSameTime) return;

        //    foreach (GameObject m in enemies)
        //    {
        //        MommottiAttributes otherAttr = m.GetComponent<MommottiAttributes>();
        //        if (otherAttr == null) continue;

        //        float theirDist = Vector3.Distance(otherAttr.playerPosition.position, m.transform.position);

        //        if (myDist <= theirDist)
        //        {
        //            stateMachine.SwitchState(new MommottiStateAttack(stateMachine));
        //            return;
        //        }
        //    }
        //}
    }
}
