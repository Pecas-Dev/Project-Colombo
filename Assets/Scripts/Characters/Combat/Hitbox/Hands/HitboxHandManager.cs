using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Combat
{
    public class HitboxHandManager : MonoBehaviour
    {
        [Header("Hitbox References")]
        [SerializeField] GameObject leftHandHitbox;
        [SerializeField] GameObject rightHandHitbox;

        [Header("Knockback Settings")]
        [Tooltip("How strong the left hand knockback is when it hits a rigidbody.")]
        [SerializeField] float leftHandKnockbackForce = 2f;

        [Tooltip("How strong the right hand knockback is when it hits a rigidbody.")]
        [SerializeField] float rightHandKnockbackForce = 2f;

        bool leftHandActive = false;
        bool rightHandActive = false;

        List<Collider> leftHandOverlaps = new List<Collider>();
        List<Collider> rightHandOverlaps = new List<Collider>();

        public GameObject LeftHandHitbox => leftHandHitbox;
        public GameObject RightHandHitbox => rightHandHitbox;
        public bool IsLeftHandActive => leftHandActive;
        public bool IsRightHandActive => rightHandActive;

        // ==============================
        // L E F T   H A N D
        // ==============================
        public void EnableLeftHandHitbox()
        {
            if (leftHandHitbox == null) return;

            leftHandHitbox.SetActive(true);
            leftHandActive = true;
            leftHandOverlaps.Clear();

            Debug.Log("Left hand hitbox enabled.");
        }

        public void DealDamageLeftHand()
        {
            if (!leftHandActive)
            {
                Debug.Log("Left hand hitbox is not active; ignoring damage call.");
                return;
            }

            foreach (var col in leftHandOverlaps)
            {
                Debug.Log($"Left hand did a hit on: {col.name}!");

                HealthManager target = col.GetComponent<HealthManager>();

                if (target != null)
                {
                    target.TakeDamage(5);
                }

                Rigidbody targetRb = col.attachedRigidbody;

                if (targetRb != null)
                {
                    Vector3 direction = col.transform.position - leftHandHitbox.transform.position;

                    direction.y = 0f;
                    direction.Normalize();

                    targetRb.AddForce(direction * leftHandKnockbackForce, ForceMode.Impulse);
                }
            }
        }

        public void DisableLeftHandHitbox()
        {
            if (leftHandHitbox == null) return;

            leftHandHitbox.SetActive(false);
            leftHandActive = false;
            leftHandOverlaps.Clear();

            Debug.Log("Left hand hitbox disabled.");
        }

        // ==============================
        // R I G H T   H A N D
        // ==============================
        public void EnableRightHandHitbox()
        {
            if (rightHandHitbox == null) return;

            rightHandHitbox.SetActive(true);
            rightHandActive = true;
            rightHandOverlaps.Clear();

            Debug.Log("Right hand hitbox enabled.");
        }

        public void DealDamageRightHand()
        {
            if (!rightHandActive)
            {
                Debug.Log("Right hand hitbox is not active; ignoring damage call.");
                return;
            }

            foreach (var col in rightHandOverlaps)
            {
                Debug.Log($"Right hand did a hit on: {col.name}!");

                HealthManager target = col.GetComponent<HealthManager>();

                if (target != null)
                {
                    target.TakeDamage(10);
                }

                Rigidbody targetRb = col.attachedRigidbody;

                if (targetRb != null)
                {
                    Vector3 direction = col.transform.position - rightHandHitbox.transform.position;

                    direction.y = 0f;
                    direction.Normalize();

                    targetRb.AddForce(direction * rightHandKnockbackForce, ForceMode.Impulse);
                }
            }
        }

        public void DisableRightHandHitbox()
        {
            if (rightHandHitbox == null) return;

            rightHandHitbox.SetActive(false);
            rightHandActive = false;
            rightHandOverlaps.Clear();

            Debug.Log("Right hand hitbox disabled.");
        }


        // ----------------------------------------------------
        // Overlaps
        // ----------------------------------------------------
        public void AddOverlapLeft(Collider other)
        {
            if (!leftHandOverlaps.Contains(other))
            {
                leftHandOverlaps.Add(other);
            }
        }

        public void RemoveOverlapLeft(Collider other)
        {
            if (leftHandOverlaps.Contains(other))
            {
                leftHandOverlaps.Remove(other);
            }
        }

        public void AddOverlapRight(Collider other)
        {
            if (!rightHandOverlaps.Contains(other))
            {
                rightHandOverlaps.Add(other);
            }
        }

        public void RemoveOverlapRight(Collider other)
        {
            if (rightHandOverlaps.Contains(other))
            {
                rightHandOverlaps.Remove(other);
            }
        }
    }
}
