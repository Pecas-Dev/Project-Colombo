using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectColombo.GameManagement.Stats
{

    public enum AllWeapons { SWORD };


    public class GlobalStats : MonoBehaviour
    {
        [Header("Player Default Stats")]
        [Header("EntityAttributes")]
        public float defaultPlayerSpeed = 5.55f;
        public float defaultPlayerAttackSpeed = 1f;

        [ReadOnlyInspector] public float currentPlayerSpeed;
        [ReadOnlyInspector] public float currentPlayerAttackSpeed;

        [Header("HealthManager")]
        public int defaultPlayerMaxHealth = 1000;
        [HideInInspector] public int currentPlayerHealth;
        [ReadOnlyInspector] public int currentPlayerMaxHealth;
        

        [Header("Stamina")]
        public int defaultPlayerStamina = 7;
        public float defaultStaminaRegenTime = 0.5f;

        [ReadOnlyInspector] public int currentPlayerStamina;
        [ReadOnlyInspector] public float currentStaminaRegenTime;


        [Header("Inventory")]
        public int defaultLuckPoints = 0;
        public int defaultCurrencyAmount = 0;

        [ReadOnlyInspector] public int currentLuckPoints;
        [ReadOnlyInspector] public int currentCurrencyAmount;

        [Header("WeaponAttributes")]
        public AllWeapons playerWeapon = AllWeapons.SWORD;
        public List<GameObject> allWeapons;
        public float defaultMajorDamageMultiplyer = 1f;
        public float defaultMinorDamageMultiplyer = 1f;
        public float defaultCorrectAttackScalePercent = 40f;
        public float defaultBlockReductionPercent = 20f;
        public float defaultMissedParryPaneltyPercent = 25f;


        [ReadOnlyInspector] public float currentMajorDamageMultiplyer;
        [ReadOnlyInspector] public float currentMinorDamageMultiplyer; 
        [ReadOnlyInspector] public float currentCorrectAttackScalePercent;
        [ReadOnlyInspector] public float currentBlockReductionPercent;
        [ReadOnlyInspector] public float currentMissedParryPaneltyPercent;


        public float defaultShopDiscountPercent = 0;
        [ReadOnlyInspector] public float currentShopDiscountPercent;



        private void Awake()
        {
            //set all default stats when starting the game
            CustomEvents.OnGameReset += ResetStats;
            ResetStats();
        }


        public void ResetStats()
        {
            currentPlayerSpeed = defaultPlayerSpeed;
            currentPlayerAttackSpeed = defaultPlayerAttackSpeed;

            currentPlayerMaxHealth = defaultPlayerMaxHealth;
            currentPlayerHealth = currentPlayerMaxHealth;

            currentPlayerStamina = defaultPlayerStamina;
            currentStaminaRegenTime = defaultStaminaRegenTime;

            currentLuckPoints = defaultLuckPoints;
            currentCurrencyAmount = defaultCurrencyAmount;

            currentMajorDamageMultiplyer = defaultMajorDamageMultiplyer;
            currentMinorDamageMultiplyer = defaultMinorDamageMultiplyer;
            currentCorrectAttackScalePercent = defaultCorrectAttackScalePercent;
            currentBlockReductionPercent = defaultBlockReductionPercent;
            currentMissedParryPaneltyPercent = defaultMissedParryPaneltyPercent;

            currentShopDiscountPercent = defaultShopDiscountPercent;

            PlayerInventory myInventory = GetComponent<PlayerInventory>();
            myInventory.Reset();
        }

        private void OnDestroy()
        {
            CustomEvents.OnGameReset -= ResetStats;
        }


        public GameObject GetMyWeapon()
        {
            switch (playerWeapon)
            {
                case AllWeapons.SWORD:
                    return allWeapons[0];
            }

            return null;
        }
    }
}