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
        float dropChanceMask;

        public List<GameObject> commonCharms;
        public List<GameObject> rareCharms;
        public List<GameObject> legendaryCharms;
        public List<GameObject> masks;
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
                    dropChanceMask = myLevelStats.dropChanceMaskVase;
                    return;

                case Variant.CHEST:
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsChest;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsChest;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmChest;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmChest;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmChest;
                    dropChanceMask = myLevelStats.dropChanceMaskChest;
                    return;

                default:
                    minAmountOfCoins = myLevelStats.minAmountOfCoinsEnemies;
                    maxAmountOfCoins = myLevelStats.maxAmountOfCoinsEnemies;
                    dropChanceCommonCharm = myLevelStats.dropChanceCommonCharmEnemies;
                    dropChanceRareCharm = myLevelStats.dropChanceRareCharmEnemies;
                    dropChanceLegendaryCharm = myLevelStats.dropChanceLegendaryCharmEnemies;
                    dropChanceMask = myLevelStats.dropChanceMaskEnemies;
                    return;
            }
        }

        public void DropItem()
        {
            float random = Random.Range(0, 100);

            if (random <= dropChanceCommonCharm)
            {
                int rand = Random.Range(0, commonCharms.Count);
                Debug.Log("common dropped");

                Instantiate(commonCharms[rand], new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
            }
            else if (random <= dropChanceRareCharm)
            {
                int rand = Random.Range(0, rareCharms.Count);
                Debug.Log("rare dropped");

                Instantiate(rareCharms[rand], new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);

            }
            else if (random <= dropChanceLegendaryCharm)
            {
                int rand = Random.Range(0, legendaryCharms.Count);
                Debug.Log("legandary dropped");

                Instantiate(legendaryCharms[rand], new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);

            }
            else if (random <= dropChanceMask)
            {
                int rand = Random.Range(0, masks.Count);
                Debug.Log("mask dropped");

                Instantiate(masks[rand], new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
            }
            else
            {
                int rand = Random.Range(minAmountOfCoins, maxAmountOfCoins+1);

                GameObject coininstance = Instantiate(coins, new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                Debug.Log("coins dropped: " + rand);
                coininstance.GetComponent<Coins>().amount = rand;
            }
        }
    }
}