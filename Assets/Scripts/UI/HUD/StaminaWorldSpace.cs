using System.Collections.Generic;
using ProjectColombo.Combat;
using UnityEngine;


namespace ProjectColombo.UI.HUD
{
    public class StaminaWorldSpace : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Stamina staminaData;
        [SerializeField] GameObject firstIndicator;

        [Header("Layout")]
        [SerializeField] float offsetX = 0.05f;
        [SerializeField] bool useCurvature = false;
        [SerializeField] float curveRadius = 0.3f;
        [SerializeField] float startAngle = 0f;
        [SerializeField, Range(0f, 180f)] float arcAngle = 90f;
        [SerializeField, Range(0.5f, 2f)] float spacingMultiplier = 1f;

        [Header("Positioning")]
        [SerializeField] Vector3 positionOffset = new Vector3(0, 0, 0); // Additional offset from parent position

        [Header("Debug")]
        [SerializeField] bool debugMode = false;
        [SerializeField, Range(1, 10)] int debugMaxStamina = 7;
        [SerializeField] bool applyDebugMaxStamina = false;
        [SerializeField] bool addStaminaPoint = false;
        [SerializeField] bool removeStaminaPoint = false;
        [SerializeField, Range(0f, 1f)] float debugConsumeAmount = 0.5f;
        [SerializeField] bool consumeStamina = false;
        [SerializeField] bool refillStamina = false;

        readonly List<GameObject> indicators = new();
        int lastBuildMaxStamina = -1;

        // Reference to parent transform for position tracking
        Transform parentTransform;

        // World position tracker to completely bypass parent rotation
        Vector3 worldPositionWithoutRotation;

        void Start()
        {
            // Store reference to parent
            parentTransform = transform.parent;

            // Ensure we store the initial world position accounting for local offset
            if (parentTransform != null)
            {
                // Set local position to apply the offset
                transform.localPosition = positionOffset;

                // Assign a fixed world rotation (identity = no rotation)
                transform.rotation = Quaternion.identity;
            }

            BuildLayout();
        }

        void LateUpdate() // Using LateUpdate to ensure this happens after parent movement
        {
            // Handle debug controls if debug mode is enabled
            if (debugMode)
            {
                HandleDebugControls();
            }

            // If max stamina has changed, update the layout values and rebuild
            if (lastBuildMaxStamina != staminaData.currentMaxStamina)
            {
                UpdateLayoutValuesBasedOnStaminaCount();
                BuildLayout();
            }

            // Handle parent-child positioning with fixed rotation
            if (parentTransform != null)
            {
                // First, calculate the desired world position by adding offset to parent's position
                Vector3 targetWorldPosition = parentTransform.position + positionOffset;

                // Force rotation to identity (no rotation)
                transform.rotation = Quaternion.identity;

                // Set world position directly to bypass parent rotation influence
                transform.position = targetWorldPosition;
            }

            UpdateVisuals();
        }

        // Update layout parameters based on stamina count
        void UpdateLayoutValuesBasedOnStaminaCount()
        {
            // Switch case to set values based on stamina count
            switch (staminaData.currentMaxStamina)
            {
                case 1:
                    startAngle = 227f;
                    offsetX = 1.07f;
                    break;
                case 2:
                    startAngle = 220.0f;
                    offsetX = 1.07f;
                    break;
                case 3:
                    startAngle = 215.0f;
                    offsetX = 1.07f;
                    break;
                case 4:
                    startAngle = 209.0f;
                    offsetX = 1.07f;
                    break;
                case 5:
                    startAngle = 202.0f;
                    offsetX = 1.07f; ;
                    break;
                case 6:
                    startAngle = 198.0f;
                    offsetX = 1.07f;
                    break;
                case 7:
                    startAngle = 187.0f;
                    offsetX = 1.07f;
                    break;
                case 8:
                    startAngle = 172.0f;
                    offsetX = 1.36f;
                    break;
                case 9:
                    startAngle = 164f;
                    offsetX = 1.36f;
                    break;
                default:
                    break;
            }

            if (debugMode)
            {
                Debug.Log($"<color=cyan>Layout</color>: Adjusted for {staminaData.currentMaxStamina} stamina points. " +
                          $"startAngle={startAngle}, offsetX={offsetX}");
            }
        }

