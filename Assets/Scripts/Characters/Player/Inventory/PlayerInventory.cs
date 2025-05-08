using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.UI;
using System.Collections.Generic;
using ProjectColombo.Objects.Masks;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects;


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
        public GameObject legendaryCharmSlot;
        public List<GameObject> charms;
        public List<GameObject> legendaryCharms;
        public int maxCharms;
        int currentCharmAmount;

        public GameObject maskAbilitySlot;
        public int numberOfPotions = 0;
        public GameObject potionSlot;
        public GameObject legendaryCharmAbilitySlot;

        DropManager dropManager;


        private void Start()
        {
            CustomEvents.OnCharmCollected += AddCharm;
            CustomEvents.OnLevelChange += LevelChange;
            CustomEvents.OnEchoUnlocked += EnableMaskAbility;
            dropManager = GameManager.Instance.GetComponent<DropManager>();
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            charmSelectScreen.SetActive(false);
            GetCurrentStats();
            currentCharmAmount = 0;
        }

        private void EnableMaskAbility()
        {
            GameObject mask = maskSlot.transform.GetChild(0).gameObject;
            mask.GetComponent<BaseMask>().abilityObject = Instantiate(mask.GetComponent<BaseMask>().GetAbility(), maskAbilitySlot.transform);
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
            GameObject charmobj;

            if (charm.scene.IsValid())
            {
                //Debug.Log("Charm is already instantiated in the scene.");
                charmobj = charm;
            }
            else
            {
                charmobj = Instantiate(charm);
            }

            BaseCharm charmComponent = charmobj.GetComponent<BaseCharm>();
            RARITY newCharmRarity = charmComponent.charmRarity;

            //if legendary just drop what is in the legendary slot
            if (newCharmRarity == RARITY.LEGENDARY)
            {
                if (legendaryCharmSlot.transform.childCount > 0)
                {
                    RemoveCharm(legendaryCharmSlot.transform.GetChild(0).gameObject);
                }

                //add ability
                if (charmComponent.GetAbility() != null)
                {
                    charmComponent.abilityObject = Instantiate(charmComponent.GetAbility(), legendaryCharmAbilitySlot.transform);
                }

                // Add legendary directly
                charmobj.transform.SetParent(legendaryCharmSlot.transform);

                charmComponent.Equip();
                currentCharmAmount++;
                legendaryCharms.Add(charmobj);
            }
            else
            {
                //if already carrying max amount open select screen
                if (currentCharmAmount >= maxCharms)
                {
                    OpenCharmSelectScreen(charmobj);
                    return;
                }
                else
                {
                    //fill the other slots
                    if (charmSlot.transform.childCount < maxCharms - 1)
                    {
                        charmobj.transform.SetParent(charmSlot.transform);
                        charmComponent.Equip();
                        currentCharmAmount++;
                        charms.Add(charmobj);
                    }
                    else if (legendaryCharmSlot.transform.childCount == 0) //check if legendary slot is full
                    {
                        charmobj.transform.SetParent(legendaryCharmSlot.transform);
                        charmComponent.Equip();
                        currentCharmAmount++;
                        legendaryCharms.Add(charmobj);
                    }
                    else
                    {
                        OpenCharmSelectScreen(charmobj);
                        return;
                    }
                }

            }
        }



        public void RemoveCharm(GameObject charm)
        {
            charm.GetComponent<BaseCharm>().Remove();
            charm.transform.parent = null;

            if (charms.Contains(charm))
            {
                charms.Remove(charm);
            }            
            else if (legendaryCharms.Contains(charm))
            {
                legendaryCharms.Remove(charm);
            }

            currentCharmAmount--;

            //remove ability
            if (charm.GetComponent<BaseCharm>().GetAbility() != null)
            {
                Destroy(charm.GetComponent<BaseCharm>().GetAbility());
            }

            Transform player = GameObject.Find("Player").transform;
            Vector3 position = new Vector3(player.position.x, 0f, player.position.z);
            dropManager.DropCharm(charm.GetComponent<BaseCharm>(), position);

            Destroy(charm);
        }


        public void ReplaceCharm(GameObject charmToRemove, GameObject charmToAdd)
        {
            RemoveCharm(charmToRemove);
            AddCharm(charmToAdd);
        }

        public void OpenCharmSelectScreen(GameObject charm)
        {
            GameManager.Instance.PauseGame(false);
            charmSelectScreen.SetActive(true);
            charmSelectScreen.GetComponent<CharmSelectScreen>().ActivateScreen(charm);
        }
        
        public void ActivateMask()
        {
            if (maskSlot.transform.childCount == 0)
            {
                Debug.Log("No mask equipped");
                return;
            }

            maskSlot.transform.GetChild(0).gameObject.GetComponent<BaseMask>().Equip();
        }

        void DeactivateMask()
        {
            if (maskSlot.transform.childCount == 0)
            {
                Debug.Log("No mask equipped");
                return;
            }

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

        public void UsePotion()
        {
            if (numberOfPotions >= 0)
            {
                numberOfPotions--;
                potionSlot.GetComponentInChildren<BaseAbility>().Activate();
            }
        }

        public void UseCharmAbility()
        {
            if (legendaryCharmAbilitySlot.transform.childCount > 0)
            {
                legendaryCharmAbilitySlot.GetComponentInChildren<BaseAbility>().Activate();
            }

        }

        public void UseMaskAbility()
        {
            if (maskAbilitySlot.transform.childCount > 0)
            {
                maskAbilitySlot.GetComponentInChildren<BaseAbility>().Activate();
            }
        }
    }
}