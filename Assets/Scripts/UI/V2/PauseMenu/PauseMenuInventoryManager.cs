using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Masks;
using ProjectColombo.GameManagement;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryManager : MonoBehaviour
    {
        [Header("Inventory References")]
        [SerializeField] PauseMenuInventoryTabController inventoryTabController;
        [SerializeField] PlayerInventory playerInventory;

        [Header("Slot References")]
        [SerializeField] Button weaponSlotButton; // Slot 0
        [SerializeField] Button maskSlotButton;   // Slot 1
        [SerializeField] CharmButton[] charmButtons; // Slots 3-6 in the inventory array
        [SerializeField] CharmButton legendaryCharmButton; //Slot 2
        [SerializeField] Button potionSlotButton; // Slot 7

        [Header("Slot Images")]
        [SerializeField] Image weaponSlotImage;
        [SerializeField] Image maskSlotImage;
        [SerializeField] Image potionSlotImage;
        [SerializeField] TextMeshProUGUI potionCountText;

        [Header("Empty Slot Sprites")]
        [SerializeField] Sprite emptyCharmSprite;
        [SerializeField] Sprite emptyPotionSprite;
        [SerializeField] Sprite emptyLegendaryCharmSprite;

        [Header("Text Elements")]
        [SerializeField] TextMeshProUGUI charmTitleText;
        [SerializeField] TextMeshProUGUI charmDescriptionText;

        [Header("Empty Slot Messages")]
        [SerializeField] string emptyWeaponMessage = "No weapon equipped";
        [SerializeField] string emptyMaskMessage = "No mask equipped";
        [SerializeField] string emptyCharmMessage = "No charm equipped";
        [SerializeField] string emptyPotionMessage = "No potion available";

        [Header("Rarity Colors")]
        [SerializeField] Color defaultTextColor = Color.white;
        [SerializeField] Color commonRarityColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] Color rareRarityColor = new Color(0.0f, 0.5f, 1.0f);
        [SerializeField] Color legendaryRarityColor = new Color(1.0f, 0.84f, 0.0f);

        [Header("Mask Colors")]
        [SerializeField] Color normalMaskColor = new Color(0.6f, 0.8f, 1.0f);
        [SerializeField] Color echoMaskColor = new Color(1.0f, 0.6f, 0.8f);
        [SerializeField] Color weaponTextColor;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;


        bool isInitialized = false;


        Sprite originalMaskSlotSprite;


        public Color GetRarityColor(RARITY rarity)
        {
            switch (rarity)
            {
                case RARITY.COMMON:
                    return commonRarityColor;
                case RARITY.RARE:
                    return rareRarityColor;
                case RARITY.LEGENDARY:
                    return legendaryRarityColor;
                default:
                    return defaultTextColor;
            }
        }

        public Color GetMaskColor(bool echoUnlocked)
        {
            return echoUnlocked ? echoMaskColor : normalMaskColor;
        }

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void OnEnable()
        {
            LogDebug("Inventory Manager enabled");
            UpdateInventoryDisplay();
            SetupEventTriggers();
        }

        void Initialize()
        {
            LogDebug("Initializing Inventory Manager");

            if (playerInventory == null)
            {
                if (GameManager.Instance != null)
                {
                    playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                    LogDebug("Found PlayerInventory from GameManager");
                }

                if (playerInventory == null)
                {
                    playerInventory = FindFirstObjectByType<PlayerInventory>();
                    LogDebug("Found PlayerInventory in scene");
                }
            }

            if (maskSlotImage != null)
            {
                originalMaskSlotSprite = maskSlotImage.sprite;
                LogDebug("Stored original mask slot sprite");
            }

            if (charmTitleText != null)
            {
                charmTitleText.text = "";
                charmTitleText.color = defaultTextColor;
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.isPauseMenuButton = true;
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "";
            }

            isInitialized = true;
        }

        void SetupEventTriggers()
        {
            SetupButtonEventTrigger(weaponSlotButton, () => ShowEmptyWeaponInfo());
            SetupButtonEventTrigger(maskSlotButton, () => ShowMaskInfo());
            SetupButtonEventTrigger(potionSlotButton, () => ShowPotionInfo());
        }

        void SetupButtonEventTrigger(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();

            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }

            eventTrigger.triggers.Clear();

            EventTrigger.Entry selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };

            selectEntry.callback.AddListener((data) => action());
            eventTrigger.triggers.Add(selectEntry);

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };

            pointerEnterEntry.callback.AddListener((data) => action());
            eventTrigger.triggers.Add(pointerEnterEntry);
        }

        public void UpdateInventoryDisplay()
        {
            if (playerInventory == null)
            {
                LogDebug("Player inventory reference is missing!", true);
                return;
            }

            LogDebug("Updating inventory display");

            if (weaponSlotImage != null)
            {
            }

            if (maskSlotImage != null && playerInventory.maskSlot != null)
            {
                bool hasMask = playerInventory.maskSlot.transform.childCount > 0;

                if (hasMask)
                {
                    GameObject maskObject = playerInventory.maskSlot.transform.GetChild(0).gameObject;
                    BaseMask maskComponent = maskObject.GetComponent<BaseMask>();

                    if (maskComponent != null && maskComponent.maskPicture != null)
                    {
                        maskSlotImage.sprite = maskComponent.maskPicture;
                        maskSlotImage.enabled = true;
                        LogDebug($"Updated mask slot image on startup: {maskComponent.maskName}");
                    }
                    else
                    {
                        if (originalMaskSlotSprite != null)
                        {
                            maskSlotImage.sprite = originalMaskSlotSprite;
                            maskSlotImage.enabled = true;
                        }
                        else
                        {
                            maskSlotImage.enabled = false;
                        }
                        LogDebug("Mask found but no picture available, using original sprite");
                    }
                }
                else
                {
                    if (originalMaskSlotSprite != null)
                    {
                        maskSlotImage.sprite = originalMaskSlotSprite;
                        maskSlotImage.enabled = true;
                        LogDebug("No mask equipped, showing original sprite");
                    }
                    else
                    {
                        maskSlotImage.enabled = false;
                        LogDebug("No mask equipped and no original sprite");
                    }
                }
            }

            UpdatePotionSlotDisplay();
            UpdatePotionCountDisplay();
            UpdateCharmSlotsWithEmptySprites();
            //UpdateCharmSlots();
        }

        void UpdatePotionSlotDisplay()
        {
            if (potionSlotImage != null)
            {
                bool hasPotion = playerInventory.numberOfPotions > 0;

                LogDebug($"Potion check - numberOfPotions: {playerInventory.numberOfPotions}");

                if (hasPotion && playerInventory.potionPrefab != null)
                {
                    BaseCharm potionCharmComponent = playerInventory.potionPrefab.GetComponent<BaseCharm>();

                    if (potionCharmComponent != null && potionCharmComponent.charmPicture != null)
                    {
                        potionSlotImage.sprite = potionCharmComponent.charmPicture;
                        potionSlotImage.enabled = true;
                        LogDebug($"Updated potion slot image from prefab: {potionCharmComponent.charmName}");
                    }
                    else if (emptyPotionSprite != null)
                    {
                        potionSlotImage.sprite = emptyPotionSprite;
                        potionSlotImage.enabled = true;
                        LogDebug("Potion prefab found but no picture, showing empty potion sprite");
                    }
                    else
                    {
                        potionSlotImage.enabled = false;
                        LogDebug("Potion prefab found but no picture and no empty sprite");
                    }
                }
                else
                {
                    if (emptyPotionSprite != null)
                    {
                        potionSlotImage.sprite = emptyPotionSprite;
                        potionSlotImage.enabled = true;
                        LogDebug("No potions available, showing empty potion sprite");
                    }
                    else
                    {
                        potionSlotImage.enabled = false;
                        LogDebug("No potions available and no empty sprite assigned");
                    }
                }
            }
        }

        void UpdateCharmSlotsWithEmptySprites()
        {
            if (charmButtons != null)
            {
                foreach (CharmButton button in charmButtons)
                {
                    if (button != null)
                    {
                        button.UpdateInfoWithEmptySprite(null, emptyCharmSprite);
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.UpdateInfoWithEmptySprite(null, emptyLegendaryCharmSprite);
            }

            if (playerInventory == null)
            {
                return;
            }

            if (charmButtons != null && playerInventory.charms != null)
            {
                for (int i = 0; i < playerInventory.charms.Count && i < charmButtons.Length; i++)
                {
                    if (charmButtons[i] != null && playerInventory.charms[i] != null)
                    {
                        charmButtons[i].UpdateInfoWithEmptySprite(playerInventory.charms[i], emptyCharmSprite);
                    }
                }
            }

            if (legendaryCharmButton != null && playerInventory.legendaryCharms != null && playerInventory.legendaryCharms.Count > 0)
            {
                legendaryCharmButton.UpdateInfoWithEmptySprite(playerInventory.legendaryCharms[0], emptyLegendaryCharmSprite);
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.UpdateAllCharmButtonColors();
                LogDebug("Updated all charm button colors after charm slot update");
            }
        }



        public void ShowMaskInfo()
        {
            if (playerInventory == null || playerInventory.maskSlot == null)
            {
                ShowEmptyMaskInfo();
                return;
            }

            bool hasMask = playerInventory.maskSlot.transform.childCount > 0;

            if (!hasMask)
            {
                ShowEmptyMaskInfo();
                return;
            }

            GameObject maskObject = playerInventory.maskSlot.transform.GetChild(0).gameObject;
            BaseMask maskComponent = maskObject.GetComponent<BaseMask>();

            if (maskComponent == null)
            {
                LogDebug("Selected mask object doesn't have a BaseMask component!", true);
                ShowEmptyMaskInfo();
                return;
            }

            if (charmTitleText != null)
            {
                charmTitleText.text = maskComponent.maskName;
                charmTitleText.color = GetMaskColor(maskComponent.echoUnlocked);
            }

            if (charmDescriptionText != null)
            {
                if (maskComponent.echoUnlocked && !string.IsNullOrEmpty(maskComponent.echoDescription))
                {
                    charmDescriptionText.text = maskComponent.echoDescription;
                }
                else
                {
                    charmDescriptionText.text = maskComponent.maskDescription;
                }
            }

            if (maskSlotImage != null)
            {
                if (maskComponent.maskPicture != null)
                {
                    maskSlotImage.sprite = maskComponent.maskPicture;
                    maskSlotImage.enabled = true;
                    LogDebug($"Updated mask slot image: {maskComponent.maskName}");
                }
                else
                {
                    maskSlotImage.enabled = false;
                    LogDebug($"Mask {maskComponent.maskName} has no picture assigned");
                }
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.UpdateSelectorColorForMask(maskComponent);
            }

            LogDebug($"Updated mask info display for: {maskComponent.maskName} (Echo Unlocked: {maskComponent.echoUnlocked})");
        }

        public void UpdateCharmInfo(GameObject charmObject)
        {
            if (charmObject == null)
            {
                ShowEmptyCharmInfo();
                return;
            }

            BaseCharm charmInfo = charmObject.GetComponent<BaseCharm>();

            if (charmInfo == null)
            {
                LogDebug("Selected object doesn't have a BaseCharm component!", true);
                ShowEmptyCharmInfo();
                return;
            }

            if (charmTitleText != null)
            {
                string rarityText = GetRarityDisplayName(charmInfo.charmRarity);
                string fullTitle = $"{charmInfo.charmName} ({rarityText})";

                charmTitleText.text = fullTitle;
                charmTitleText.color = GetRarityColor(charmInfo.charmRarity);

                LogDebug($"Set charm title to: {fullTitle}");
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = charmInfo.charmDescription;
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.UpdateSelectorColorForCharm(charmInfo);
            }

            LogDebug($"Updated charm info display for: {charmInfo.charmName} (Rarity: {charmInfo.charmRarity})");
        }

        string GetRarityDisplayName(RARITY rarity)
        {
            switch (rarity)
            {
                case RARITY.COMMON:
                    return "Common";
                case RARITY.RARE:
                    return "Rare";
                case RARITY.LEGENDARY:
                    return "Legendary";
                default:
                    return "Unknown";
            }
        }

        public void ShowEmptyWeaponInfo()
        {
            if (charmTitleText != null)
            {
                charmTitleText.text = "Musician's Stoccoviola";
                charmTitleText.color = weaponTextColor;
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "Musicians are well-versed in the finest arts esteemed by nobility. This weapon is one of the Musician's trusted companions along their journey, able to pierce the toughest armor while composing exquisite melodies in equal measure.";
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.ResetSelectorColor();
            }

            LogDebug("Showing Musician's Stoccoviola weapon info");
        }

        public void ShowEmptyMaskInfo()
        {
            if (charmTitleText != null)
            {
                charmTitleText.text = emptyMaskMessage;
                charmTitleText.color = defaultTextColor;
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "";
            }

            if (maskSlotImage != null)
            {
                if (originalMaskSlotSprite != null)
                {
                    maskSlotImage.sprite = originalMaskSlotSprite;
                    maskSlotImage.enabled = true;
                    LogDebug("Restored original mask slot sprite");
                }
                else
                {
                    maskSlotImage.enabled = false;
                    LogDebug("No original mask sprite to restore, hiding image");
                }
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.ResetSelectorColor();
            }

            LogDebug("Showing empty mask info");
        }

        public void ShowEmptyCharmInfo()
        {
            if (charmTitleText != null)
            {
                charmTitleText.text = emptyCharmMessage;
                charmTitleText.color = defaultTextColor;
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "";
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.ResetSelectorColor();
            }

            LogDebug("Showing empty charm info");
        }

        public void ShowEmptyPotionInfo()
        {
            if (charmTitleText != null)
            {
                charmTitleText.text = emptyPotionMessage;
                charmTitleText.color = defaultTextColor;
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "";
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.ResetSelectorColor();
            }

            LogDebug("Showing empty potion info");
        }

        public void ShowPotionInfo()
        {
            if (playerInventory == null)
            {
                ShowEmptyPotionInfo();
                return;
            }

            bool hasPotion = playerInventory.numberOfPotions > 0;

            LogDebug($"ShowPotionInfo - hasPotion: {hasPotion}");

            if (!hasPotion)
            {
                ShowEmptyPotionInfo();
                return;
            }

            if (playerInventory.potionPrefab == null)
            {
                LogDebug("No potion prefab available");
                ShowEmptyPotionInfo();
                return;
            }

            BaseCharm potionCharmComponent = playerInventory.potionPrefab.GetComponent<BaseCharm>();

            if (potionCharmComponent == null)
            {
                LogDebug("Potion prefab doesn't have a BaseCharm component!", true);
                ShowEmptyPotionInfo();
                return;
            }

            if (charmTitleText != null)
            {
                string potionCountText = playerInventory.numberOfPotions > 1 ? $" (x{playerInventory.numberOfPotions})" : "";
                charmTitleText.text = potionCharmComponent.charmName + potionCountText;
                charmTitleText.color = GetRarityColor(potionCharmComponent.charmRarity);
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = potionCharmComponent.charmDescription;
            }

            if (potionSlotImage != null)
            {
                if (potionCharmComponent.charmPicture != null)
                {
                    potionSlotImage.sprite = potionCharmComponent.charmPicture;
                    potionSlotImage.enabled = true;
                    LogDebug($"Updated potion slot image from prefab: {potionCharmComponent.charmName}");
                }
                else
                {
                    potionSlotImage.enabled = false;
                    LogDebug($"Potion {potionCharmComponent.charmName} has no picture assigned");
                }
            }

            if (inventoryTabController != null)
            {
                inventoryTabController.UpdateSelectorColorForCharm(potionCharmComponent);
            }

            LogDebug($"Updated potion info display for: {potionCharmComponent.charmName} (Count: {playerInventory.numberOfPotions})");
        }

        void UpdatePotionCountDisplay()
        {
            if (potionCountText == null || playerInventory == null)
            {
                return;
            }

            bool hasPotion = playerInventory.numberOfPotions > 0;

            if (hasPotion)
            {
                potionCountText.text = playerInventory.numberOfPotions.ToString();
                potionCountText.gameObject.SetActive(true);
                LogDebug($"Updated potion count display: {playerInventory.numberOfPotions}");
            }
            else
            {
                potionCountText.gameObject.SetActive(false);
                LogDebug("Hidden potion count display (0 potions)");
            }
        }

        public void UpdatePotionDisplay()
        {
            if (playerInventory == null)
            {
                return;
            }

            UpdatePotionSlotDisplay();
            UpdatePotionCountDisplay();

            LogDebug($"Updated potion display - Count: {playerInventory.numberOfPotions}");
        }

        void LogDebug(string message, bool isWarning = false)
        {
            if (enableDebugLogs)
            {
                if (isWarning)
                {
                    Debug.LogWarning($"[PauseMenuInventoryManager] {message}");

                }
                else
                {
                    Debug.Log($"<color=#FF9900>[PauseMenuInventoryManager] {message}</color>");
                }
            }
        }
    }
}