using UnityEngine;

namespace ProjectColombo.Combat
{
    public class HandBasedSlashCollider : MonoBehaviour
    {
        public Transform rootBone;
        public Transform handBone;
        private CapsuleCollider capsule;

        void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
        }

        void Update()
        {
            // Get root position in hand's local space
            Vector3 rootInHandLocal = handBone.InverseTransformPoint(rootBone.position);

            // Calculate direction and distance from hand to root
            Vector3 direction = rootInHandLocal;
            float distance = direction.magnitude;

            // Compute rotation from hand "up" to root direction
            Quaternion capsuleRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);


            capsule.center = capsuleRotation * (Vector3.up * (distance * 0.5f));
            capsule.height = distance;
            capsule.direction = 1; // Y-axis

            Debug.DrawLine(handBone.position, rootBone.position, Color.red);

        }
    }
}
