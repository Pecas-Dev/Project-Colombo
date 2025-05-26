using UnityEngine;
using ProjectColombo.UI;
using ProjectColombo.Shop;
using System.Collections.Generic;
using ProjectColombo.Objects.Masks;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement.Events;


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

        //for when charms in shop
        [HideInInspector] public bool inShop = false;
        [HideInInspector] public ShopKeeper currentShopKeeper;

        bool hasInitialized = false;

        DropManager dropManager;


        private void Start()
        {
            CustomEvents.OnCharmCollected += AddCharm;
            CustomEvents.OnLevelChange += LevelChange;
            CustomEvents.OnShopOpen += ShopOpened;
            CustomEvents.OnShopClose += ShopClosed;
            CustomEvents.OnEchoUnlocked += EnableMaskAbility;
            dropManager = GameManager.Instance.GetComponent<DropManager>();
            myGlobalStats = GameManager.Instance.GetComponent<GlobalStats>();

            if (charmSelectScreen != null)
            {
                charmSelectScreen.SetActive(false);
            }

            GetCurrentStats();
            currentCharmAmount = 0;

            hasInitialized = true;
            CheckSystemPreference();
        }

        private void ShopClosed()
        {
            inShop = false;
            currentShopKeeper = null;
        }

        private void ShopOpened(Shop.ShopKeeper shopkeeper)
        {
            inShop = true;
            currentShopKeeper = shopkeeper;
        }

        private void EnableMaskAbility()
        {
            GameObject mask = maskSlot.transform.GetChild(0).gameObject;
            mask.GetComponent<BaseMask>().abilityObject = Instantiate(mask.GetComponent<BaseMask>().GetAbility(), maskAbilitySlot.transform);
        }

        public void AddLuckPoints(int amount)
        {
            currentLuck = Mathf.Clamp(currentLuck + amount, 0, 20);
        }


        public void Reset()
        {
            DeactivateMask();
            DeactivateCharms();

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

            GetCurrentStats();
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
            if (myGlobalStats == null)
            {
                myGlobalStats = GetComponent<GlobalStats>();
            }

            currencyAmount = myGlobalStats.currentCurrencyAmount;
            currentLuck = myGlobalStats.currentLuckPoints;
        }

        public void EquipMask(GameObject mask)
        {
            Instantiate(mask, maskSlot.transform);
        }

        public void AddCharm(GameObject charm)
        {
            /*GameObject charmObject;

            if (charm.scene.IsValid())
            {
                charmObject = charm;
            }
            else
            {
                charmObject = Instantiate(charm);
            }

            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            RARITY newCharmRarity = charmComponent.charmRarity;

            if (newCharmRarity == RARITY.LEGENDARY)
            {
                if (legendaryCharmSlot.transform.childCount > 0)
                {
                    RemoveCharm(legendaryCharmSlot.transform.GetChild(0).gameObject);
                }

                if (charmComponent.GetAbility() != null)
                {
                    charmComponent.abilityObject = Instantiate(charmComponent.GetAbility(), legendaryCharmAbilitySlot.transform);
                }

                charmObject.transform.SetParent(legendaryCharmSlot.transform);
                charmComponent.Equip();
                currentCharmAmount++;
                legendaryCharms.Add(charmObject);
            }
            else
            {
                if (currentCharmAmount >= maxCharms)
                {
                    OpenCharmSelectScreen(charmObject);
                    return;
                }
                else
                {
                    if (charmSlot.transform.childCount < maxCharms - 1)
                    {
                        charmObject.transform.SetParent(charmSlot.transform);
                        charmComponent.Equip();
                        currentCharmAmount++;
                        charms.Add(charmObject);
                    }
                    else if (legendaryCharmSlot.transform.childCount == 0)
                    {
                        charmObject.transform.SetParent(legendaryCharmSlot.transform);
                        charmComponent.Equip();
                        currentCharmAmount++;
                        legendaryCharms.Add(charmObject);
                    }
                    else
                    {
                        OpenCharmSelectScreen(charmObject);
                        return;
                    }
                }
            }*/

            GameObject charmObject;

            if (charm.scene.IsValid())
            {
                Debug.Log("charm not instantiated -> good");
                charmObject = charm;
            }
            else
            {
                Debug.Log("charm instatiated -> bad (except shop");
                charmObject = Instantiate(charm);
            }

            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            RARITY newCharmRarity = charmComponent.charmRarity;

            //Debug.Log($"AddCharm called: {charmComponent.charmName} (Rarity: {newCharmRarity})");
            //Debug.Log($"Current inventory state - Amount: {currentCharmAmount}/{maxCharms}, HasLegendary: {HasLegendaryCharmEquipped()}");

            if (currentCharmAmount >= maxCharms)
            {
                Debug.Log("Inventory is full, checking for legendary replacement scenarios...");

                // SCENARIO 5: Inventory Full + Have Legendary + Find Legendary = LEGENDARY MODE
                if (newCharmRarity == RARITY.LEGENDARY && HasLegendaryCharmEquipped())
                {
                    Debug.Log("SCENARIO 5: Full inventory + Legendary equipped + New legendary = LEGENDARY MODE");
                    OpenCharmSelectScreenLegendaryMode(charmObject);
                    return;
                }
                // SCENARIO 4 & 6: Inventory Full + Any other combination = NORMAL MODE
                else
                {
                    Debug.Log($"SCENARIO 4/6: Full inventory + Other combination = NORMAL MODE (New: {newCharmRarity}, HasLegendary: {HasLegendaryCharmEquipped()})");
                    OpenCharmSelectScreen(charmObject);
                    return;
                }
            }

            if (newCharmRarity == RARITY.LEGENDARY)
            {
                // SCENARIO 2: Not Full + Have Legendary + Find Legendary = LEGENDARY MODE
                if (HasLegendaryCharmEquipped())
                {
                    Debug.Log("SCENARIO 2: Not full + Legendary equipped + New legendary = LEGENDARY MODE");
                    OpenCharmSelectScreenLegendaryMode(charmObject);
                    return;
                }
                // SCENARIO 1 & 3: Not Full + No Legendary OR Non-Legendary in slot = Direct Add
                else
                {
                    Debug.Log("SCENARIO 1/3: Not full + No legendary OR non-legendary in slot = Direct add legendary");

                    if (legendaryCharmSlot.transform.childCount > 0)
                    {
                        GameObject oldCharm = legendaryCharmSlot.transform.GetChild(0).gameObject;
                        Debug.Log($"Removing non-legendary charm from legendary slot: {oldCharm.GetComponent<BaseCharm>().charmName}");
                        RemoveCharm(oldCharm);
                    }

                    if (charmComponent.GetAbility() != null)
                    {
                        charmComponent.abilityObject = Instantiate(charmComponent.GetAbility(), legendaryCharmAbilitySlot.transform);
                    }

                    charmObject.transform.SetParent(legendaryCharmSlot.transform);
                    charmComponent.Equip();
                    currentCharmAmount++;
                    legendaryCharms.Add(charmObject);
                    Debug.Log("Legendary charm added directly to legendary slot");
                }
            }
            else
            {
                // Regular charm (Common/Rare) - SCENARIO 1: Not Full + Add to available slot
                Debug.Log("SCENARIO 1: Not full + Regular charm = Add to available slot");

                if (charmSlot.transform.childCount < maxCharms - 1)
                {
                    charmObject.transform.SetParent(charmSlot.transform);
                    charmComponent.Equip();
                    currentCharmAmount++;
                    charms.Add(charmObject);
                    Debug.Log("Regular charm added to regular slot");
                }
                else if (legendaryCharmSlot.transform.childCount == 0)
                {
                    charmObject.transform.SetParent(legendaryCharmSlot.transform);
                    charmComponent.Equip();
                    currentCharmAmount++;
                    charms.Add(charmObject);
                    Debug.Log("Regular charm added to legendary slot");
                }
                else
                {
                    Debug.LogWarning("Unexpected state: inventory not full but no slots available!");
                    OpenCharmSelectScreen(charmObject);
                }
            }

            Debug.Log($"Final inventory state - Amount: {currentCharmAmount}/{maxCharms}");
        }

        bool HasLegendaryCharmEquipped()
        {
            if (legendaryCharmSlot.transform.childCount == 0)
            {
                return false;
            }

            GameObject equippedCharm = legendaryCharmSlot.transform.GetChild(0).gameObject;
            if (equippedCharm == null)
            {
                return false;
            }

            BaseCharm charmComponent = equippedCharm.GetComponent<BaseCharm>();

            if (charmComponent == null)
            {
                return false;
            }

            bool isLegendary = charmComponent.charmRarity == RARITY.LEGENDARY;

            Debug.Log($"HasLegendaryCharmEquipped: {isLegendary} (Charm: {charmComponent.charmName}, Rarity: {charmComponent.charmRarity})");

            return isLegendary;
        }

        GameObject GetEquippedLegendaryCharm()
        {
            if (!HasLegendaryCharmEquipped())
            {
                return null;
            }

            return legendaryCharmSlot.transform.GetChild(0).gameObject;
        }

        public void OpenCharmSelectScreenLegendaryMode(GameObject newLegendaryCharm)
        {
            if (inShop)
            {
                currentShopKeeper.CloseShopScreen();
            }

            GameManager manager = GameManager.Instance;

            if (manager != null)
            {
                CharmSwapMenuController swapController = manager.CharmSwapMenuCtrl;

                if (swapController != null)
                {
                    GameManager.Instance.PauseGame(false);
                    swapController.ActivateScreenLegendaryMode(newLegendaryCharm);
                }
                else
                {
                    Debug.LogError("CharmSwapMenuController not found! Make sure it's properly setup.");
                }
            }
            else
            {
                Debug.LogError("GameManager instance not found!");
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
            dropManager.DropCharm(charm, position);

            //Destroy(charm);
        }


        public void ReplaceCharm(GameObject charmToRemove, GameObject charmToAdd)
        {
            RemoveCharm(charmToRemove);
            AddCharm(charmToAdd);
        }

        public void OpenCharmSelectScreen(GameObject newCharm)
        {
            if (inShop)
            {
                currentShopKeeper.CloseShopScreen();
            }

            GameManager manager = GameManager.Instance;

            if (manager != null)
            {
                CharmSwapMenuController swapController = manager.CharmSwapMenuCtrl;

                if (swapController != null)
                {
                    GameManager.Instance.PauseGame(false);
                    swapController.ActivateScreen(newCharm);
                }
                else
                {
                    Debug.LogError("CharmSwapMenuController not found! Make sure it's properly setup.");
                }
            }
            else
            {
                Debug.LogError("GameManager instance not found!");
            }
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
            if (numberOfPotions > 0)
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

        void CheckSystemPreference()
        {
            if (!hasInitialized)
            {
                return;
            }

            GameManager manager = GameManager.Instance;

            if (manager == null)
            {
                return;
            }

            if (charmSelectScreen != null)
            {
                charmSelectScreen.SetActive(false);
            }
        }
    }
}