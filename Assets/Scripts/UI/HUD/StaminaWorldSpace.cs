using System.Collections.Generic;
using ProjectColombo.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;


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
        [SerializeField] Vector3 positionOffset = new Vector3(0, 0, 0);

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

        Transform parentTransform;

        Vector3 worldPositionWithoutRotation;

        void Start()
        {
            parentTransform = transform.parent;

            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "05_Church")
            {
                positionOffset = new Vector3(-9.4f, 10f, -7f);
            }
            else
            {
                positionOffset = new Vector3(-14f, 14f, -11.5f);
            }

            if (parentTransform != null)
            {
                transform.localPosition = positionOffset;

                transform.rotation = Quaternion.identity;
            }

            BuildLayout();
        }

        void LateUpdate()
        {
            if (debugMode)
            {
                HandleDebugControls();
            }

            if (lastBuildMaxStamina != staminaData.currentMaxStamina)
            {
                UpdateLayoutValuesBasedOnStaminaCount();
                BuildLayout();
            }

            if (parentTransform != null)
            {
                Vector3 targetWorldPosition = parentTransform.position + positionOffset;

                transform.rotation = Quaternion.identity;

                transform.position = targetWorldPosition;
            }

            UpdateVisuals();
        }

        void UpdateLayoutValuesBasedOnStaminaCount()
        {
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
            if (applyDebugMaxStamina)
            {
                applyDebugMaxStamina = false;
                staminaData.AddStamina(debugMaxStamina - staminaData.currentMaxStamina);
                Debug.Log($"<color=yellow>Debug</color>: Set max stamina to {debugMaxStamina}");
            }

            if (addStaminaPoint)
            {
                addStaminaPoint = false;
                staminaData.AddStamina(1);
                Debug.Log($"<color=green>Debug</color>: Added stamina point. New max: {staminaData.currentMaxStamina}");
            }

            if (removeStaminaPoint)
            {
                removeStaminaPoint = false;
                if (staminaData.currentMaxStamina > 1)
                {
                    staminaData.AddStamina(-1);
                    Debug.Log($"<color=red>Debug</color>: Removed stamina point. New max: {staminaData.currentMaxStamina}");
                }
                else
                {
                    Debug.Log($"<color=red>Debug</color>: Cannot remove stamina point. Already at minimum (1).");
                }
            }

            if (consumeStamina)
            {
                consumeStamina = false;

                if (staminaData.TryConsumeStamina(debugConsumeAmount))
                {
                    Debug.Log($"<color=orange>Debug</color>: Consumed {debugConsumeAmount} stamina. Current: {staminaData.currentStamina}");
                }
                else
                {
                    Debug.Log($"<color=orange>Debug</color>: Not enough stamina to consume {debugConsumeAmount}.");
                }
            }

            if (refillStamina)
            {
                refillStamina = false;
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