using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.UI;
using System.Collections.Generic;


namespace ProjectColombo.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        public int currencyAmount = 0;
        public int currentLuck = 0;
        public GameObject maskSlot;
        public GameObject charmSelectScreen;
        public GameObject charmSlot;
        public List<GameObject> charms;
        public int maxCharms;
        int currentCharmAmount;

        private void Start()
        {
            CustomEvents.OnCharmCollected += AddCharm;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            charmSelectScreen.SetActive(false);
            GetCurrentStats();
            currentCharmAmount = 0;
        }

        private void OnDestroy()
        {
            CustomEvents.OnCharmCollected -= AddCharm;
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

        public void AddCharm(GameObject charm)
        {
            GameObject charmobj = Instantiate(charm, charmSlot.transform);

            if (currentCharmAmount < maxCharms)
            {
                charms.Add(charmobj);
                currentCharmAmount++;
            }
            else
            {
                OpenCharmSelectScreen(charmobj);
                GameManager.Instance.PauseGame(false);
            }
        }


        public void RemoveCharm(GameObject charm)
        {
            Destroy(charm);
            charms.Remove(charm);
        }


        public void ReplaceCharm(GameObject charmToRemove, GameObject charmToAdd)
        {
            Destroy(charmToRemove);
            charms.Remove(charmToRemove);
            charms.Add(charmToAdd);
        }

        public void OpenCharmSelectScreen(GameObject charm)
        {
            charmSelectScreen.SetActive(true);
            charmSelectScreen.GetComponent<CharmSelectScreen>().ActivateScreen(charm);
        }
    }
}