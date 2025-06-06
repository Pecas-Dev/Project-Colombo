using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using ProjectColombo.Objects.Masks;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryTabController : MonoBehaviour
    {
        [Header("Inventory Manager Reference")]
        [SerializeField] PauseMenuInventoryManager inventoryManager;

        [Header("Inventory Slots")]
        [SerializeField] Button[] inventorySlotButtons;
        [SerializeField] GameObject[] slotSelectors;

        [Header("Pentagram Areas")]
        [SerializeField] GameObject weaponPentagram;
        [SerializeField] GameObject maskPentagram;
        [SerializeField] GameObject potionCharmPentagram;

        [Header("Clef Indicators")]
        [SerializeField] GameObject weaponClef;
        [SerializeField] GameObject maskClef;
        [SerializeField] GameObject potionCharmClef;

        [Header("More Info Panel")]
        [SerializeField] GameObject moreInfoPanel;
        [SerializeField] public bool isMoreInfoActive = false;
        [SerializeField] float moreInfoActivationDelay = 0.4f;

        [Header("Objects to Hide/Show")]
        [SerializeField] GameObject[] objectsToHideOnMoreInfo;

        [Header("Selector Animation")]
        [SerializeField] float selectorPulseSpeed = 2f;
        [SerializeField] float selectorMinAlpha = 0.5f;
        [SerializeField] float selectorMaxAlpha = 1f;

        [Header("Selector Colors")]
        [SerializeField] Color defaultSelectorColor = Color.black;
        [SerializeField] Color commonSelectorColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] Color rareSelectorColor = new Color(0.0f, 0.5f, 1.0f, 1f);
        [SerializeField] Color legendarySelectorColor = new Color(1.0f, 0.84f, 0.0f, 1f);

        [Header("Mask Selector Colors")]
        [SerializeField] Color normalMaskSelectorColor = new Color(0.6f, 0.8f, 1.0f, 1f); 
        [SerializeField] Color echoMaskSelectorColor = new Color(1.0f, 0.6f, 0.8f, 1f);

        [Header("Text Display Categories")]
        [SerializeField] string weaponCategoryName = "Weapon";
        [SerializeField] string maskCategoryName = "Mask";
        [SerializeField] string charmCategoryName = "Charm";
        [SerializeField] string potionCategoryName = "Potion";

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        int currentSelectedIndex = -1;

        float lastMoreInfoActivationTime;
        float lastSubmitTime;

        bool isInitialized = false;
        bool isActive = false;
        bool isTabSelected = false;

        Color[] originalSelectorColors;

        IEnumerator[] selectorAnimations;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void Start()
        {
            if (moreInfoPanel != null)
            {
                moreInfoPanel.SetActive(false);
                isMoreInfoActive = false;
                LogDebug("More info panel disabled on Start");
            }
        }

        void OnEnable()
        {
            LogDebug("OnEnable called");
            isActive = true;

            if (moreInfoPanel != null)
            {
                moreInfoPanel.SetActive(false);
                isMoreInfoActive = false;
                LogDebug("More info panel disabled on tab enable");
            }

            ResetSelectionState();

            StartCoroutine(DelayedInitialSelection());
            StartCoroutine(WaitForTabSelection());

            UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager != null && inventorySlotButtons != null && inventorySlotButtons.Length > 0)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.PauseInventoryTab, inventorySlotButtons[0].gameObject);
                LogDebug("Registered first selectable with navigation manager");
            }
        }

        void OnDisable()
        {
            LogDebug("OnDisable called");
            isActive = false;
            isTabSelected = false;

            if (moreInfoPanel != null && isMoreInfoActive)
            {
                HideMoreInfo();
                LogDebug("More info panel hidden on tab disable");
            }

            ResetSelectionState();
        }

        IEnumerator WaitForTabSelection()
        {
            yield return new WaitForEndOfFrame();

            yield return null;

            UINavigationManager navManager = FindFirstObjectByType<UINavigationManager>();

            if (navManager != null)
            {
                isTabSelected = navManager.GetCurrentState() == UINavigationState.PauseInventoryTab;
                LogDebug($"Tab selection status from UINavigationManager: {isTabSelected}");
            }
        }

        void Update()
        {
            if (!isActive || !isTabSelected)
            {
                return;
            }

            HandleMoreInfoInput();

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                bool isOurButton = false;
                for (int i = 0; i < inventorySlotButtons.Length; i++)
                {
                    if (EventSystem.current.currentSelectedGameObject == inventorySlotButtons[i].gameObject)
                    {
                        if (i != currentSelectedIndex)
                        {
                            SelectSlot(i, false);
                        }
                        isOurButton = true;
                        break;
                    }
                }

                if (!isOurButton && currentSelectedIndex >= 0)
                {
                    ResetSelectionState();
                }
            }
            else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (currentSelectedIndex >= 0)
                {
                    ResetSelectionState();
                }
            }
        }

        void HandleMoreInfoRawInput()
        {
            if (!isMoreInfoActive)
            {
                return;
            }

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.backspaceKey.wasPressedThisFrame)
                {
                    HideMoreInfo();
                    LogDebug("More info closed via keyboard input (Escape/Backspace)");
                    return;
                }
            }

            if (UnityEngine.InputSystem.Gamepad.current != null)
            {
                if (UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    HideMoreInfo();
                    LogDebug("More info closed via gamepad East button");
                    return;
                }
            }
        }

        void HandleMoreInfoInput()
        {
            if (gameInput == null || gameInput.inputActions == null)
            {
                return;
            }

            HandleMoreInfoRawInput();

            if (isMoreInfoActive)
            {
                return;
            }

            if (gameInput.inputActions.UI.Submit.WasPressedThisFrame())
            {
                if (Time.unscaledTime - lastSubmitTime < 0.3f)
                {
                    return;
                }

                lastSubmitTime = Time.unscaledTime;
                ShowMoreInfo();
                lastMoreInfoActivationTime = Time.unscaledTime;
            }
        }

        void DisableUIInputsForMoreInfo()
        {
            if (gameInput != null && gameInput.inputActions != null)
            {
                gameInput.inputActions.UI.Cancel.Disable();
                gameInput.inputActions.UI.MoveLeftShoulder.Disable();
                gameInput.inputActions.UI.MoveRightShoulder.Disable();
                gameInput.inputActions.UI.Navigate.Disable();
                gameInput.inputActions.UI.Submit.Disable();
                LogDebug("Disabled UI inputs for More Info panel");
            }
        }

        void EnableUIInputsAfterMoreInfo()
        {
            if (gameInput != null && gameInput.inputActions != null)
            {
                gameInput.inputActions.UI.Cancel.Enable();
                gameInput.inputActions.UI.MoveLeftShoulder.Enable();
                gameInput.inputActions.UI.MoveRightShoulder.Enable();
                gameInput.inputActions.UI.Navigate.Enable();
                gameInput.inputActions.UI.Submit.Enable();
                LogDebug("Re-enabled UI inputs after More Info panel");
            }
        }

        public void ShowMoreInfo()
        {
            if (moreInfoPanel == null)
            {
                LogDebug("MoreInfo panel is not assigned in inspector!", true);
                return;
            }

            if (currentSelectedIndex < 0 || currentSelectedIndex >= inventorySlotButtons.Length)
            {
                LogDebug("Invalid selection for More Info", true);
                return;
            }

            if (inventoryManager != null)
            {
                if (!inventoryManager.ValidateMoreInfoReferences())
                {
                    LogDebug("More Info references not properly assigned in InventoryManager!", true);
                    return;
                }
            }

            UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseInventoryMoreInfo);
                LogDebug("Set navigation state to PauseInventoryMoreInfo");
            }

            DisableUIInputsForMoreInfo();

            moreInfoPanel.SetActive(true);
            isMoreInfoActive = true;

            HideGameObjects();

            UpdateMoreInfoDisplay();

            LogDebug($"MoreInfo panel activated for slot {currentSelectedIndex}");
        }

        public void HideMoreInfo()
        {
            if (moreInfoPanel == null)
            {
                return;
            }

            UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseInventoryTab);
            }

            EnableUIInputsAfterMoreInfo();

            moreInfoPanel.SetActive(false);
            isMoreInfoActive = false;

            ShowGameObjects();

            LogDebug("MoreInfo panel deactivated");
        }

        void HideGameObjects()
        {
            if (objectsToHideOnMoreInfo == null)
            {
                return;
            }

            foreach (GameObject gameObjectToHide in objectsToHideOnMoreInfo)
            {
                if (gameObjectToHide != null)
                {
                    gameObjectToHide.SetActive(false);
                    LogDebug($"Hidden object: {gameObjectToHide.name}");
                }
            }
        }

        void ShowGameObjects()
        {
            if (objectsToHideOnMoreInfo == null)
            {
                return;
            }

            foreach (GameObject gameObjectToShow in objectsToHideOnMoreInfo)
            {
                if (gameObjectToShow != null)
                {
                    gameObjectToShow.SetActive(true);
                    LogDebug($"Shown object: {gameObjectToShow.name}");
                }
            }
        }

        void UpdateMoreInfoDisplay()
        {
            if (!isMoreInfoActive || inventoryManager == null)
            {
                return;
            }

            switch (currentSelectedIndex)
            {
                case 0: // Weapon Slot
                    inventoryManager.ShowMoreInfoForWeapon();
                    break;
                case 1: // Mask Slot
                    inventoryManager.ShowMoreInfoForMask();
                    break;
                case 7: // Potion Slot
                    inventoryManager.ShowMoreInfoForPotion();
                    break;
                default: // Charm Slots (2-6)
                    CharmButton charmButton = inventorySlotButtons[currentSelectedIndex].GetComponent<CharmButton>();
                    if (charmButton != null)
                    {
                        inventoryManager.ShowMoreInfoForCharm(charmButton.charmObject);
                    }
                    else
                    {
                        inventoryManager.ShowMoreInfoForEmptyCharm();
                    }
                    break;
            }
        }

        public void ResetSelectionState()
        {
            LogDebug("Resetting selection state");

            HideAllClefs();
            HideAllSelectors();

            if (selectorAnimations != null)
            {
                for (int i = 0; i < selectorAnimations.Length; i++)
                {
                    StopSelectorAnimation(i);
                }
            }

            currentSelectedIndex = -1;
        }

        public void Initialize()
        {
            LogDebug("Initializing inventory tab controller");

            InitializeInventoryManager();

            selectorAnimations = new IEnumerator[slotSelectors.Length];
            originalSelectorColors = new Color[slotSelectors.Length];

            for (int i = 0; i < slotSelectors.Length; i++)
            {
                if (slotSelectors[i] != null)
                {
                    Image selectorImage = slotSelectors[i].GetComponent<Image>();
                    if (selectorImage != null)
                    {
                        originalSelectorColors[i] = selectorImage.color;
                    }
                }
            }

            HideAllClefs();
            HideAllSelectors();

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                if (inventorySlotButtons[i] == null) continue;

                int slotIndex = i;

                inventorySlotButtons[i].onClick.RemoveAllListeners();
                inventorySlotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));

                EventTrigger eventTrigger = inventorySlotButtons[i].GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = inventorySlotButtons[i].gameObject.AddComponent<EventTrigger>();
                }

                eventTrigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Select
                };
                selectEntry.callback.AddListener((data) =>
                {
                    SelectSlot(slotIndex);
                    UpdateInfoDisplay(slotIndex);
                });
                eventTrigger.triggers.Add(selectEntry);

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnterEntry.callback.AddListener((data) =>
                {
                    SelectSlot(slotIndex);
                    UpdateInfoDisplay(slotIndex);
                });
                eventTrigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };
                pointerExitEntry.callback.AddListener((data) => EnsureVisualSelection());
                eventTrigger.triggers.Add(pointerExitEntry);

                SetupExplicitNavigation(i);
            }

            isInitialized = true;
        }

        void InitializeInventoryManager()
        {
            if (inventoryManager == null)
            {
                inventoryManager = GetComponent<PauseMenuInventoryManager>();

                if (inventoryManager == null)
                {
                    inventoryManager = FindFirstObjectByType<PauseMenuInventoryManager>();
                }
            }
        }

        void UpdateInfoDisplay(int slotIndex)
        {
            if (inventoryManager == null) return;

            CharmButton charmButton = inventorySlotButtons[slotIndex].GetComponent<CharmButton>();
            if (charmButton != null)
            {
                return;
            }

            switch (slotIndex)
            {
                case 0: // Weapon Slot
                    inventoryManager.ShowEmptyWeaponInfo();
                    break;
                case 1: // Mask Slot
                    inventoryManager.ShowMaskInfo();
                    break;
                case 7: // Potion Slot
                    inventoryManager.ShowEmptyPotionInfo();
                    break;
                default:
                    inventoryManager.ShowEmptyCharmInfo();
                    break;
            }
        }

        public void UpdateSelectorColorForCharm(BaseCharm charm)
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= slotSelectors.Length || charm == null)
            {
                return;
            }

            GameObject selector = slotSelectors[currentSelectedIndex];

            if (selector == null)
            {
                return;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                return;
            }

            Color newColor;

            switch (charm.charmRarity)
            {
                case RARITY.COMMON:
                    newColor = commonSelectorColor;
                    break;
                case RARITY.RARE:
                    newColor = rareSelectorColor;
                    break;
                case RARITY.LEGENDARY:
                    newColor = legendarySelectorColor;
                    break;
                default:
                    newColor = defaultSelectorColor;
                    break;
            }

            float currentAlpha = selectorImage.color.a;
            selectorImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);

            LogDebug($"Updated selector color for {charm.charmName} (Rarity: {charm.charmRarity}) - Color: {newColor}");
        }

        public void UpdateSelectorColorForMask(BaseMask mask)
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= slotSelectors.Length || mask == null)
            {
                return;
            }

            GameObject selector = slotSelectors[currentSelectedIndex];

            if (selector == null)
            {
                return;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                return;
            }

            Color newColor = mask.echoUnlocked ? echoMaskSelectorColor : normalMaskSelectorColor;

            float currentAlpha = selectorImage.color.a;
            selectorImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);

            LogDebug($"Updated selector color for {mask.maskName} (Echo Unlocked: {mask.echoUnlocked}) - Color: {newColor}");
        }

        public void ResetSelectorColor()
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= slotSelectors.Length)
            {
                return;
            }

            GameObject selector = slotSelectors[currentSelectedIndex];

            if (selector == null)
            {
                return;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                return;
            }

            float currentAlpha = selectorImage.color.a;
            selectorImage.color = new Color(defaultSelectorColor.r, defaultSelectorColor.g, defaultSelectorColor.b, currentAlpha);

            LogDebug("Reset selector color to default black");
        }

        public void UpdateCharmButtonImageColorInstant(BaseCharm charm, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length || charm == null)
            {
                return;
            }

            CharmButton charmButton = inventorySlotButtons[slotIndex].GetComponent<CharmButton>();

            if (charmButton == null)
            {
                return;
            }

            Image charmButtonImage = charmButton.GetComponent<Image>();

            if (charmButtonImage == null)
            {
                return;
            }

            Color newColor;
            float currentAlpha = charmButtonImage.color.a;

            switch (charm.charmRarity)
            {
                case RARITY.COMMON:
                    newColor = commonSelectorColor;
                    break;
                case RARITY.RARE:
                    newColor = rareSelectorColor;
                    break;
                case RARITY.LEGENDARY:
                    newColor = legendarySelectorColor;
                    break;
                default:
                    newColor = new Color(1, 1, 1, currentAlpha);
                    break;
            }

            charmButtonImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);

            LogDebug($"Instantly updated charm button image color for {charm.charmName} (Rarity: {charm.charmRarity}) - Color: {newColor}");
        }

        public void ResetCharmButtonImageColorInstant(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length)
            {
                return;
            }

            CharmButton charmButton = inventorySlotButtons[slotIndex].GetComponent<CharmButton>();

            if (charmButton == null)
            {
                return;
            }

            Image charmButtonImage = charmButton.GetComponent<Image>();

            if (charmButtonImage == null)
            {
                return;
            }

            float currentAlpha = charmButtonImage.color.a;
            charmButtonImage.color = new Color(1, 1, 1, currentAlpha);

            LogDebug($"Instantly reset charm button image color for slot {slotIndex} to default white");
        }

        public void ResetAllCharmButtonColorsToWhite()
        {
            if (inventorySlotButtons == null)
            {
                return;
            }

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                if (inventorySlotButtons[i] != null)
                {
                    CharmButton charmButton = inventorySlotButtons[i].GetComponent<CharmButton>();

                    if (charmButton != null)
                    {
                        Image charmButtonImage = charmButton.GetComponent<Image>();

                        if (charmButtonImage != null)
                        {
                            float currentAlpha = charmButtonImage.color.a;
                            charmButtonImage.color = new Color(1, 1, 1, currentAlpha);
                        }
                    }
                }
            }

            LogDebug("Reset all charm button colors to white (1,1,1) while preserving alpha");
        }


        public void UpdateAllCharmButtonColors()
        {
            if (inventorySlotButtons == null)
            {
                return;
            }

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                if (inventorySlotButtons[i] != null)
                {
                    CharmButton charmButton = inventorySlotButtons[i].GetComponent<CharmButton>();

                    if (charmButton != null)
                    {
                        if (charmButton.charmObject != null)
                        {
                            BaseCharm charm = charmButton.charmObject.GetComponent<BaseCharm>();
                            if (charm != null)
                            {
                                UpdateCharmButtonImageColorInstant(charm, i);
                            }
                        }
                        else
                        {
                            ResetCharmButtonImageColorInstant(i);
                        }
                    }
                }
            }

            LogDebug("Updated all charm button colors based on current inventory");
        }




        void SetupExplicitNavigation(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= inventorySlotButtons.Length || inventorySlotButtons[buttonIndex] == null)
            {
                return;
            }

            Button button = inventorySlotButtons[buttonIndex];
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // 0.SlotWeapon, 1.SlotMask, 2.SlotCharm_1, 3.SlotCharm_2, 4.SlotCharm_3, 
            // 5.SlotCharm_4, 6.SlotCharm_5, 7.SlotPotion_1

            switch (buttonIndex)
            {
                // SlotWeapon (0)
                case 0:
                    nav.selectOnUp = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnDown = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnLeft = null;
                    nav.selectOnRight = null;
                    break;

                // SlotMask (1)
                case 1:
                    nav.selectOnUp = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnDown = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnLeft = null;
                    nav.selectOnRight = null;
                    break;

                // SlotCharm_1 (2)
                case 2:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[7]; // SlotPotion_1
                    nav.selectOnRight = inventorySlotButtons[3]; // SlotCharm_2
                    break;

                // SlotCharm_2 (3)
                case 3:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnRight = inventorySlotButtons[4]; // SlotCharm_3
                    break;

                // SlotCharm_3 (4)
                case 4:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[3]; // SlotCharm_2
                    nav.selectOnRight = inventorySlotButtons[5]; // SlotCharm_4
                    break;

                // SlotCharm_4 (5)
                case 5:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[4]; // SlotCharm_3
                    nav.selectOnRight = inventorySlotButtons[6]; // SlotCharm_5
                    break;

                // SlotCharm_5 (6)
                case 6:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[5]; // SlotCharm_4
                    nav.selectOnRight = inventorySlotButtons[7]; // SlotPotion_1
                    break;

                // SlotPotion_1 (7)
                case 7:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[6]; // SlotCharm_5
                    nav.selectOnRight = inventorySlotButtons[2]; // SlotCharm_1
                    break;
            }

            button.navigation = nav;
        }

        Button FindFirstButtonInArea(GameObject area)
        {
            if (area == null) return null;

            foreach (Button button in inventorySlotButtons)
            {
                if (button != null && IsChildOf(button.transform, area))
                {
                    return button;
                }
            }

            return null;
        }

        Button[] GetButtonsInArea(GameObject area)
        {
            if (area == null) return new Button[0];

            System.Collections.Generic.List<Button> buttonsInArea = new System.Collections.Generic.List<Button>();

            foreach (Button button in inventorySlotButtons)
            {
                if (button != null && IsChildOf(button.transform, area))
                {
                    buttonsInArea.Add(button);
                }
            }

            return buttonsInArea.ToArray();
        }

        void EnsureVisualSelection()
        {
            if (currentSelectedIndex >= 0 && currentSelectedIndex < inventorySlotButtons.Length)
            {
                UpdateSelectionVisuals(currentSelectedIndex);
            }
        }

        public void SelectSlot(int slotIndex, bool setEventSystemSelection = true)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length)
            {
                LogDebug($"Invalid slot index: {slotIndex}");
                return;
            }

            if (slotIndex == currentSelectedIndex && inventorySlotButtons[slotIndex].gameObject == EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            LogDebug($"Selecting inventory slot {slotIndex}, setEventSystemSelection: {setEventSystemSelection}");

            if (currentSelectedIndex >= 0 && currentSelectedIndex < slotSelectors.Length)
            {
                if (slotSelectors[currentSelectedIndex] != null)
                {
                    slotSelectors[currentSelectedIndex].SetActive(false);
                }

                if (selectorAnimations[currentSelectedIndex] != null)
                {
                    StopCoroutine(selectorAnimations[currentSelectedIndex]);
                    selectorAnimations[currentSelectedIndex] = null;
                }
            }

            currentSelectedIndex = slotIndex;

            UpdateSelectionVisuals(slotIndex);

            UpdateClefs(slotIndex);

            if (setEventSystemSelection && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(inventorySlotButtons[slotIndex].gameObject);
            }
        }

        void UpdateSelectionVisuals(int slotIndex)
        {
            if (slotIndex < slotSelectors.Length && slotSelectors[slotIndex] != null)
            {
                slotSelectors[slotIndex].SetActive(true);

                CharmButton charmButton = inventorySlotButtons[slotIndex].GetComponent<CharmButton>();

                if (charmButton != null && charmButton.charmObject != null)
                {
                    BaseCharm charm = charmButton.charmObject.GetComponent<BaseCharm>();

                    if (charm != null)
                    {
                        UpdateSelectorColorForCharm(charm);
                        UpdateCharmButtonImageColorInstant(charm, slotIndex);
                    }
                }
                else
                {
                    ResetSelectorColor();
                    ResetCharmButtonImageColorInstant(slotIndex);
                }

                StopSelectorAnimation(slotIndex);
                selectorAnimations[slotIndex] = AnimateSelector(slotSelectors[slotIndex]);
                StartCoroutine(selectorAnimations[slotIndex]);
            }
        }

        void StopSelectorAnimation(int index)
        {
            if (index >= 0 && index < selectorAnimations.Length && selectorAnimations[index] != null)
            {
                StopCoroutine(selectorAnimations[index]);
                selectorAnimations[index] = null;
            }
        }

        void UpdateClefs(int slotIndex)
        {
            HideAllClefs();

            Button selectedButton = inventorySlotButtons[slotIndex];

            if (selectedButton == null)
            {
                return;
            }

            if (IsChildOf(selectedButton.transform, weaponPentagram))
            {
                if (weaponClef != null)
                {
                    weaponClef.SetActive(true);
                }
            }
            else if (IsChildOf(selectedButton.transform, maskPentagram))
            {
                if (maskClef != null)
                {
                    maskClef.SetActive(true);
                }
            }
            else if (IsChildOf(selectedButton.transform, potionCharmPentagram))
            {
                if (potionCharmClef != null)
                {
                    potionCharmClef.SetActive(true);
                }
            }
        }

        public void OnTabSelected()
        {
            isTabSelected = true;
            LogDebug("Tab selected notification received");
        }

        public void OnTabDeselected()
        {
            isTabSelected = false;
            LogDebug("Tab deselected notification received");
            ResetSelectionState();
        }

        IEnumerator AnimateSelector(GameObject selector)
        {
            if (selector == null) yield break;

            Image selectorImage = selector.GetComponent<Image>();
            if (selectorImage == null) yield break;

            Color baseColor = selectorImage.color;
            float time = 0f;

            while (selector != null && selector.activeInHierarchy && selectorImage != null)
            {
                float alpha = Mathf.Lerp(selectorMinAlpha, selectorMaxAlpha, (Mathf.Sin(time * selectorPulseSpeed) + 1f) * 0.5f);

                Color newColor = baseColor;
                newColor.a = alpha;
                selectorImage.color = newColor;

                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();

            yield return null;

            if (inventorySlotButtons != null && inventorySlotButtons.Length > 0 && inventorySlotButtons[0] != null)
            {
                SelectSlot(0, true);
                LogDebug("Initial selection set to slot 0");
            }
            else
            {
                LogDebug("Failed to set initial selection - no buttons available");
            }
        }

        void HideAllClefs()
        {
            if (weaponClef != null)
            {
                weaponClef.SetActive(false);
            }
            if (maskClef != null)
            {
                maskClef.SetActive(false);
            }
            if (potionCharmClef != null)
            {
                potionCharmClef.SetActive(false);
            }
        }

        void HideAllSelectors()
        {
            if (slotSelectors == null) return;

            foreach (var selector in slotSelectors)
            {
                if (selector != null)
                {
                    selector.SetActive(false);
                }
            }
        }

        bool IsChildOf(Transform child, GameObject parent)
        {
            if (child == null || parent == null) return false;

            Transform current = child;
            while (current != null)
            {
                if (current.gameObject == parent)
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }

        public void Show()
        {
            isActive = true;
            gameObject.SetActive(true);

            LogDebug("Show called - waiting for parent UI to handle selection");
        }

        public void Hide()
        {
            isActive = false;
            isTabSelected = false;
            ResetSelectionState();
            gameObject.SetActive(false);
        }

        void LogDebug(string message, bool isWarning = false)
        {
            if (enableDebugLogs)
            {
                if (isWarning)
                    Debug.LogWarning($"[InventoryTabController] {message}");
                else
                    Debug.Log($"<color=#00AAFF>[InventoryTabController] {message}</color>");
            }
        }
    }
}