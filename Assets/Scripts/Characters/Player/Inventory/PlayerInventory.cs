using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.UI;
using System.Collections.Generic;
using ProjectColombo.Objects.Masks;
using ProjectColombo.Objects.Charms;


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
            CustomEvents.OnLevelChange += LevelChange;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            charmSelectScreen.SetActive(false);
            GetCurrentStats();
            currentCharmAmount = 0;
        }

        public void Reset()
        {
            foreach (Transform child in maskSlot.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in charmSlot.transform)
            {
                Destroy(child.gameObject);
            }

            charms = new();
            currentCharmAmount = 0;
        }

        private void LevelChange()
        {
            DeactivateMask();
            DeactivateCharms();
        }

        private void OnDestroy()
        {
            CustomEvents.OnCharmCollected -= AddCharm;
            CustomEvents.OnLevelChange -= LevelChange;
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
                charmobj.GetComponent<BaseCharm>().Equip();
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
            charm.GetComponent<BaseCharm>().Remove();
            Destroy(charm);
            charms.Remove(charm);
        }


        public void ReplaceCharm(GameObject charmToRemove, GameObject charmToAdd)
        {
            charmToRemove.GetComponent<BaseCharm>().Remove();
            Destroy(charmToRemove);
            charms.Remove(charmToRemove);
            charms.Add(charmToAdd);
            charmToAdd.GetComponent<BaseCharm>().Equip();
        }

        public void OpenCharmSelectScreen(GameObject charm)
        {
            charmSelectScreen.SetActive(true);
            charmSelectScreen.GetComponent<CharmSelectScreen>().ActivateScreen(charm);
        }
        
        public void ActivateMask()
        {
            maskSlot.transform.GetChild(0).gameObject.GetComponent<BaseMask>().Equip();
        }

        void DeactivateMask()
        {
            maskSlot.transform.GetChild(0).gameObject.GetComponent<BaseMask>().Remove();
        }

        public void ActivateCharms()
        {
            foreach (GameObject charm in charms)
            {
                charm.GetComponent<BaseCharm>().Equip();
            }
        }

        void DeactivateCharms()
        {
            foreach (GameObject charm in charms)
            {
                charm.GetComponent<BaseCharm>().Remove();
            }
        }
    }
}