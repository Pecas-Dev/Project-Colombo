using ProjectColombo.Input;

using System.Collections.Generic;
using UnityEngine;


namespace ProjectColombo.Combat
{
    public class Targeter : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the GameInput script.")]
        [SerializeField] GameInputSO gameInputSO;

        [Tooltip("Reference to the EntityAttributes for initial facing direction.")]
        [SerializeField] EntityAttributes entityAttributes;


        [Header("Targeting State")]
        public bool isTargetingActive = false;
        public Target currentTarget = null;
        public List<Target> targets = new List<Target>();


        [Header("--DEBUG--")]
        [Tooltip("Line direction controlled by the right stick.")]
        public Vector2 targetDirection = Vector2.up;
        public float lineLength = 2.0f;


        SphereCollider sphereCollider;


        RaycastHit hitInfo;


        void Awake()
        {
            if (gameInputSO == null)
            {
                Debug.LogError("GameInput reference not found. Please assign it in the inspector!");
            }

            if (entityAttributes == null)
            {
                entityAttributes = GetComponentInParent<EntityAttributes>();

                if (entityAttributes == null)
                {
                    Debug.LogError("EntityAttributes reference not found. Please assign it in the inspector!");
                }
            }

            sphereCollider = GetComponent<SphereCollider>();
        }

        void Update()
        {
            HandleTargetingInput();
            UpdateTargetDirection();

            if (isTargetingActive)
            {
                SelectTarget();
            }
        }

        void HandleTargetingInput()
        {
            if (gameInputSO == null)
            {
                return;
            }

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
                    SetInitialTargetDirection();
                    Debug.Log("Targeting System Active");
                }
                else
                {
                    Debug.LogWarning("No targets available to lock on. Targeting not activated.");
                }

                gameInputSO.ResetTargetPressed();
            }
        }

        void SetInitialTargetDirection()
        {
            if (entityAttributes != null)
            {
                Vector3 forward = entityAttributes.GetFacingDirection();
                targetDirection = new Vector2(forward.x, forward.z).normalized;
            }
        }

        void UpdateTargetDirection()
        {
            if (isTargetingActive)
            {
                Vector2 input = gameInputSO.TargetPointInput;

                if (input.magnitude > 0.1f)
                {
                    targetDirection = input.normalized;
                }
            }
        }

        void SelectTarget()
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = new Vector3(targetDirection.x, 0, targetDirection.y).normalized;

            if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, sphereCollider.radius))
            {
                Target hitTarget = hitInfo.collider.GetComponent<Target>();

                if (hitTarget != null && targets.Contains(hitTarget))
                {
                    if (currentTarget != hitTarget)
                    {
                        ClearCurrentTarget();
                        currentTarget = hitTarget;

                        SetTargetColor(currentTarget, Color.red);
                        Debug.Log($"Target Selected: {currentTarget.gameObject.name}");
                    }
                }
            }
        }
        void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                SetTargetColor(currentTarget, Color.white);
                currentTarget = null;
            }
        }

        void SetTargetColor(Target target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<Target>(out Target target))
            {
                return;
            }

            if (!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<Target>(out Target target))
            {
                return;
            }

            if (targets.Contains(target))
            {
                targets.Remove(target);
            }


            // Uncomment if needed in the future

            /*if (targets.Count == 0 && isTargetingActive)
            {
                isTargetingActive = false;
                Debug.Log("Targeting System Off - No targets in range.");
            }*/
        }

        void OnDrawGizmos()
        {
            if (!isTargetingActive || sphereCollider == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);

            Gizmos.color = Color.red;
            Vector3 direction = new Vector3(targetDirection.x, 0, targetDirection.y).normalized;
            Gizmos.DrawLine(transform.position, transform.position + direction * lineLength);

            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}
