using ProjectColombo.UI.HUD;
using UnityEngine;



namespace ProjectColombo.Combat
{
    public class Stamina : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] public StaminaConfigSO staminaConfig;

        [HideInInspector] public int maxStamina = 0;
        [HideInInspector] public float currentStamina = 0f;


        void Start()
        {
            maxStamina = (int)staminaConfig.MaxStaminaPoints;
            currentStamina = staminaConfig.MaxStaminaPoints;

            GetComponentInChildren<StaminaHUD>().Reset();
        }

        void Update()
        {
            // If the current stamina is less than max stamina, regenerate it over time
            if (currentStamina < staminaConfig.MaxStaminaPoints)
            {
                RegenerateStamina(Time.deltaTime);
            }
        }

        void RegenerateStamina(float deltaTime)
        {
            // How much stamina to regenerate based on the configured rate
            float regenerationAmount = staminaConfig.StaminaRegenerationRate * deltaTime; // Apply frame time (Time.deltaTime)

            // Add the regeneration amount but make sure we don't exceed max stamina
            currentStamina = Mathf.Min(currentStamina + regenerationAmount, staminaConfig.MaxStaminaPoints);
        }

        public bool HasEnoughStamina(float staminaCost)
        {
            return Mathf.FloorToInt(currentStamina) >= staminaCost;
        }

        public bool TryConsumeStamina(float staminaCost)
        {
            if (staminaCost < 0)
            {
                currentStamina = Mathf.Min(currentStamina - staminaCost, staminaConfig.MaxStaminaPoints);
                return true;
            }

            if (Mathf.FloorToInt(currentStamina) >= staminaCost)
            {
                currentStamina -= staminaCost;
                return true;
            }

            return false;
        }
    }
}
