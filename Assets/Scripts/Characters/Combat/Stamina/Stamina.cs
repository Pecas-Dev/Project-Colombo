using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


namespace ProjectColombo.Combat
{
    public class Stamina : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] StaminaConfigSO staminaConfig;

        [Header("UI References")]
        [SerializeField] Image staminaBar;
        [SerializeField] TextMeshProUGUI staminaText;

        float currentStamina;
        float visualStamina;
        bool isRegenerating;
        Coroutine regenerationCoroutine;
        Coroutine visualUpdateCoroutine;

        //add bool for to only use stamina if not in combat (when there are enemies)



        public StaminaConfigSO StaminaConfig => staminaConfig;

        void Start()
        {
            currentStamina = staminaConfig.MaxStaminaPoints;
            visualStamina = currentStamina;
            UpdateStaminaVisual();
            UpdateStaminaText();
        }

        public bool HasEnoughStamina(float staminaCost)
        {
            return currentStamina >= staminaCost;
        }

        public bool TryConsumeStamina(float staminaCost)
        {
            if (staminaCost < 0)
            {
                currentStamina = Mathf.Min(currentStamina - staminaCost, staminaConfig.MaxStaminaPoints);
                UpdateStaminaVisual();
                UpdateStaminaText();
                return true;
            }

            if (currentStamina >= staminaCost)
            {
                currentStamina -= staminaCost;
                UpdateStaminaVisual();
                UpdateStaminaText();
                StartStaminaRegeneration();
                return true;
            }

            return false;
        }

        void StartStaminaRegeneration()
        {
            if (regenerationCoroutine != null)
            {
                StopCoroutine(regenerationCoroutine);
            }

            regenerationCoroutine = StartCoroutine(RegenerateStamina());
        }

        IEnumerator RegenerateStamina()
        {
            if (currentStamina < staminaConfig.MaxStaminaPoints)
            {
                yield return new WaitForSeconds(staminaConfig.StaminaRegenerationDelay);

                while (currentStamina < staminaConfig.MaxStaminaPoints)
                {
                    currentStamina = Mathf.Min(currentStamina + 1, staminaConfig.MaxStaminaPoints);
                    UpdateStaminaVisual();
                    UpdateStaminaText();

                    yield return new WaitForSeconds(staminaConfig.StaminaRegenerationRate);
                }
            }
        }

        void UpdateStaminaText()
        {
            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.Floor(currentStamina)}/{staminaConfig.MaxStaminaPoints}";
            }
        }

        void UpdateStaminaVisual()
        {
            if (visualUpdateCoroutine != null)
            {
                StopCoroutine(visualUpdateCoroutine);
            }

            visualUpdateCoroutine = StartCoroutine(SmoothUpdateStaminaVisual());
        }

        IEnumerator SmoothUpdateStaminaVisual()
        {
            float startValue = visualStamina;
            float endValue = currentStamina;
            float elapsedTime = 0f;

            while (elapsedTime < staminaConfig.StaminaPointVisualFillDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / staminaConfig.StaminaPointVisualFillDuration;
                visualStamina = Mathf.Lerp(startValue, endValue, normalizedTime);
                staminaBar.fillAmount = visualStamina / staminaConfig.MaxStaminaPoints;
                yield return null;
            }

            visualStamina = endValue;
            staminaBar.fillAmount = visualStamina / staminaConfig.MaxStaminaPoints;
        }
    }
}