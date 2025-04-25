using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory.Collectable;
using System.Collections.Generic;
using UnityEngine;
using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        public int currencyAmount = 0;
        public int currentLuck = 0;

        private void Start()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            GetCurrentStats();
        }

        private void OnDestroy()
        {
            CustomEvents.OnLevelChange -= SaveCurrentStats;
        }

        private void SaveCurrentStats()
        {
            myGlobalStats.currentCurrencyAmount = currencyAmount;
            myGlobalStats.currentLuckPoints = currentLuck;
        }

        void GetCurrentStats()
        {
            currencyAmount = myGlobalStats.currentCurrencyAmount;
            currentLuck = myGlobalStats.currentLuckPoints;
        }
    }
}