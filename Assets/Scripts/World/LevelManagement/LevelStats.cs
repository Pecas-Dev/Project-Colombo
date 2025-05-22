using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class LevelStats : MonoBehaviour
    {
        [Header("Level Management")]
        public int nextSceneNumber;

        [Header("Mommotti Default Stats")]
        [Header("EntityAttributes")]
        public float defaultMommottiSpeed = 3f;
        [ReadOnlyInspector] public float currentMommottiSpeed;

        [Header("HealthManager")]
        public int defaultMommottiMaxHealth = 200;
        [ReadOnlyInspector] public int currentMommottiMaxHealth;

        [Header("WeaponAttibutes")]
        public int defaultMommottiDamage = 50;
        [ReadOnlyInspector] public int currentMommottiDamage;

        [Header("Boss Default Stats")]
        [Header("EntityAttributes")]
        public float defaultBossSpeed = 3f;

        [Header("HealthManager")]
        public int defaultBossMaxHealth = 200;



        [Header("DropRates")]
        [Header("Vases")]
        public float dropChanceCoinsVase;
        public int minAmountOfCoinsVase;
        public int maxAmountOfCoinsVase;
        public float dropChanceCommonCharmVase;
        public float dropChanceRareCharmVase;
        public float dropChanceLegendaryCharmVase;

        [Header("Chest")]
        public float dropChanceCoinsChest;
        public int minAmountOfCoinsChest;
        public int maxAmountOfCoinsChest;
        public float dropChanceCommonCharmChest;
        public float dropChanceRareCharmChest;
        public float dropChanceLegendaryCharmChest;

        [Header("Enemies")]
        public float dropChanceCoinsEnemies;
        public int minAmountOfCoinsEnemies;
        public int maxAmountOfCoinsEnemies;
        public float dropChanceCommonCharmEnemies;
        public float dropChanceRareCharmEnemies;
        public float dropChanceLegendaryCharmEnemies;



        private void Awake()
        {
            //set all default stats when starting the game
            ResetStats();
        }

        public void ResetStats()
        {
            currentMommottiSpeed = defaultMommottiSpeed;
            currentMommottiMaxHealth = defaultMommottiMaxHealth;
            currentMommottiDamage = defaultMommottiDamage;
        }
    }
}