using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using UnityEngine;
using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        public int currencyAmount = 0;
        public int currentLuck = 0;
        public GameObject maskSlot;

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

        public void EquipMask(GameObject mask)
        {
            Instantiate(mask, maskSlot.transform);
        }
    }
}