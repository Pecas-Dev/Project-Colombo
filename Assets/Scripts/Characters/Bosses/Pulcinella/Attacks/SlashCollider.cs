using UnityEngine;

namespace ProjectColombo.Combat
{
    public class HandBasedSlashCollider : MonoBehaviour
    {
        public Transform rootBone;
        private CapsuleCollider capsule;

        void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
        }

        void Update()
        {
            // Get direction and distance to root in local space of the hand
            Vector3 localRootPosition = transform.InverseTransformPoint(rootBone.position);
            Vector3 direction = localRootPosition;
            float distance = direction.magnitude;

            // Set the center of the capsule at the midpoint between hand (0) and root
            capsule.center = direction * 0.5f;

            // Align the capsule to point toward the root
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            capsule.transform.localRotation = rotation;

            // Set capsule properties
            capsule.height = distance;
            capsule.direction = 1; // Y-axis
        }
    }
}