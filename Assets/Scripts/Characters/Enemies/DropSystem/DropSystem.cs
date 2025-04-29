using ProjectColombo.Inventory.Collectable;
using ProjectColombo.LevelManagement;
using ProjectColombo.Objects.Items;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Enemies.DropSystem
{
    public enum Variant { VASE = 0, CHEST, ENEMY };

    public class DropSystem : MonoBehaviour
    {
        public Variant myVariant;
        LevelStats myLevelStats;

        int minAmountOfCoins;
        int maxAmountOfCoins;
        float dropChanceCommonCharm;
        float dropChanceRareCharm;
        float dropChanceLegendaryCharm;

        public List<GameObject> commonCharms;
        public List<GameObject> rareCharms;
        public List<GameObject> legendaryCharms;
        public GameObject pickup;
        public GameObject coins;



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
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsVase;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsVase;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmVase;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmVase;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmVase;
                    return;

                case Variant.CHEST:
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsChest;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsChest;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmChest;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmChest;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmChest;
                    return;

                default:
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

            if (random <= dropChanceCommonCharm)
            {
                int rand = Random.Range(0, commonCharms.Count);

                GameObject instance = Instantiate(pickup, new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                Instantiate(commonCharms[rand], instance.transform);
            }
            else if (random <= dropChanceRareCharm + dropChanceCommonCharm)
            {
                int rand = Random.Range(0, rareCharms.Count);

                GameObject instance = Instantiate(pickup, new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                Instantiate(rareCharms[rand], instance.transform);

            }
            else if (random <= dropChanceLegendaryCharm + dropChanceRareCharm + dropChanceCommonCharm)
            {
                int rand = Random.Range(0, legendaryCharms.Count);

                GameObject instance = Instantiate(pickup, new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                Instantiate(legendaryCharms[rand], instance.transform);

            }
            else
            {
                int rand = Random.Range(minAmountOfCoins, maxAmountOfCoins+1);

                GameObject coininstance = Instantiate(coins, new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                coininstance.GetComponent<Coins>().amount = rand;
            }
        }
    }
}