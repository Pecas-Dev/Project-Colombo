using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.LevelManagement;
using UnityEngine;

namespace ProjectColombo.Enemies.DropSystem
{
    public enum Variant { VASE = 0, CHEST, ENEMY };

    public class DropSystem : MonoBehaviour
    {
        public Variant myVariant;
        LevelStats myLevelStats;
        bool hasDropped = false;

        float dropChanceCoins;
        int minAmountOfCoins;
        int maxAmountOfCoins;
        float dropChanceCommonCharm;
        float dropChanceRareCharm;
        float dropChanceLegendaryCharm;



        private void Start()
        {
            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            GetDropRates();
        }

        void GetDropRates()
        {
            switch (myVariant)
            {
                case Variant.VASE:
                    dropChanceCoins = myLevelStats.dropChanceCoinsVase;
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsVase;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsVase;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmVase;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmVase;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmVase;
                    return;

                case Variant.CHEST:
                    dropChanceCoins = myLevelStats.dropChanceCoinsChest;
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsChest;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsChest;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmChest;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmChest;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmChest;
                    return;

                default:
                    dropChanceCoins = myLevelStats.dropChanceCoinsEnemies;
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsEnemies;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsEnemies;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmEnemies;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmEnemies;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmEnemies;
                    return;
            }
        }

        public void DropItem()
        {
            if (hasDropped) return;
            hasDropped = true;

            DropManager manager = GameManager.Instance.GetComponent<DropManager>();
            int currentLuck = GameManager.Instance.GetComponent<PlayerInventory>().currentLuck;

            float currentCommon = dropChanceCommonCharm + currentLuck * 0.25f;
            float currentRare = dropChanceRareCharm + currentLuck * 0.25f;
            float currentLegendary = dropChanceLegendaryCharm + currentLuck * 0.25f;
            float currentCoins = dropChanceCoins;

            float totalChance = currentCommon + currentRare + currentLegendary + currentCoins;

            float roll = Random.Range(0f, 100f);
            if (roll > totalChance) return; // No drop

            float weightedRoll = Random.Range(0f, totalChance);
            float cumulative = 0f;

            Vector3 pos = new Vector3(transform.position.x, 0f, transform.position.z);

            if ((cumulative += currentCommon) >= weightedRoll)
            {
                manager.DropRandomCommonCharm(pos);
            }
            else if ((cumulative += currentRare) >= weightedRoll)
            {
                manager.DropRandomRareCharm(pos);
            }
            else if ((cumulative += currentLegendary) >= weightedRoll)
            {
                manager.DropRandomLegendaryCharm(pos);
            }
            else
            {
                int rand = Random.Range(minAmountOfCoins, maxAmountOfCoins + 1);
                rand += 2 * currentLuck;
                manager.DropCoins(rand, pos);
            }
        }

    }
}