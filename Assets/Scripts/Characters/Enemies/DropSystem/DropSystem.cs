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
            float random = Random.Range(0, 100);
            DropManager manager = GameManager.Instance.GetComponent<DropManager>();

            int currentLuck = GameManager.Instance.GetComponent<PlayerInventory>().currentLuck;

            int currentDropChanceCommonCharm = Mathf.RoundToInt(dropChanceCommonCharm * currentLuck * 0.25f);
            int currentDropChanceRareCharm = Mathf.RoundToInt(dropChanceRareCharm * currentLuck * 0.25f);
            int currentDropChanceLegendaryCharm = Mathf.RoundToInt(dropChanceLegendaryCharm * currentLuck * 0.25f);

            if (random <= currentDropChanceCommonCharm)
            {
                Vector3 position = new Vector3(transform.position.x, 0f, transform.position.z);
                manager.DropRandomCommonCharm(position);
            }
            else if (random <= currentDropChanceRareCharm + currentDropChanceCommonCharm)
            {
                Vector3 position = new Vector3(transform.position.x, 0f, transform.position.z);
                manager.DropRandomRareCharm(position);
            }
            else if (random <= currentDropChanceLegendaryCharm + currentDropChanceRareCharm + currentDropChanceCommonCharm)
            {
                Vector3 position = new Vector3(transform.position.x, 0f, transform.position.z);
                manager.DropRandomLegendaryCharm(position);
            }
            else if (random <= dropChanceCoins + currentDropChanceLegendaryCharm + currentDropChanceRareCharm + currentDropChanceCommonCharm)
            {
                int rand = Random.Range(minAmountOfCoins, maxAmountOfCoins+1);
                rand += 2 * currentLuck;

                Vector3 position = new Vector3(transform.position.x, 0f, transform.position.z);
                manager.DropCoins(rand, position);
            }
        }
    }
}