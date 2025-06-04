using UnityEngine;
using ProjectColombo.UI;
using ProjectColombo.Shop;
using System.Collections.Generic;
using ProjectColombo.Objects.Masks;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement.Events;
using UnityEngine.SceneManagement;
using ProjectColombo.UI.Pausescreen;


namespace ProjectColombo.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Pick Up Screen System")]
        [SerializeField] GameObject pickUpCanvas;

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


        [SerializeField] public GameObject potionPrefab;

        public GameObject maskAbilitySlot;
        public GameObject potionSlot;
        public int numberOfPotions = 0;
        public GameObject legendaryCharmAbilitySlot;

        //for when charms in shop
        [HideInInspector] public bool inShop = false;
        [HideInInspector] public ShopKeeper currentShopKeeper;

        bool hasInitialized = false;

        DropManager dropManager;
        PickUpScreenController pickUpScreenController;


        void Awake()
        {
            //SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            CustomEvents.OnCharmCollected += AddCharm;
            CustomEvents.OnLevelChange += LevelChange;
            CustomEvents.OnShopOpen += ShopOpened;
            CustomEvents.OnShopClose += ShopClosed;
            CustomEvents.OnEchoUnlocked += EnableMaskAbility;
            CustomEvents.OnGameReset += Reset;
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

            if (pickUpCanvas == null)
            {
                pickUpCanvas = GameObject.Find("PickUpCanvas");
                if (pickUpCanvas != null)
                {
                    Transform pickUpContainerTransform = pickUpCanvas.transform.Find("PickUpContainer");
                    if (pickUpContainerTransform != null)
                    {
                        pickUpScreenController = pickUpContainerTransform.GetComponent<PickUpScreenController>();
                    }
                }
            }

            InitializePotionInPauseInventory();
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

            foreach (Transform child in maskAbilitySlot.transform)
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
            CustomEvents.OnShopOpen -= ShopOpened;
            CustomEvents.OnShopClose -= ShopClosed;
            CustomEvents.OnEchoUnlocked -= EnableMaskAbility;
            CustomEvents.OnGameReset -= Reset;

            //SceneManager.sceneLoaded -= OnSceneLoaded;
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
            GameObject charmObject;

            if (charm.scene.IsValid())
            {
                Debug.Log("charm not instantiated -> good");
                charmObject = charm;
            }
            else
            {
                Debug.Log("charm instatiated -> bad (except shop)");
                charmObject = Instantiate(charm);
            }

            if (IsTutorialScene())
            {
                Debug.Log("Tutorial scene detected - bypassing pickup screen and adding charm directly");
                AddCharmDirectlyWithLogic(charmObject);
                return;
            }

            ShowPickUpScreen(charmObject);
        }

        bool IsTutorialScene()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

            return currentSceneName == "01_Tutorial" || currentSceneIndex == 1;
        }

        void AddCharmDirectlyWithLogic(GameObject charmObject)
        {
            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            RARITY newCharmRarity = charmComponent.charmRarity;

            Debug.Log($"AddCharmDirectlyWithLogic called: {charmComponent.charmName} (Rarity: {newCharmRarity})");
            Debug.Log($"Current inventory state - Amount: {currentCharmAmount}/{maxCharms}, HasLegendary: {HasLegendaryCharmEquipped()}");

            if (currentCharmAmount >= maxCharms)
            {
                Debug.Log("Inventory is full, checking for legendary replacement scenarios...");

                if (newCharmRarity == RARITY.LEGENDARY && HasLegendaryCharmEquipped())
                {
                    Debug.Log("SCENARIO 5: Full inventory + Legendary equipped + New legendary = LEGENDARY MODE");
                    OpenCharmSelectScreenLegendaryMode(charmObject);
                    return;
                }
                else
                {
                    Debug.Log($"SCENARIO 4/6: Full inventory + Other combination = NORMAL MODE (New: {newCharmRarity}, HasLegendary: {HasLegendaryCharmEquipped()})");
                    OpenCharmSelectScreen(charmObject);
                    return;
                }
            }

            if (newCharmRarity == RARITY.LEGENDARY)
            {
                if (HasLegendaryCharmEquipped())
                {
                    Debug.Log("SCENARIO 2: Not full + Legendary equipped + New legendary = LEGENDARY MODE");
                    OpenCharmSelectScreenLegendaryMode(charmObject);
                    return;
                }
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

        void ShowPickUpScreen(GameObject charmObject)
        {
            if (inShop)
            {
                currentShopKeeper.CloseShopScreen();
            }

            if (pickUpScreenController != null)
            {
                GameManager.Instance.PauseGame(false);
                pickUpScreenController.ShowPickUpScreen(charmObject);
                Debug.Log($"Showing pick up screen for charm: {charmObject.GetComponent<BaseCharm>().charmName}");
            }
            else
            {
                Debug.LogError("PickUpScreenController not found! Falling back to direct add.");
                AddCharmDirectly(charmObject);
            }
        }

        public void AddCharmDirectly(GameObject charmObject)
        {
            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            RARITY newCharmRarity = charmComponent.charmRarity;

            Debug.Log($"AddCharmDirectly called: {charmComponent.charmName} (Rarity: {newCharmRarity})");
            Debug.Log($"Current inventory state - Amount: {currentCharmAmount}/{maxCharms}, HasLegendary: {HasLegendaryCharmEquipped()}");

            if (currentCharmAmount >= maxCharms)
            {
                Debug.LogError("AddCharmDirectly called with full inventory - this should not happen!");
                return;
            }

            if (newCharmRarity == RARITY.LEGENDARY)
            {
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
            else
            {
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
                }
            }

            Debug.Log($"Final inventory state - Amount: {currentCharmAmount}/{maxCharms}");
        }

        public bool HasLegendaryCharmEquipped()
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
            AddCharmDirectly(charmToAdd);
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

        public void DeactivateMask()
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

        public void DeactivateCharms()
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

                BaseAbility potionAbility = potionSlot.GetComponentInChildren<BaseAbility>();

                if (potionAbility != null)
                {
                    potionAbility.Activate();
                }
                else
                {
                    Debug.LogError("No BaseAbility found in potionSlot children!");
                }

                NotifyPotionCountChanged();
            }
        }

        public void AddPotion(int amount = 1)
        {
            numberOfPotions += amount;

            if (numberOfPotions == amount)
            {
                AddPotionToAbilitySlot();
            }

            NotifyPotionCountChanged();

            Debug.Log($"Added {amount} potion(s). Total: {numberOfPotions}");
        }

        public void AddPotionToPauseInventory()
        {
            if (potionPrefab != null && potionSlot != null && numberOfPotions > 0)
            {
                foreach (Transform child in potionSlot.transform)
                {
                    Destroy(child.gameObject);
                }

                GameObject potionDisplayObject = Instantiate(potionPrefab, potionSlot.transform);

                Debug.Log($"Added potion to pause inventory display: {potionDisplayObject.GetComponent<BaseCharm>().charmName}");
            }
        }

        public void AddPotionToAbilitySlot()
        {
            if (potionPrefab != null && potionSlot != null && numberOfPotions > 0)
            {
                foreach (Transform child in potionSlot.transform)
                {
                    Destroy(child.gameObject);
                }

                GameObject potionObject = Instantiate(potionPrefab, potionSlot.transform);
                BaseCharm potionCharm = potionObject.GetComponent<BaseCharm>();

                if (potionCharm != null && potionCharm.GetAbility() != null)
                {
                    potionCharm.abilityObject = Instantiate(potionCharm.GetAbility(), potionSlot.transform);
                }

                Debug.Log($"Added potion ability to slot: {potionCharm.charmName}");
            }
        }

        void NotifyPotionCountChanged()
        {
            PauseMenuInventoryManager pauseInventoryManager = FindFirstObjectByType<PauseMenuInventoryManager>();
            if (pauseInventoryManager != null)
            {
                pauseInventoryManager.UpdatePotionDisplay();
            }

            PlayerHUDManager hudManager = FindFirstObjectByType<PlayerHUDManager>();
            if (hudManager != null)
            {
                hudManager.UpdatePotionDisplayFromInventory();
            }

            Debug.Log($"Notified UI managers of potion count change: {numberOfPotions}");
        }

        void InitializePotionInPauseInventory()
        {
            if (numberOfPotions > 0 && potionPrefab != null)
            {
                AddPotionToPauseInventory();
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

                CustomEvents.MaskAbilityUsed();
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