        void HandleDebugControls()
        {
            // Apply the debug max stamina if the button was pressed
            if (applyDebugMaxStamina)
            {
                applyDebugMaxStamina = false; // Reset the button
                staminaData.AddStamina(debugMaxStamina - staminaData.currentMaxStamina);
                Debug.Log($"<color=yellow>Debug</color>: Set max stamina to {debugMaxStamina}");
            }

            // Add a stamina point if the button was pressed
            if (addStaminaPoint)
            {
                addStaminaPoint = false; // Reset the button
                staminaData.AddStamina(1);
                Debug.Log($"<color=green>Debug</color>: Added stamina point. New max: {staminaData.currentMaxStamina}");
            }

            // Remove a stamina point if the button was pressed
            if (removeStaminaPoint)
            {
                removeStaminaPoint = false; // Reset the button
                if (staminaData.currentMaxStamina > 1) // Prevent going below 1
                {
                    staminaData.AddStamina(-1);
                    Debug.Log($"<color=red>Debug</color>: Removed stamina point. New max: {staminaData.currentMaxStamina}");
                }
                else
                {
                    Debug.Log($"<color=red>Debug</color>: Cannot remove stamina point. Already at minimum (1).");
                }
            }

            // Consume stamina if the button was pressed
            if (consumeStamina)
            {
                consumeStamina = false; // Reset the button
                if (staminaData.TryConsumeStamina(debugConsumeAmount))
                {
                    Debug.Log($"<color=orange>Debug</color>: Consumed {debugConsumeAmount} stamina. Current: {staminaData.currentStamina}");
                }
                else
                {
                    Debug.Log($"<color=orange>Debug</color>: Not enough stamina to consume {debugConsumeAmount}.");
                }
            }

            // Refill stamina if the button was pressed
            if (refillStamina)
            {
                refillStamina = false; // Reset the button
                float before = staminaData.currentStamina;
                staminaData.currentStamina = staminaData.currentMaxStamina;
                Debug.Log($"<color=blue>Debug</color>: Refilled stamina from {before} to {staminaData.currentStamina}.");
            }
        }

        void BuildLayout()
        {
            lastBuildMaxStamina = staminaData.currentMaxStamina;

            foreach (GameObject indicator in indicators)
            {
                if (indicator != firstIndicator)
                {
                    Destroy(indicator);
                }
            }

            indicators.Clear();

            Vector3 basePos = firstIndicator.transform.localPosition;
            Quaternion baseRot = firstIndicator.transform.localRotation;

            Vector3 xDirection = Vector3.ProjectOnPlane(firstIndicator.transform.right, Vector3.up).normalized;
            Vector3 zDirection = -Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            for (int i = 0; i < staminaData.currentMaxStamina; i++)
            {
                GameObject clone = Instantiate(firstIndicator, firstIndicator.transform.parent);
                clone.SetActive(true);

                Vector3 offset;

                if (useCurvature && curveRadius > Mathf.Epsilon)
                {
                    float startAngleRad = startAngle * Mathf.Deg2Rad;
                    float angleRad = startAngleRad + (offsetX * spacingMultiplier * i) / curveRadius;
                    float maxAngleRad = startAngleRad + (arcAngle * Mathf.Deg2Rad);

                    angleRad = Mathf.Min(angleRad, maxAngleRad);

                    offset = xDirection * (curveRadius * Mathf.Sin(angleRad));
                    offset += zDirection * (curveRadius * (1f - Mathf.Cos(angleRad)));
                }
                else
                {
                    offset = xDirection * (offsetX * spacingMultiplier * i);
                }

                clone.transform.localPosition = basePos + offset;
                clone.transform.localRotation = baseRot;

                indicators.Add(clone);
            }

            firstIndicator.SetActive(false);

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            float current = staminaData.currentStamina;

            for (int i = 0; i < indicators.Count; i++)
            {
                float fill = 0f;

                if (current >= i + 1f)
                {
                    fill = 1f;
                }
                else if (current > i)
                {
                    fill = current % 1;
                }

                indicators[i].GetComponentInChildren<StaminaIndicator>().UpdateDisplay(fill);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Reset the button flags if they're checked when not in play mode
            if (!Application.isPlaying)
            {
                applyDebugMaxStamina = false;
                addStaminaPoint = false;
                removeStaminaPoint = false;
                consumeStamina = false;
                refillStamina = false;
            }

            if (Application.isPlaying)
            {
                // Update parent reference if it changed
                if (transform.parent != parentTransform)
                {
                    parentTransform = transform.parent;
                }

                if (indicators.Count > 0)
                {
                    BuildLayout();
                }
            }
        }
#endif
    }
}

