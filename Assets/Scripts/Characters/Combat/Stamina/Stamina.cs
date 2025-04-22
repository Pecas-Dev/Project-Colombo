using ProjectColombo.GameManagement.Events;
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
        [HideInInspector] public float regenSpeed;


        void Start()
        {
            regenSpeed = staminaConfig.StaminaRegenerationRate;
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
            // Calculate the stamina to regenerate this frame
            float regenerationAmount = regenSpeed * deltaTime;

            // Store the old integer value of stamina
            int oldStaminaInt = Mathf.FloorToInt(currentStamina);

            // Regenerate stamina but cap it at max
            currentStamina = Mathf.Min(currentStamina + regenerationAmount, staminaConfig.MaxStaminaPoints);

            // Store the new integer value of stamina
            int newStaminaInt = Mathf.FloorToInt(currentStamina);

            // Fire event for every integer increase
            for (int i = oldStaminaInt + 1; i <= newStaminaInt; i++)
            {
                CustomEvents.StaminaRegenerated();
            }
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
                CustomEvents.StaminaUsed();
                return true;
            }

            return false;
        }
    }
}
