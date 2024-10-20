using UnityEngine;
using ProjectColombo.Core;
using UnityEngine.AI;

namespace ProjectColombo.Control
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float waypointDwellTime = 1.5f;
        [SerializeField] float waypointTolerance = 1f;

        [Header("Patrol Settings")]
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float patrolSpeed = 2f;
        [SerializeField] float chaseSpeed = 3.5f;

        private NavMeshAgent agent;
        private Health health;
        private GameObject player;
        private Vector3 guardPosition;
        private float timeSinceLastSawPlayer = Mathf.Infinity;
        private float timeAtWaypoint = Mathf.Infinity;
        private int currentWaypointIndex = 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            player = GameObject.FindGameObjectWithTag("Player");
            guardPosition = transform.position;
        }

        private void Update()
        {
            if (health.GetIsDead()) return;

            if (InChaseRangeOfPlayer() && player.GetComponent<Health>().GetIsDead() == false)
            {
                ChasePlayer();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehavior();
            }
            else
            {
                PatrolBehavior();
            }

            UpdateTimers();
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeAtWaypoint += Time.deltaTime;
        }

        private void ChasePlayer()
        {
            timeSinceLastSawPlayer = 0f;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.transform.position);
        }

        private void SuspicionBehavior()
        {
            agent.ResetPath();
        }

        private void PatrolBehavior()
        {
            agent.speed = patrolSpeed;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeAtWaypoint = 0f;
                    CycleWaypoint();
                }

                if (timeAtWaypoint > waypointDwellTime)
                {
                    agent.SetDestination(GetCurrentWaypoint());
                }
            }
            else
            {
                agent.SetDestination(guardPosition);
            }
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint <= waypointTolerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private bool InChaseRangeOfPlayer()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer <= chaseDistance;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
