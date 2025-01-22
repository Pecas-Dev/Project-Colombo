using UnityEngine;

namespace ProjectColombo.Enemies.Mommotti
{ 
    public class MommottiAttributes : MonoBehaviour
    {
        [HideInInspector]public Transform playerPosition;
        float currentDistanceToPlayer;

        [Header("Adjustables")]
        public float alertedBufferTime;
        public float circleDistance;
        public float circleTolerance;

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

                float anglePlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (anglePlayer <= angleFOVDetection / 2)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(transform.position, 0.5f, directionToPlayer, out hit, rangeFOVDetection, 3)) // You can adjust the radius (0.5f) depending on your needs
                    {
                        if (hit.collider.gameObject == playerPosition.gameObject)
                        {
                            //Debug.DrawLine(transform.position, transform.position + directionToPlayer * m_RangeFOVDetection, Color.green);
                            return true;
                        }
                    }
                }
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
    }
}