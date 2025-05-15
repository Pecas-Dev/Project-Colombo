using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.UI.HUD;
using UnityEngine;



namespace ProjectColombo.Combat
{
    public class Stamina : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        [Header("Configuration")]
        [HideInInspector] public int currentMaxStamina;
        [HideInInspector] public float regenTime;
        [HideInInspector] public float currentStamina = 0f;


        [Header("Costs")]
        public int staminaToRoll = 1;
        public int staminaToAttack = 1;



        private void Start()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            GetCurrentStats();
            //GetComponentInChildren<StaminaHUD>().Reset();
        }

        private void OnDestroy()
        {
            CustomEvents.OnLevelChange -= SaveCurrentStats;
        }

        private void SaveCurrentStats()
        {
             myGlobalStats.currentPlayerStamina = currentMaxStamina;
             myGlobalStats.currentStaminaRegenTime = regenTime;
        }

        void GetCurrentStats()
        {
            currentMaxStamina = myGlobalStats.currentPlayerStamina;
            currentStamina = currentMaxStamina;
            regenTime = myGlobalStats.currentStaminaRegenTime;
        }



        void Update()
        {
            // If the current stamina is less than max stamina, regenerate it over time
            if (currentStamina < currentMaxStamina)
            {
                RegenerateStamina(Time.deltaTime);
            }
        }

        void RegenerateStamina(float deltaTime)
        {
            // Calculate the stamina to regenerate this frame
            float regenerationAmount = (1f / regenTime) * deltaTime;

            // Store the old integer value of stamina
            int oldStaminaInt = Mathf.FloorToInt(currentStamina);

            // Regenerate stamina but cap it at max
            currentStamina = Mathf.Min(currentStamina + regenerationAmount, currentMaxStamina);

            // Store the new integer value of stamina
            int newStaminaInt = Mathf.FloorToInt(currentStamina);

            // Fire event for every integer increase
            for (int i = oldStaminaInt + 1; i <= newStaminaInt; i++)
            {
                CustomEvents.StaminaRegenerated();
            }
        }

        public void AddStamina(int extra)
        {
            Debug.Log("changed stamina from: " + currentMaxStamina+ ", by: " + extra);
            currentMaxStamina += extra;

            if (currentStamina > currentMaxStamina)
            {
                currentStamina = currentMaxStamina;
            }

            //GetComponentInChildren<StaminaHUD>().Reset();
        }


        public bool HasEnoughStamina(float staminaCost)
        {
            return Mathf.FloorToInt(currentStamina) >= staminaCost;
        }

        public bool TryConsumeStamina(float staminaCost)
        {
            if (staminaCost < 0)
            {
                currentStamina = Mathf.Min(currentStamina - staminaCost, currentMaxStamina);
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
