using ProjectColombo.Combat;
using System.Collections.Generic;
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

        readonly List<GameObject> indicators = new();

        void Start() => BuildLayout();
        void Update()
        {
            if (indicators.Count != staminaData.currentMaxStamina)
                BuildLayout();

            UpdateVisuals();
        }

        void BuildLayout()
        {
            for (int i = 1; i < indicators.Count; i++)
                Destroy(indicators[i]);
            indicators.Clear();

            firstIndicator.SetActive(true);
            indicators.Add(firstIndicator);

            Vector3 basePos = firstIndicator.transform.localPosition;
            Quaternion baseRot = firstIndicator.transform.localRotation;

            Vector3 xDir = Vector3.ProjectOnPlane(firstIndicator.transform.right, Vector3.up).normalized;

            Vector3 zDir = Vector3.Cross(Vector3.up, xDir).normalized;

            for (int i = 1; i < staminaData.currentMaxStamina; i++)
            {
                GameObject clone = Instantiate(firstIndicator, firstIndicator.transform.parent);

                Vector3 offset;

                if (useCurvature && curveRadius > Mathf.Epsilon)
                {
                    float angleRad = (offsetX * i) / curveRadius;

                    offset = xDir * (curveRadius * Mathf.Sin(angleRad));
                    offset += zDir * (curveRadius * (1f - Mathf.Cos(angleRad)));
                }
                else
                {
                    offset = xDir * offsetX * i;
                }

                clone.transform.localPosition = basePos + offset;
                clone.transform.localRotation = baseRot;

                indicators.Add(clone);
            }

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            float current = staminaData.currentStamina;

            for (int i = 0; i < indicators.Count; i++)
            {
                float fill = 0f;
                if (current >= i + 1f) fill = 1f;
                else if (current > i) fill = current % 1;

                indicators[i]
                    .GetComponentInChildren<StaminaIndicator>()
                    .UpdateDisplay(fill);
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying) BuildLayout();
        }
#endif
    }
}
