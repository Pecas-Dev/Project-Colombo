using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Linq;


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
        Coroutine regenerationCoroutine;
        Coroutine visualUpdateCoroutine;

        public bool isInCombat = false;
        float checkTime = 1f;
        float checkCombatTimer = 0f;
        //add bool for to only use stamina if not in combat (when there are enemies)



        public StaminaConfigSO StaminaConfig => staminaConfig;

        void Start()
        {
            currentStamina = staminaConfig.MaxStaminaPoints;
            visualStamina = currentStamina;
            UpdateStaminaVisual();
            UpdateStaminaText();
        }

        private void Update()
        {
            checkCombatTimer += Time.deltaTime;

            if (checkCombatTimer >= checkTime)
            {
                isInCombat = CheckForEnemies();
                checkCombatTimer = 0;
            }
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

            if (!isInCombat) return true; //ignore stamina if out of combat

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

                // After delay, start regenerating with a faster rate
                float fastRegenerationRate = staminaConfig.StaminaRegenerationRate * 0.2f; // e.g., 50% faster

                // Regenerate until stamina reaches max
                while (currentStamina < staminaConfig.MaxStaminaPoints)
                {
                    TryConsumeStamina(-1);
                    yield return new WaitForSeconds(fastRegenerationRate);
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
            float startValue = staminaBar.fillAmount * staminaConfig.MaxStaminaPoints; // Use the current fill as start value
            float endValue = currentStamina; // Target stamina
            float elapsedTime = 0f;

            while (elapsedTime < staminaConfig.StaminaPointVisualFillDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / staminaConfig.StaminaPointVisualFillDuration;

                // Interpolate from current visual stamina to new stamina value
                visualStamina = Mathf.Lerp(startValue, endValue, normalizedTime);
                staminaBar.fillAmount = visualStamina / staminaConfig.MaxStaminaPoints;

                yield return null; // Wait for next frame
            }

            // Ensure final value is set properly
            visualStamina = endValue;
            staminaBar.fillAmount = visualStamina / staminaConfig.MaxStaminaPoints;
        }


        private bool CheckForEnemies()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            return enemies.Count() > 0;
        }

    }
}