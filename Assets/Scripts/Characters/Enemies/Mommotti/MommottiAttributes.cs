using UnityEngine;

namespace ProjectColombo.Enemies.Mommotti
{ 
    public class MommottiAttributes : MonoBehaviour
    {
        [HideInInspector]public Transform m_PlayerPosition;
        float m_CurrentDistanceToPlayer;

        [Header("Adjustables")]
        public float m_AlertedBufferTime;
        public float m_CircleDistance;
        public float m_CircleTolerance;

        [Header("Field of View Detection")]
        public float m_RangeFOVDetection;
        public float m_AngleFOVDetection;

        [Header("Sound Detection")]
        public float m_RangeSoundDetection;

        private void Awake()
        {
            m_PlayerPosition = GameObject.Find("Player").transform;
        }

        private void Update()
        {
            m_CurrentDistanceToPlayer = (m_PlayerPosition.position - transform.position).magnitude;
        }

        private void OnDrawGizmos()
        {
            //draw fov
            Gizmos.color = Color.yellow;
            //range
            Gizmos.DrawWireSphere(transform.position, m_RangeFOVDetection);

            //angles
            Vector3 forward = transform.forward;
            float halfFOV = m_AngleFOVDetection / 2;

            Quaternion leftRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRotation = Quaternion.Euler(0, halfFOV, 0);

            Vector3 leftDirection = leftRotation * forward;
            Vector3 rightDirection = rightRotation * forward;

            Gizmos.DrawLine(transform.position, transform.position + leftDirection * m_RangeFOVDetection);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * m_RangeFOVDetection);


            //draw sound detection
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_RangeSoundDetection);
        }

        public bool FieldOfViewCheck()
        {
            if (m_CurrentDistanceToPlayer <= m_RangeFOVDetection)
            {
                Vector3 directionToPlayer = (m_PlayerPosition.position - transform.position).normalized;

                float anglePlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (anglePlayer <= m_AngleFOVDetection / 2)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(transform.position, 0.5f, directionToPlayer, out hit, m_RangeFOVDetection, 3)) // You can adjust the radius (0.5f) depending on your needs
                    {
                        if (hit.collider.gameObject == m_PlayerPosition.gameObject)
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
            if (m_CurrentDistanceToPlayer < m_RangeSoundDetection)
            {
                return true;
            }

            return false;
        }
    }
}