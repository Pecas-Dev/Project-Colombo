using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectColombo.Combat
{
    public class Targeter : MonoBehaviour
    {
        [Header("References")]
        GameInputSO gameInputSO;
        EntityAttributes entityAttributes;

        [Header("Targeting State")]
        public bool isTargetingActive = false;
        public Target currentTarget = null;
        public List<Target> targets = new List<Target>();

        [Header("--DEBUG--")]
        [Tooltip("Direction vector from the targeter input.")]
        public Vector2 targetDirection = Vector2.up;
        [Tooltip("For debug visualization of the target direction.")]
        public float lineLength = 2.0f;

        const float MIN_INPUT_MAGNITUDE = 0.1f;
        const float DIRECTION_ANGLE_THRESHOLD = 45f;
        const float TARGET_SWITCH_COOLDOWN = 0.3f;  // Time in seconds to prevent jarring target switching

        private float targetSwitchTimer = 0f;

        SphereCollider sphereCollider;


        void Start()
        {
            gameInputSO = GameManager.Instance.gameInput;
            entityAttributes = GetComponentInParent<EntityAttributes>();
            sphereCollider = GetComponent<SphereCollider>();
        }

        void Update()
        {
            HandleTargetingInput();
            CleanUpDeadTargets();

            if (isTargetingActive)
            {
                if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
                {
                    ClearCurrentTarget();
                    if (targets.Count > 0)
                    {
                        SelectClosestTarget();
                    }
                }
                else if (currentTarget == null)
                {
                    SelectClosestTarget();
                }
                else
                {
                    UpdateTargetByInput();
                }
            }
            else
            {
                currentTarget = null;
            }

            if (targetSwitchTimer > 0)
            {
                targetSwitchTimer -= Time.deltaTime;
            }
        }

        void HandleTargetingInput()
        {
            if (gameInputSO.TargetPressed)
            {
                if (isTargetingActive)
                {
                    isTargetingActive = false;
                    ClearCurrentTarget();
                    Debug.Log("Targeting System Off");
                }
                else if (targets.Count > 0)
                {
                    isTargetingActive = true;
                    ClearCurrentTarget();
                    SelectClosestTarget();
                    Debug.Log("Targeting System On");
                }
                else
                {
                    Debug.Log("No targets available to lock on. Targeting not activated.");
                }

                gameInputSO.ResetTargetPressed();
            }
        }

        void SelectClosestTarget()
        {
            Target closestTarget = null;

            float minimumDistance = Mathf.Infinity;

            Vector3 originPosition = transform.position;

            foreach (Target target in targets)
            {
                float currentDistance = Vector3.Distance(originPosition, target.transform.position);

                if (currentDistance < minimumDistance)
                {
                    minimumDistance = currentDistance;
                    closestTarget = target;
                }
            }

            if (closestTarget != null)
            {
                SetCurrentTarget(closestTarget);
            }
        }

        void UpdateTargetByInput()
        {
            // Prevent input if the cooldown timer is active
            if (targetSwitchTimer > 0)
            {
                return;
            }

            Vector2 inputDirection = gameInputSO.TargetPointInput;

            if (inputDirection.magnitude < MIN_INPUT_MAGNITUDE)
            {
                return;
            }

            Vector2 normalizedInputDirection = inputDirection.normalized;
            targetDirection = normalizedInputDirection;

            Target candidateTarget = null;

            float minimumCandidateDistance = Mathf.Infinity;

            foreach (Target target in targets)
            {
                if (target == currentTarget)
                {
                    continue;
                }

                Vector3 vectorDifference = target.transform.position - currentTarget.transform.position;
                Vector2 vectorDifference2D = new Vector2(vectorDifference.x, vectorDifference.z);

                if (vectorDifference2D.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                vectorDifference2D.Normalize();

                float angleBetween = Vector2.Angle(normalizedInputDirection, vectorDifference2D);

                if (angleBetween <= DIRECTION_ANGLE_THRESHOLD)
                {
                    float distanceBetween = Vector3.Distance(currentTarget.transform.position, target.transform.position);

                    if (distanceBetween < minimumCandidateDistance)
                    {
                        minimumCandidateDistance = distanceBetween;
                        candidateTarget = target;
                    }
                }
            }

            if (candidateTarget != null && candidateTarget != currentTarget)
            {
                SetCurrentTarget(candidateTarget);
                targetSwitchTimer = TARGET_SWITCH_COOLDOWN;
            }
        }

        void SetCurrentTarget(Target newTarget)
        {
            if (currentTarget == newTarget)
            {
                return;
            }

            ClearCurrentTarget();
            currentTarget = newTarget;
            currentTarget.SetTargetIconActive(true);
        }

        void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.SetTargetIconActive(false);
                currentTarget = null;
            }
        }

        void CleanUpDeadTargets()
        {
            for (int index = targets.Count - 1; index >= 0; index--)
            {
                if (targets[index] == null || !targets[index].gameObject.activeInHierarchy)
                {
                    targets.RemoveAt(index);
                }
            }
        }

        void OnTriggerEnter(Collider colliderOther)
        {
            if (!colliderOther.TryGetComponent<Target>(out Target targetComponent))
            {
                return;
            }

            if (!targets.Contains(targetComponent))
            {
                targets.Add(targetComponent);
            }
        }

        void OnTriggerExit(Collider colliderOther)
        {
            if (!colliderOther.TryGetComponent<Target>(out Target targetComponent))
            {
                return;
            }

            if (targets.Contains(targetComponent))
            {
                targets.Remove(targetComponent);
            }

            if (currentTarget == targetComponent)
            {
                ClearCurrentTarget();

                if (targets.Count > 0)
                {
                    SelectClosestTarget();
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!isTargetingActive || sphereCollider == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);

            Vector3 completeTargetDirection = new Vector3(targetDirection.x, 0, targetDirection.y);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + completeTargetDirection * lineLength);

            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}
