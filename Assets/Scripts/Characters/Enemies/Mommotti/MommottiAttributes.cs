using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.Enemies.Mommotti
{ 
    public class MommottiAttributes : MonoBehaviour
    {
        [HideInInspector]public Transform playerPosition;
        float currentDistanceToPlayer;
        [HideInInspector] public Vector3 spawnPointLocation;
        [HideInInspector] public GridManager myGridManager;
        [HideInInspector] public float patrolAreaDistance;

        [Header("Adjustables")]
        public float alertedBufferTime;
        public float circleDistance;
        public float circleTolerance;
        public float areaToAlertOthers;
        public int attackersAtTheSameTime;

        [Header("Field of View Detection")]
        public float rangeFOVDetection;
        public float angleFOVDetection;

        [Header("Sound Detection")]
        public float rangeSoundDetection;

        private void Awake()
        {
            playerPosition = GameObject.Find("Player").transform;
        }

        private void Update()
        {
            currentDistanceToPlayer = (playerPosition.position - transform.position).magnitude;
        }

        private void OnDrawGizmos()
        {
            //draw fov
            Gizmos.color = Color.yellow;
            //range
            Gizmos.DrawWireSphere(transform.position, rangeFOVDetection);

            //angles
            Vector3 forward = transform.forward;
            float halfFOV = angleFOVDetection / 2;

            Quaternion leftRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRotation = Quaternion.Euler(0, halfFOV, 0);

            Vector3 leftDirection = leftRotation * forward;
            Vector3 rightDirection = rightRotation * forward;

            Gizmos.DrawLine(transform.position, transform.position + leftDirection * rangeFOVDetection);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * rangeFOVDetection);


            //draw sound detection
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rangeSoundDetection);
        }

        public bool FieldOfViewCheck()
        {
            if (currentDistanceToPlayer <= rangeFOVDetection)
            {
                Vector3 directionToPlayer = (playerPosition.position - transform.position).normalized;
                directionToPlayer.y = 0;

                float anglePlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (anglePlayer <= angleFOVDetection / 2)
                {
                    RaycastHit hit;
                    float capsuleRadius = 0.1f;  
                    Vector3 top = transform.position + Vector3.up * capsuleRadius;
                    Vector3 bottom = transform.position - Vector3.up * capsuleRadius;

                    if (Physics.CapsuleCast(top, bottom, capsuleRadius, directionToPlayer, out hit, rangeFOVDetection))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            Debug.DrawRay(transform.position, directionToPlayer * hit.distance, Color.green);
                            return true;
                        }
                    }
                }

                Debug.DrawRay(transform.position, directionToPlayer * rangeFOVDetection, Color.red);
            }

            return false;
        }

        public bool SoundDetectionCheck()
        {
            if (currentDistanceToPlayer < rangeSoundDetection)
            {
                return true;
            }

            return false;
        }

        public Vector3 GetPlayerPosition()
        {
            return playerPosition.position;
        }
    }
}