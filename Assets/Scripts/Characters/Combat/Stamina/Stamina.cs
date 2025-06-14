using System;
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
        public float regenDelay = 1f;
        float timer = 0;
        [ReadOnlyInspector] public bool doRegen = false;


        [Header("Costs")]
        public int staminaToRoll = 1;
        public int staminaToAttack = 1;

        [ReadOnlyInspector] public bool exploreMode;


        public static Action OnInsufficientStamina;



        private void Start()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            CustomEvents.OnChamberActivated += ActivateFightingMode;
            CustomEvents.OnChamberFinished += ActivateExploreMode;
            CustomEvents.OnSuccessfullParry += RestoreOnGoodParry;

            exploreMode = true;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            GetCurrentStats();
        }

        private void RestoreOnGoodParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                RestoreStamina(1);
            }
        }

        private void ActivateExploreMode()
        {
            exploreMode = true;
        }

        private void ActivateFightingMode()
        {
            exploreMode = false;
        }

        private void OnDestroy()
        {
            CustomEvents.OnLevelChange -= SaveCurrentStats;
            CustomEvents.OnChamberActivated -= ActivateFightingMode;
            CustomEvents.OnChamberFinished -= ActivateExploreMode;
            CustomEvents.OnSuccessfullParry -= RestoreOnGoodParry;
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
            timer += Time.deltaTime;

            if (timer > regenDelay)
            {
                doRegen = true;
            }

            // If the current stamina is less than max stamina, regenerate it over time
            if (doRegen && currentStamina < currentMaxStamina)
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

        public void RestoreStamina(int amount)
        {
            doRegen = true;
            currentStamina += amount;

            if (currentStamina > currentMaxStamina)
            {
                currentStamina = currentMaxStamina;
            }
        }

        public bool HasEnoughStamina(float staminaCost)
        {
            if (exploreMode) return true;

            return Mathf.FloorToInt(currentStamina) >= staminaCost;
        }

        public bool TryConsumeStamina(float staminaCost)
        {
            if (exploreMode) return true;

            if (staminaCost < 0)
            {
                currentStamina = Mathf.Min(currentStamina - staminaCost, currentMaxStamina);
                doRegen = false;
                timer = 0;
                return true;
            }

            if (Mathf.FloorToInt(currentStamina) >= staminaCost)
            {
                currentStamina -= staminaCost;
                doRegen = false;
                timer = 0;
                CustomEvents.StaminaUsed();
                return true;
            }

            return false;
        }

        public bool TryConsumeStaminaWithFeedback(float staminaCost)
        {
            if (exploreMode) 
            {
                return true;
            }

            if (staminaCost < 0)
            {
                currentStamina = Mathf.Min(currentStamina - staminaCost, currentMaxStamina);
                doRegen = false;
                timer = 0;
                return true;
            }

            if (Mathf.FloorToInt(currentStamina) >= staminaCost)
            {
                currentStamina -= staminaCost;
                doRegen = false;
                timer = 0;
                CustomEvents.StaminaUsed();
                return true;
            }

            OnInsufficientStamina?.Invoke();
            return false;
        }
    }
}