/*using System.Collections.Generic;
using ProjectColombo.Combat;
using UnityEngine;


namespace ProjectColombo.UI.HUD
{
    public class StaminaWorldSpace : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Stamina staminaData;
        [SerializeField] GameObject firstIndicator;

        [Header("Layout")]
        [SerializeField] float offsetX = 0.05f;
        [SerializeField] bool useCurvature = false;
        [SerializeField] float curveRadius = 0.3f;
        [SerializeField] float startAngle = 0f;
        [SerializeField, Range(0f, 180f)] float arcAngle = 90f;
        [SerializeField, Range(0.5f, 2f)] float spacingMultiplier = 1f;

        [Header("Position Tracking")]
        [SerializeField] Transform targetToFollow;
        [SerializeField] Vector3 positionOffset = new Vector3(0, 0, 0);
        [SerializeField] bool maintainWorldRotation = true;

        [Header("Debug")]
        [SerializeField] bool debugMode = false;
        [SerializeField, Range(1, 10)] int debugMaxStamina = 7;
        [SerializeField] bool applyDebugMaxStamina = false;
        [SerializeField] bool addStaminaPoint = false;
        [SerializeField] bool removeStaminaPoint = false;
        [SerializeField, Range(0f, 1f)] float debugConsumeAmount = 0.5f;
        [SerializeField] bool consumeStamina = false;
        [SerializeField] bool refillStamina = false;

        readonly List<GameObject> indicators = new();
        int lastBuildMaxStamina = -1;

        void Start()
        {
            BuildLayout();
        }

        void Update()
        {
            // Handle debug controls if debug mode is enabled
            if (debugMode)
            {
                HandleDebugControls();
            }

            // If max stamina has changed, update the layout values and rebuild
            if (lastBuildMaxStamina != staminaData.currentMaxStamina)
            {
                UpdateLayoutValuesBasedOnStaminaCount();
                BuildLayout();
            }

            if (targetToFollow != null)
            {
                transform.position = targetToFollow.position + positionOffset;

                if (maintainWorldRotation)
                {
                    transform.rotation = Quaternion.identity;
                }
            }

            UpdateVisuals();
        }

        // Update layout parameters based on stamina count
        void UpdateLayoutValuesBasedOnStaminaCount()
        {
            // Switch case to set values based on stamina count
            switch (staminaData.currentMaxStamina)
            {
                case 1:
                    startAngle = 40f;
                    offsetX = 1.07f;
                    break;
                case 2:
                    startAngle = 35f;
                    offsetX = 1.07f;
                    break;
                case 3:
                    startAngle = 25f;
                    offsetX = 1.07f;
                    break;
                case 4:
                    startAngle = 13f;
                    offsetX = 1.07f;
                    break;
                case 5:
                    startAngle = 5f;
                    offsetX = 1.07f; ;
                    break;
                case 6:
                    startAngle = -5f;
                    offsetX = 1.07f;
                    break;
                case 7:
                    startAngle = 170f;
                    offsetX = 1.07f;
                    break;
                case 8:
                    startAngle = -25f;
                    offsetX = 1.36f;
                    break;
                case 9:
                    startAngle = -45f;
                    offsetX = 1.36f;
                    break;
                default:
                    break;
            }

            if (debugMode)
            {
                Debug.Log($"<color=cyan>Layout</color>: Adjusted for {staminaData.currentMaxStamina} stamina points. " +
                          $"startAngle={startAngle}, offsetX={offsetX}");
            }
        }

        void HandleDebugControls()
        {
            // Apply the debug max stamina if the button was pressed
            if (applyDebugMaxStamina)
            {
                applyDebugMaxStamina = false; // Reset the button
                staminaData.AddStamina(debugMaxStamina - staminaData.currentMaxStamina);
                Debug.Log($"<color=yellow>Debug</color>: Set max stamina to {debugMaxStamina}");
            }

            // Add a stamina point if the button was pressed
            if (addStaminaPoint)
            {
                addStaminaPoint = false; // Reset the button
                staminaData.AddStamina(1);
                Debug.Log($"<color=green>Debug</color>: Added stamina point. New max: {staminaData.currentMaxStamina}");
            }

            // Remove a stamina point if the button was pressed
            if (removeStaminaPoint)
            {
                removeStaminaPoint = false; // Reset the button
                if (staminaData.currentMaxStamina > 1) // Prevent going below 1
                {
                    staminaData.AddStamina(-1);
                    Debug.Log($"<color=red>Debug</color>: Removed stamina point. New max: {staminaData.currentMaxStamina}");
                }
                else
                {
                    Debug.Log($"<color=red>Debug</color>: Cannot remove stamina point. Already at minimum (1).");
                }
            }

            // Consume stamina if the button was pressed
            if (consumeStamina)
            {
                consumeStamina = false; // Reset the button
                if (staminaData.TryConsumeStamina(debugConsumeAmount))
                {
                    Debug.Log($"<color=orange>Debug</color>: Consumed {debugConsumeAmount} stamina. Current: {staminaData.currentStamina}");
                }
                else
                {
                    Debug.Log($"<color=orange>Debug</color>: Not enough stamina to consume {debugConsumeAmount}.");
                }
            }

            // Refill stamina if the button was pressed
            if (refillStamina)
            {
                refillStamina = false; // Reset the button
                float before = staminaData.currentStamina;
                staminaData.currentStamina = staminaData.currentMaxStamina;
                Debug.Log($"<color=blue>Debug</color>: Refilled stamina from {before} to {staminaData.currentStamina}.");
            }
        }

        void BuildLayout()
        {
            lastBuildMaxStamina = staminaData.currentMaxStamina;

            foreach (GameObject indicator in indicators)
            {
                if (indicator != firstIndicator)
                {
                    Destroy(indicator);
                }
            }

            indicators.Clear();

            Vector3 basePos = firstIndicator.transform.localPosition;
            Quaternion baseRot = firstIndicator.transform.localRotation;

            Vector3 xDirection = Vector3.ProjectOnPlane(firstIndicator.transform.right, Vector3.up).normalized;
            Vector3 zDirection = -Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            for (int i = 0; i < staminaData.currentMaxStamina; i++)
            {
                GameObject clone = Instantiate(firstIndicator, firstIndicator.transform.parent);
                clone.SetActive(true);

                Vector3 offset;

                if (useCurvature && curveRadius > Mathf.Epsilon)
                {
                    float startAngleRad = startAngle * Mathf.Deg2Rad;
                    float angleRad = startAngleRad + (offsetX * spacingMultiplier * i) / curveRadius;
                    float maxAngleRad = startAngleRad + (arcAngle * Mathf.Deg2Rad);

                    angleRad = Mathf.Min(angleRad, maxAngleRad);

                    offset = xDirection * (curveRadius * Mathf.Sin(angleRad));
                    offset += zDirection * (curveRadius * (1f - Mathf.Cos(angleRad)));
                }
                else
                {
                    offset = xDirection * (offsetX * spacingMultiplier * i);
                }

                clone.transform.localPosition = basePos + offset;
                clone.transform.localRotation = baseRot;

                indicators.Add(clone);
            }

            firstIndicator.SetActive(false);

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            float current = staminaData.currentStamina;

            for (int i = 0; i < indicators.Count; i++)
            {
                float fill = 0f;

                if (current >= i + 1f)
                {
                    fill = 1f;
                }
                else if (current > i)
                {
                    fill = current % 1;
                }

                indicators[i].GetComponentInChildren<StaminaIndicator>().UpdateDisplay(fill);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Reset the button flags if they're checked when not in play mode
            if (!Application.isPlaying)
            {
                applyDebugMaxStamina = false;
                addStaminaPoint = false;
                removeStaminaPoint = false;
                consumeStamina = false;
                refillStamina = false;
            }

            if (Application.isPlaying && indicators.Count > 0)
            {
                BuildLayout();
            }

            if (Application.isPlaying && targetToFollow != null && maintainWorldRotation)
            {
                transform.rotation = Quaternion.identity;
            }
        }
#endif
    }
}*/