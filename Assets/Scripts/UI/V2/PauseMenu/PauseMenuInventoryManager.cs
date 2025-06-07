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

        [Header("More Info Panel References")]
        [SerializeField] GameObject moreInfoPanel;
        [SerializeField] Image moreInfoItemImage;
        [SerializeField] TextMeshProUGUI moreInfoTitleText;
        [SerializeField] TextMeshProUGUI moreInfoDescriptionText;

        [Header("Mask More Info Panel References")]
        [SerializeField] TextMeshProUGUI moreInfoMaskDescriptionText;
        [SerializeField] TextMeshProUGUI moreInfoEchoMissionTitleText;
        [SerializeField] TextMeshProUGUI moreInfoEchoMissionDescriptionText;
        [SerializeField] TextMeshProUGUI moreInfoAbilityTitleText;
        [SerializeField] TextMeshProUGUI moreInfoAbilityDescriptionText;

        [Header("Mask More Info Colors")]
        [SerializeField] Color moreInfoMaskTitleColor = Color.black;
        [SerializeField] Color moreInfoEchoMissionTitleColor = new Color(0.4f, 0.2f, 0.8f);
        [SerializeField] Color moreInfoAbilityTitleColor = new Color(0.0f, 0.6f, 0.9f);

        [Header("More Info Weapon Settings")]
        [SerializeField] Sprite weaponSpriteForMoreInfo;

        [Header("More Info Colors")]
        [SerializeField] Color moreInfoDefaultColor = Color.white;
        [SerializeField] Color moreInfoWeaponColor = Color.cyan;

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

        public bool ValidateMoreInfoReferences()
        {
            bool isValid = true;

            if (moreInfoPanel == null)
            {
                LogDebug("WARNING: More Info Panel is not assigned!", true);
                isValid = false;
            }

            if (moreInfoTitleText == null)
            {
                LogDebug("WARNING: More Info Title Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoDescriptionText == null)
            {
                LogDebug("WARNING: More Info Description Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoItemImage == null)
            {
                LogDebug("WARNING: More Info Item Image is not assigned!", true);
                isValid = false;
            }

            if (moreInfoMaskDescriptionText == null)
            {
                LogDebug("WARNING: More Info Mask Description Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoEchoMissionTitleText == null)
            {
                LogDebug("WARNING: More Info Echo Mission Title Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoEchoMissionDescriptionText == null)
            {
                LogDebug("WARNING: More Info Echo Mission Description Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoAbilityTitleText == null)
            {
                LogDebug("WARNING: More Info Ability Title Text is not assigned!", true);
                isValid = false;
            }

            if (moreInfoAbilityDescriptionText == null)
            {
                LogDebug("WARNING: More Info Ability Description Text is not assigned!", true);
                isValid = false;
            }

            return isValid;
        }

        public void ShowMoreInfoForWeapon()
        {
            LogDebug("ShowMoreInfoForWeapon called");

            if (moreInfoTitleText != null)
            {
                moreInfoTitleText.text = "Musician's Stoccoviola";
                moreInfoTitleText.color = moreInfoWeaponColor;
                LogDebug("Set weapon title text");
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.text = "Musicians are well-versed in the finest arts esteemed by nobility. This weapon is one of the Musician's trusted companions along their journey, able to pierce the toughest armor while composing exquisite melodies in equal measure.";
                moreInfoDescriptionText.gameObject.SetActive(true); // Make sure it's visible for weapons
                LogDebug("Set weapon description text");
            }

            HideMaskSpecificFields();

            if (moreInfoItemImage != null)
            {
                if (weaponSpriteForMoreInfo != null)
                {
                    moreInfoItemImage.sprite = weaponSpriteForMoreInfo;
                    moreInfoItemImage.gameObject.SetActive(true);
                    LogDebug("Set weapon image from manually assigned sprite");
                }
                else
                {
                    moreInfoItemImage.gameObject.SetActive(false);
                    LogDebug("No weapon sprite assigned for more info");
                }
            }

            LogDebug("ShowMoreInfoForWeapon completed");
        }


        public void ShowMoreInfoForMask()
        {
            if (playerInventory == null || playerInventory.maskSlot == null)
            {
                ShowMoreInfoForEmptyMask();
                return;
            }

            bool hasMask = playerInventory.maskSlot.transform.childCount > 0;

            if (!hasMask)
            {
                ShowMoreInfoForEmptyMask();
                return;
            }

            GameObject maskObject = playerInventory.maskSlot.transform.GetChild(0).gameObject;
            BaseMask maskComponent = maskObject.GetComponent<BaseMask>();

            if (maskComponent == null)
            {
                ShowMoreInfoForEmptyMask();
                return;
            }

            ShowMaskSpecificFields();

            if (moreInfoTitleText != null)
            {
                moreInfoTitleText.text = maskComponent.maskName;
                moreInfoTitleText.color = moreInfoMaskTitleColor;
            }

            if (moreInfoMaskDescriptionText != null)
            {
                moreInfoMaskDescriptionText.text = maskComponent.maskDescription;
            }

            if (moreInfoEchoMissionTitleText != null)
            {
                moreInfoEchoMissionTitleText.text = "• Echo Mission";
                moreInfoEchoMissionTitleText.color = moreInfoEchoMissionTitleColor;
            }

            if (moreInfoEchoMissionDescriptionText != null)
            {
                string echoMissionText = GetEchoMissionDescription(maskComponent);
                moreInfoEchoMissionDescriptionText.text = echoMissionText;
            }

            BaseAbility ability = maskComponent.abilityObject.GetComponent<BaseAbility>();

            if (moreInfoAbilityTitleText != null)
            {
                moreInfoAbilityTitleText.text = "• Ability (" + ability.abilityName + ")";
                moreInfoAbilityTitleText.color = moreInfoAbilityTitleColor;
            }

            if (moreInfoAbilityDescriptionText != null)
            {
                string abilityDescription = GetAbilityDescription(maskComponent);
                moreInfoAbilityDescriptionText.text = abilityDescription;
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.gameObject.SetActive(false);
            }

            if (moreInfoItemImage != null)
            {
                if (maskComponent.maskPicture != null)
                {
                    moreInfoItemImage.sprite = maskComponent.maskPicture;
                    moreInfoItemImage.gameObject.SetActive(true);
                }
                else
                {
                    moreInfoItemImage.gameObject.SetActive(false);
                }
            }

            LogDebug($"Showing more info for mask: {maskComponent.maskName}");
        }

        string GetEchoMissionDescription(BaseMask maskComponent)
        {
            if (maskComponent.echoMission == null)
            {
                return "No echo mission available.";
            }

            if (maskComponent.echoMission is CollectGold collectGold)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(CollectGold).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentCollected = 0;
                if (fieldInfo != null)
                {
                    currentCollected = (int)fieldInfo.GetValue(collectGold);
                }

                return $"Collect gold to unlock the echo ability. Progress: {currentCollected} / {collectGold.goldToUnlockEcho}";
            }
            else if (maskComponent.echoMission is CollectMaxHealth collectHealth)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(CollectMaxHealth).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentCollected = 0;
                if (fieldInfo != null)
                {
                    currentCollected = (int)fieldInfo.GetValue(collectHealth);
                }

                return $"Collect health upgrades to unlock the echo ability. Progress: {currentCollected} / {collectHealth.maxHealthToCollect}";
            }
            else if (maskComponent.echoMission is MajorKills majorKills)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(MajorKills).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Reflection.FieldInfo requiredKillsField = typeof(MajorKills).GetField("majorKillsToDo", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                int currentKills = 0;
                int requiredKills = 0;

                if (fieldInfo != null)
                {
                    currentKills = (int)fieldInfo.GetValue(majorKills);
                }
                if (requiredKillsField != null)
                {
                    requiredKills = (int)requiredKillsField.GetValue(majorKills);
                }

                return $"Defeat enemies using major scale attacks to unlock the echo ability. Progress: {currentKills} / {requiredKills}";
            }
            else if (maskComponent.echoMission is DamageDelt damageDelt)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(DamageDelt).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Reflection.FieldInfo requiredDamageField = typeof(DamageDelt).GetField("damageToDeal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                int currentDamage = 0;
                int requiredDamage = 0;

                if (fieldInfo != null)
                {
                    currentDamage = (int)fieldInfo.GetValue(damageDelt);
                }
                if (requiredDamageField != null)
                {
                    requiredDamage = (int)requiredDamageField.GetValue(damageDelt);
                }

                return $"Deal damage to enemies to unlock the echo ability. Progress: {currentDamage} / {requiredDamage}";
            }

            return "Unknown echo mission type.";
        }

        string GetAbilityDescription(BaseMask maskComponent)
        {
            if (maskComponent.abilityObject != null)
            {
                BaseAbility ability = maskComponent.abilityObject.GetComponent<BaseAbility>();
                if (ability != null)
                {
                    return ability.abilityDescriptionAlt;
                }
            }

            if (!string.IsNullOrEmpty(maskComponent.echoDescription))
            {
                return maskComponent.echoDescription;
            }

            return "No ability description available.";
        }

        public void ShowMoreInfoForCharm(GameObject charmObject)
        {
            if (charmObject == null)
            {
                ShowMoreInfoForEmptyCharm();
                return;
            }

            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();

            if (charmComponent == null)
            {
                ShowMoreInfoForEmptyCharm();
                return;
            }

            if (moreInfoTitleText != null)
            {
                string rarityText = GetRarityDisplayName(charmComponent.charmRarity);
                moreInfoTitleText.text = $"{charmComponent.charmName} ({rarityText})";
                moreInfoTitleText.color = GetRarityColor(charmComponent.charmRarity);
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.text = charmComponent.charmDescription;
                moreInfoDescriptionText.gameObject.SetActive(true);
            }

            HideMaskSpecificFields();

            if (moreInfoItemImage != null)
            {
                if (charmComponent.charmPicture != null)
                {
                    moreInfoItemImage.sprite = charmComponent.charmPicture;
                    moreInfoItemImage.gameObject.SetActive(true);
                }
                else
                {
                    moreInfoItemImage.gameObject.SetActive(false);
                }
            }

            LogDebug($"Showing more info for charm: {charmComponent.charmName}");
        }

        public void ShowMoreInfoForPotion()
        {
            if (playerInventory == null || playerInventory.numberOfPotions <= 0)
            {
                ShowMoreInfoForEmptyPotion();
                return;
            }

            if (playerInventory.potionPrefab == null)
            {
                ShowMoreInfoForEmptyPotion();
                return;
            }

            BaseCharm potionComponent = playerInventory.potionPrefab.GetComponent<BaseCharm>();

            if (potionComponent == null)
            {
                ShowMoreInfoForEmptyPotion();
                return;
            }

            if (moreInfoTitleText != null)
            {
                string countText = playerInventory.numberOfPotions > 1 ? $" (x{playerInventory.numberOfPotions})" : "";
                moreInfoTitleText.text = potionComponent.charmName + countText;
                moreInfoTitleText.color = GetRarityColor(potionComponent.charmRarity);
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.text = potionComponent.charmDescription;
                moreInfoDescriptionText.gameObject.SetActive(true);
            }

            HideMaskSpecificFields();

            if (moreInfoItemImage != null)
            {
                if (potionComponent.charmPicture != null)
                {
                    moreInfoItemImage.sprite = potionComponent.charmPicture;
                    moreInfoItemImage.gameObject.SetActive(true);
                }
                else
                {
                    moreInfoItemImage.gameObject.SetActive(false);
                }
            }

            LogDebug($"Showing more info for potion: {potionComponent.charmName}");
        }

        void HideMaskSpecificFields()
        {
            if (moreInfoMaskDescriptionText != null)
            {
                moreInfoMaskDescriptionText.gameObject.SetActive(false);
            }

            if (moreInfoEchoMissionTitleText != null)
            {
                moreInfoEchoMissionTitleText.gameObject.SetActive(false);
            }

            if (moreInfoEchoMissionDescriptionText != null)
            {
                moreInfoEchoMissionDescriptionText.gameObject.SetActive(false);
            }

            if (moreInfoAbilityTitleText != null)
            {
                moreInfoAbilityTitleText.gameObject.SetActive(false);
            }

            if (moreInfoAbilityDescriptionText != null)
            {
                moreInfoAbilityDescriptionText.gameObject.SetActive(false);
            }
        }

        void ShowMaskSpecificFields()
        {
            if (moreInfoMaskDescriptionText != null)
            {
                moreInfoMaskDescriptionText.gameObject.SetActive(true);
            }

            if (moreInfoEchoMissionTitleText != null)
            {
                moreInfoEchoMissionTitleText.gameObject.SetActive(true);
            }

            if (moreInfoEchoMissionDescriptionText != null)
            {
                moreInfoEchoMissionDescriptionText.gameObject.SetActive(true);
            }

            if (moreInfoAbilityTitleText != null)
            {
                moreInfoAbilityTitleText.gameObject.SetActive(true);
            }

            if (moreInfoAbilityDescriptionText != null)
            {
                moreInfoAbilityDescriptionText.gameObject.SetActive(true);
            }
        }

        public void ShowMoreInfoForEmptyMask()
        {
            if (moreInfoTitleText != null)
            {
                moreInfoTitleText.text = "No Mask Equipped";
                moreInfoTitleText.color = moreInfoMaskTitleColor; // Use black color
            }

            if (moreInfoMaskDescriptionText != null)
            {
                moreInfoMaskDescriptionText.text = "Equip a mask to see its details and abilities.";
            }

            if (moreInfoEchoMissionTitleText != null)
            {
                moreInfoEchoMissionTitleText.text = "Echo Mission:";
                moreInfoEchoMissionTitleText.color = moreInfoEchoMissionTitleColor;
            }

            if (moreInfoEchoMissionDescriptionText != null)
            {
                moreInfoEchoMissionDescriptionText.text = "No echo mission available.";
            }

            if (moreInfoAbilityTitleText != null)
            {
                moreInfoAbilityTitleText.text = "Ability:";
                moreInfoAbilityTitleText.color = moreInfoAbilityTitleColor;
            }

            if (moreInfoAbilityDescriptionText != null)
            {
                moreInfoAbilityDescriptionText.text = "No ability available.";
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.gameObject.SetActive(false);
            }

            if (moreInfoItemImage != null)
            {
                moreInfoItemImage.gameObject.SetActive(false);
            }

            LogDebug("Showing more info for empty mask slot");
        }

        public void ShowMoreInfoForEmptyCharm()
        {
            if (moreInfoTitleText != null)
            {
                moreInfoTitleText.text = "No Charm Equipped";
                moreInfoTitleText.color = moreInfoDefaultColor;
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.text = "Equip a charm to see its details and effects.";
            }

            if (moreInfoItemImage != null)
            {
                moreInfoItemImage.gameObject.SetActive(false);
            }

            LogDebug("Showing more info for empty charm slot");
        }

        public void ShowMoreInfoForEmptyPotion()
        {
            if (moreInfoTitleText != null)
            {
                moreInfoTitleText.text = "No Potions Available";
                moreInfoTitleText.color = moreInfoDefaultColor;
            }

            if (moreInfoDescriptionText != null)
            {
                moreInfoDescriptionText.text = "Collect potions to restore health during your journey.";
            }

            if (moreInfoItemImage != null)
            {
                moreInfoItemImage.gameObject.SetActive(false);
            }

            LogDebug("Showing more info for empty potion slot");
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

            GameObject legendarySlotCharm = playerInventory.legendaryCharmSlot.transform.GetComponentInChildren<BaseCharm>().gameObject;

            if (legendaryCharmButton != null && legendarySlotCharm != null)
            {
                legendaryCharmButton.UpdateInfoWithEmptySprite(legendarySlotCharm, emptyLegendaryCharmSprite);
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
                    charmDescriptionText.text = maskComponent.maskDescription;
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