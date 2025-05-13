using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStatePatrol : MommottiBaseState
    {
        //player detection
        float checkInterval = 0.5f;
        float timer;

        //patrol points
        bool onSpawnPoint;

        //speeds
        float patrolSpeed = 2.5f;
        float rotationSpeed = 5f;

        public MommottiStatePatrol(MommottiStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            Color skinColor = new(0, 1, 0);
            stateMachine.myColorfullSkin.material.color = skinColor;

            timer = 0;
            onSpawnPoint = true;

            if (stateMachine.myMommottiAttributes.spawnPointLocation == null)
            {
                stateMachine.myMommottiAttributes.spawnPointLocation = stateMachine.transform.position;
                stateMachine.myMommottiAttributes.patrolAreaDistance = 25f;
            }


            if (stateMachine.myPathfindingAlgorythm.gridManager == null) //safe keeping i think there is just a timing issue :D
            {
                return;
            }

            lastWalkableNode = stateMachine.myPathfindingAlgorythm.GetNode(stateMachine.transform.position);
            SetTarget(GetNextPatrolPoint());
            pathIndex = 0;
            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.PATROL);

            patrolSpeed = stateMachine.myEntityAttributes.moveSpeed;
            rotationSpeed = stateMachine.myEntityAttributes.rotationSpeedPlayer;
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

            if (!FollowPath(deltaTime, patrolSpeed))
            {
                SetTarget(GetNextPatrolPoint());
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
    }
}