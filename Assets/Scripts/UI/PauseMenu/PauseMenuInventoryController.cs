using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryController : MenuController
    {
        [Header("Tab Navigation")]
        [SerializeField] Button[] tabButtons;
        [SerializeField] GameObject[] tabPanels;
        [SerializeField] TextMeshProUGUI[] tabTexts;
        [SerializeField] GameObject[] tabSelectionIndicators;
        [SerializeField] GameObject[] firstSelectedButton;

        [Header("Tab Colors")]
        [SerializeField] Color tabSelectedColor = Color.white;
        [SerializeField] Color tabUnselectedColor = Color.gray;
        [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);

        [Header("Inventory Pentagram System")]
        [SerializeField] GameObject weaponPentagram;
        [SerializeField] GameObject maskPentagram;
        [SerializeField] GameObject potionsCharmsPentagram;
        [SerializeField] GameObject weaponClef;
        [SerializeField] GameObject maskClef;
        [SerializeField] GameObject potionsCharmsClef;
        [SerializeField] Button[] inventorySlotButtons;
        [SerializeField] GameObject[] inventorySlotSelectors;

        [Header("Inventory Slot Animation")]
        [SerializeField] float selectorFlickerMinAlpha = 0.5f;
        [SerializeField] float selectorFlickerMaxAlpha = 1.0f;
        [SerializeField] float selectorFlickerSpeed = 2.0f;

        [SerializeField] float selectorBouncingMinScale = 0.95f;
        [SerializeField] float selectorBouncingMaxScale = 1.05f;
        [SerializeField] float selectorBouncingSpeed = 1.5f;

        [Header("Slot Hover Animation")]
        [SerializeField] float slotHoverScaleIncrease = 0.1f;
        [SerializeField] float slotHoverAlphaIncrease = 0.2f;
        //[SerializeField] float slotHoverAnimationSpeed = 5.0f;

        [Header("Slot Submit Animation")]
        [SerializeField] float slotSubmitShrinkScale = 0.8f;
        [SerializeField] float slotSubmitShrinkDuration = 0.1f;
        [SerializeField] float slotSubmitBounceDuration = 0.2f;

        [Header("Charm System")]
        [SerializeField] Button[] charmSlotButtons;
        [SerializeField] Button legendaryCharmButton;
        [SerializeField] TextMeshProUGUI charmNameText;
        [SerializeField] TextMeshProUGUI charmDescriptionText;

        [Header("Empty Slot Images")]
        [SerializeField] Sprite emptyCharmSlotSprite; 
        [SerializeField] Sprite emptyLegendaryCharmSlotSprite; 

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;


        int currentInventorySlotIndex = -1;
        int currentTabIndex = 0;


        GameObject lastSelectedButton;
        GameObject[] allClefs;

        Coroutine[] textAnimationCoroutines;

        Button currentSelectedInventoryButton;


        Dictionary<RectTransform, Vector3> slotOriginalScales = new Dictionary<RectTransform, Vector3>();
        Dictionary<Image, Vector3> slotImageOriginalScales = new Dictionary<Image, Vector3>();
        Dictionary<Image, float> slotImageOriginalAlpha = new Dictionary<Image, float>();

        Dictionary<int, bool> slotIsHovering = new Dictionary<int, bool>();

        Coroutine selectorAnimationCoroutine;

        Dictionary<int, Coroutine> slotHoverCoroutines = new Dictionary<int, Coroutine>();
        Dictionary<int, Coroutine> slotSubmitCoroutines = new Dictionary<int, Coroutine>();


        public override void Initialize()
        {
            base.Initialize();

            ProjectColombo.GameManagement.Events.CustomEvents.OnCharmCollected += OnCharmCollected;

            if (emptyCharmSlotSprite != null)
            {
                foreach (Button button in charmSlotButtons)
                {
                    if (button != null)
                    {
                        CharmButton charmButton = button.GetComponent<CharmButton>();

                        if (charmButton != null && charmButton.imageSlot != null)
                        {
                            charmButton.imageSlot.sprite = emptyCharmSlotSprite;
                        }
                        else
                        {
                            Image[] images = button.GetComponentsInChildren<Image>();

                            foreach (Image img in images)
                            {
                                if (img.gameObject != button.gameObject)
                                {
                                    img.sprite = emptyCharmSlotSprite;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                Sprite emptySprite = emptyLegendaryCharmSlotSprite != null ? emptyLegendaryCharmSlotSprite : emptyCharmSlotSprite;

                if (emptySprite != null)
                {
                    CharmButton charmButton = legendaryCharmButton.GetComponent<CharmButton>();
                    if (charmButton != null && charmButton.imageSlot != null)
                    {
                        charmButton.imageSlot.sprite = emptySprite;
                    }
                    else
                    {
                        Image[] images = legendaryCharmButton.GetComponentsInChildren<Image>();
                        foreach (Image img in images)
                        {
                            if (img.gameObject != legendaryCharmButton.gameObject)
                            {
                                img.sprite = emptySprite;
                                break;
                            }
                        }
                    }
                }
            }

            LogDebug("PauseMenuInventoryController Initialize called");

            textAnimationCoroutines = new Coroutine[tabButtons.Length];

            for (int i = 0; i < tabPanels.Length; i++)
            {
                if (tabPanels[i] != null)
                {
                    tabPanels[i].SetActive(false);
                }

                if (tabSelectionIndicators != null && i < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[i].SetActive(false);
                }
            }

            allClefs = new GameObject[] { weaponClef, maskClef, potionsCharmsClef };

            if (inventorySlotSelectors != null)
            {
                foreach (GameObject selector in inventorySlotSelectors)
                {
                    if (selector != null)
                    {
                        selector.SetActive(false);
                    }
                }
            }

            foreach (GameObject clef in allClefs)
            {
                if (clef != null)
                {
                    clef.SetActive(false);
                }
            }

            if (inventorySlotButtons != null)
            {
                for (int i = 0; i < inventorySlotButtons.Length; i++)
                {
                    if (inventorySlotButtons[i] != null)
                    {
                        RectTransform buttonRect = inventorySlotButtons[i].GetComponent<RectTransform>();

                        if (buttonRect != null)
                        {
                            slotOriginalScales[buttonRect] = buttonRect.localScale;
                        }

                        Image buttonImage = inventorySlotButtons[i].GetComponent<Image>();
                        if (buttonImage != null)
                        {
                            slotImageOriginalScales[buttonImage] = buttonImage.rectTransform.localScale;
                            slotImageOriginalAlpha[buttonImage] = buttonImage.color.a;
                        }

                        Image[] childImages = inventorySlotButtons[i].GetComponentsInChildren<Image>();

                        foreach (Image childImage in childImages)
                        {
                            if (childImage != buttonImage)
                            {
                                slotImageOriginalScales[childImage] = childImage.rectTransform.localScale;
                            }
                        }

                        slotIsHovering[i] = false;

                        int slotIndex = i;
                        EventTrigger eventTrigger = inventorySlotButtons[i].GetComponent<EventTrigger>();

                        if (eventTrigger == null)
                        {
                            eventTrigger = inventorySlotButtons[i].gameObject.AddComponent<EventTrigger>();
                        }


                        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => OnInventorySlotHoverEnter(slotIndex));
                        AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerExit, (data) => OnInventorySlotHoverExit(slotIndex));

                        AddEventTriggerEntry(eventTrigger, EventTriggerType.Submit, (data) => OnInventorySlotSubmit(slotIndex));

                        inventorySlotButtons[i].onClick.RemoveAllListeners();
                        inventorySlotButtons[i].onClick.AddListener(() => { OnInventorySlotClick(slotIndex); SelectInventorySlot(slotIndex); });
                    }
                }
            }

            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null)
                {
                    if (tabTexts != null && i < tabTexts.Length && tabTexts[i] != null)
                    {
                        SetTextToDefaultSize(tabTexts[i]);
                        tabTexts[i].color = tabUnselectedColor;
                    }

                    int tabIndex = i;
                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => SelectTab(tabIndex));

                    ColorBlock colors = tabButtons[i].colors;
                    colors.colorMultiplier = 1f;
                    colors.disabledColor = Color.white;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = Color.white;
                    colors.pressedColor = Color.white;
                    colors.selectedColor = Color.white;
                    tabButtons[i].colors = colors;

                    EventTrigger eventTrigger = tabButtons[i].GetComponent<EventTrigger>();

                    if (eventTrigger == null)
                    {
                        eventTrigger = tabButtons[i].gameObject.AddComponent<EventTrigger>();
                    }

                    eventTrigger.triggers.Clear();

                    int index = i;

                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => OnTabHoverEnter(index));
                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerExit, (data) => OnTabHoverExit(index));
                }
            }

            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (uiInputSwitcher == null)
            {
                GameObject uiInputSwitcherObject = new GameObject("UIInputSwitcher");
                uiInputSwitcher = uiInputSwitcherObject.AddComponent<UIInputSwitcher>();
            }

            SelectTab(0);
        }

        void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();

            entry.eventID = type;
            entry.callback.AddListener((data) => { action((BaseEventData)data); });

            trigger.triggers.Add(entry);
        }

        public override void Show()
        {
            base.Show();

            UpdateCharms();

            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(true);
            }

            Time.timeScale = 0.0f;

            LogDebug("PauseMenuInventoryController Show called");
            SelectTab(currentTabIndex);
        }

        public override void Hide()
        {
            base.Hide();

            LogDebug("PauseMenuInventoryController Hide called (including global elements)");
        }

        void OnCharmCollected(GameObject charm)
        {
            UpdateCharms();
        }

        public override void HandleInput()
        {
            base.HandleInput();

            if (gameInputSO != null && gameInputSO.playerInputActions != null)
            {
                if (gameInputSO.playerInputActions.UI.MoveLeftShoulder.WasPressedThisFrame())
                {
                    NavigateLeft();
                }
                else if (gameInputSO.playerInputActions.UI.MoveRightShoulder.WasPressedThisFrame())
                {
                    NavigateRight();
                }

                if (currentTabIndex == 1)
                {
                    Vector2 navDirection = gameInputSO.playerInputActions.UI.Navigate.ReadValue<Vector2>();

                    if (navDirection.sqrMagnitude > 0.5f)
                    {
                    }

                    if (gameInputSO.playerInputActions.UI.Submit.WasPressedThisFrame() && currentInventorySlotIndex >= 0 && currentInventorySlotIndex < inventorySlotButtons.Length)
                    {
                        OnInventorySlotSubmit(currentInventorySlotIndex);
                    }
                }
            }
        }

        void OnDestroy()
        {
            ProjectColombo.GameManagement.Events.CustomEvents.OnCharmCollected -= OnCharmCollected;

            for (int i = 0; i < textAnimationCoroutines.Length; i++)
            {
                if (textAnimationCoroutines[i] != null)
                {
                    StopCoroutine(textAnimationCoroutines[i]);
                }
            }

            if (selectorAnimationCoroutine != null)
            {
                StopCoroutine(selectorAnimationCoroutine);
            }

            foreach (var coroutine in slotHoverCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }

            foreach (var coroutine in slotSubmitCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
        }

        void Update()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != null && currentSelected.GetComponent<Button>() != null)
            {
                lastSelectedButton = currentSelected;

                Button selectedButton = currentSelected.GetComponent<Button>();

                for (int i = 0; i < inventorySlotButtons.Length; i++)
                {
                    if (selectedButton == inventorySlotButtons[i] && currentInventorySlotIndex != i)
                    {
                        SelectInventorySlot(i);
                        break;
                    }
                }
            }

            if (EventSystem.current.currentSelectedGameObject == null && uiInputSwitcher != null)
            {
                GameObject buttonToSelect = lastSelectedButton;

                if (buttonToSelect == null && currentTabIndex < firstSelectedButton.Length)
                {
                    buttonToSelect = firstSelectedButton[currentTabIndex];
                }

                if (buttonToSelect != null)
                {
                    EventSystem.current.SetSelectedGameObject(buttonToSelect);
                }
            }

#if UNITY_EDITOR   
            if (Gamepad.current != null)
            {
                Vector2 navVec = Gamepad.current.dpad.ReadValue();
                if (navVec != Vector2.zero)
                    Debug.Log($"<color=#00FFAA>[D-Pad] {navVec}</color>");
            }
#endif
        }

        void NavigateLeft()
        {
            int newIndex = currentTabIndex - 1;

            if (newIndex < 0)
            {
                newIndex = tabButtons.Length - 1;
            }

            SelectTab(newIndex);
        }

        void NavigateRight()
        {
            int newIndex = (currentTabIndex + 1) % tabButtons.Length;
            SelectTab(newIndex);
        }

        void OnTabHoverEnter(int index)
        {
            if (index == currentTabIndex)
            {
                return;
            }

            if (tabTexts != null && index < tabTexts.Length && tabTexts[index] != null)
            {
                tabTexts[index].color = tabHoverColor;
            }
        }

        void OnTabHoverExit(int index)
        {
            if (index == currentTabIndex)
            {
                return;
            }

            if (tabTexts != null && index < tabTexts.Length && tabTexts[index] != null)
            {
                tabTexts[index].color = tabUnselectedColor;
            }
        }

        public void SelectTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabButtons.Length || tabIndex >= tabPanels.Length)
            {
                Debug.LogWarning("Invalid tab index: " + tabIndex);
                return;
            }

            LogDebug($"Selecting tab {tabIndex}");

            if (currentTabIndex >= 0 && currentTabIndex < tabPanels.Length)
            {
                tabPanels[currentTabIndex].SetActive(false);

                if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[currentTabIndex].SetActive(false);
                }

                if (tabTexts != null && currentTabIndex < tabTexts.Length && tabTexts[currentTabIndex] != null)
                {
                    if (textAnimationCoroutines[currentTabIndex] != null)
                    {
                        StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                    }

                    textAnimationCoroutines[currentTabIndex] = StartCoroutine(AnimateTextSize(tabTexts[currentTabIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));

                    tabTexts[currentTabIndex].color = tabUnselectedColor;
                }
            }

            currentTabIndex = tabIndex;
            tabPanels[currentTabIndex].SetActive(true);

            if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
            {
                tabSelectionIndicators[currentTabIndex].SetActive(true);
            }

            if (tabTexts != null && currentTabIndex < tabTexts.Length && tabTexts[currentTabIndex] != null)
            {
                if (textAnimationCoroutines[currentTabIndex] != null)
                {
                    StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                }

                textAnimationCoroutines[currentTabIndex] = StartCoroutine(AnimateTextSize(tabTexts[currentTabIndex], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));

                tabTexts[currentTabIndex].color = tabSelectedColor;
            }

            if (currentTabIndex == 1)
            {
                if (currentInventorySlotIndex >= 0 && currentInventorySlotIndex < inventorySlotButtons.Length)
                {
                    SelectInventorySlot(currentInventorySlotIndex);
                }
                else if (inventorySlotButtons.Length > 0)
                {
                    SelectInventorySlot(0);
                }
            }


            if (firstSelectedButton != null && currentTabIndex < firstSelectedButton.Length && firstSelectedButton[currentTabIndex] != null)
            {
                GameObject buttonToSelect = firstSelectedButton[currentTabIndex];

                EventSystem.current.SetSelectedGameObject(buttonToSelect);

                lastSelectedButton = buttonToSelect;

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(buttonToSelect);
                }

                if (currentTabIndex == 1)
                {
                    Button inventoryButton = buttonToSelect.GetComponent<Button>();

                    if (inventoryButton != null)
                    {
                        for (int i = 0; i < inventorySlotButtons.Length; i++)
                        {
                            if (inventoryButton == inventorySlotButtons[i])
                            {
                                SelectInventorySlot(i);
                                break;
                            }
                        }
                    }
                }
            }

            if (tabButtons[tabIndex] != null)
            {
                RectTransform rectTransform = tabButtons[tabIndex].GetComponent<RectTransform>();
                PlayButtonClickAnimation(rectTransform);
            }
        }

        public void SelectInventorySlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length || slotIndex >= inventorySlotSelectors.Length)
            {
                Debug.LogWarning("Invalid inventory slot index: " + slotIndex);
                return;
            }

            if (currentInventorySlotIndex >= 0 && currentInventorySlotIndex < inventorySlotSelectors.Length)
            {
                if (inventorySlotSelectors[currentInventorySlotIndex] != null)
                {
                    inventorySlotSelectors[currentInventorySlotIndex].SetActive(false);

                    if (selectorAnimationCoroutine != null)
                    {
                        StopCoroutine(selectorAnimationCoroutine);
                        selectorAnimationCoroutine = null;
                    }

                    slotIsHovering[currentInventorySlotIndex] = false;
                }
            }

            currentInventorySlotIndex = slotIndex;

            if (inventorySlotSelectors[currentInventorySlotIndex] != null)
            {
                inventorySlotSelectors[currentInventorySlotIndex].SetActive(true);

                // FLICKER ANIMATION
                selectorAnimationCoroutine = StartCoroutine(AnimateSelectorFlicker(inventorySlotSelectors[currentInventorySlotIndex]));

                // BOUNCE ANIMATION
                // selectorAnimationCoroutine = StartCoroutine(AnimateSelectorBounce(inventorySlotSelectors[currentInventorySlotIndex]));
            }

            if (inventorySlotButtons[currentInventorySlotIndex] != null)
            {
                currentSelectedInventoryButton = inventorySlotButtons[currentInventorySlotIndex];
            }

            UpdatePentagramClef(slotIndex);

            if (inventorySlotButtons[slotIndex] != null)
            {
                RectTransform rectTransform = inventorySlotButtons[slotIndex].GetComponent<RectTransform>();
                PlayButtonClickAnimation(rectTransform);
            }
        }

        void OnInventorySlotSelected(int slotIndex)
        {
            if (currentInventorySlotIndex != slotIndex)
            {
                SelectInventorySlot(slotIndex);
            }
        }

        void UpdatePentagramClef(int slotIndex)
        {
            foreach (GameObject clef in allClefs)
            {
                if (clef != null)
                {
                    clef.SetActive(false);
                }
            }

            if (weaponPentagram != null && IsChildOfGameObject(inventorySlotButtons[slotIndex].gameObject, weaponPentagram))
            {
                if (weaponClef != null)
                {
                    weaponClef.SetActive(true);
                }
            }
            else if (maskPentagram != null && IsChildOfGameObject(inventorySlotButtons[slotIndex].gameObject, maskPentagram))
            {
                if (maskClef != null)
                {
                    maskClef.SetActive(true);
                }
            }
            else if (potionsCharmsPentagram != null && IsChildOfGameObject(inventorySlotButtons[slotIndex].gameObject, potionsCharmsPentagram))
            {
                if (potionsCharmsClef != null)
                {
                    potionsCharmsClef.SetActive(true);
                }
            }
        }

        public void UpdateCharms()
        {
            // Guard against missing references
            if (charmSlotButtons == null || charmSlotButtons.Length == 0)
            {
                LogDebug("No charm buttons assigned! Cannot update charms display.");
                return;
            }

            // Get player inventory
            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();

            // Reset all charm buttons first
            foreach (Button button in charmSlotButtons)
            {
                if (button != null)
                {
                    // Keep the button visible but non-interactable when empty
                    button.interactable = false;

                    // Find the child image and set it to the empty slot sprite
                    Image[] images = button.GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        if (img.gameObject != button.gameObject) // This is the child image
                        {
                            // Use the empty slot sprite we specified
                            if (emptyCharmSlotSprite != null)
                            {
                                img.sprite = emptyCharmSlotSprite;
                            }
                            img.color = new Color(1, 1, 1, 1); // Keep fully visible
                            break;
                        }
                    }

                    // Clear charm data
                    CharmButton charmButtonComponent = button.GetComponent<CharmButton>();
                    if (charmButtonComponent != null)
                    {
                        charmButtonComponent.charmObject = null;

                        // If CharmButton has an imageSlot field, set that directly
                        if (charmButtonComponent.imageSlot != null)
                        {
                            charmButtonComponent.imageSlot.sprite = emptyCharmSlotSprite;
                        }
                    }

                    button.onClick.RemoveAllListeners();
                }
            }

            // Reset legendary charm button
            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.interactable = false;

                // Find the child image and set it to the empty legendary slot sprite
                Image[] images = legendaryCharmButton.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.gameObject != legendaryCharmButton.gameObject)
                    {
                        // Use the empty legendary slot sprite we specified
                        if (emptyLegendaryCharmSlotSprite != null)
                        {
                            img.sprite = emptyLegendaryCharmSlotSprite;
                        }
                        else if (emptyCharmSlotSprite != null)
                        {
                            img.sprite = emptyCharmSlotSprite; // Fall back to regular empty slot sprite
                        }
                        img.color = new Color(1, 1, 1, 1); // Keep fully visible
                        break;
                    }
                }

                // Clear charm data
                CharmButton charmButtonComponent = legendaryCharmButton.GetComponent<CharmButton>();
                if (charmButtonComponent != null)
                {
                    charmButtonComponent.charmObject = null;

                    // If CharmButton has an imageSlot field, set that directly
                    if (charmButtonComponent.imageSlot != null)
                    {
                        charmButtonComponent.imageSlot.sprite = emptyLegendaryCharmSlotSprite != null ?
                            emptyLegendaryCharmSlotSprite : emptyCharmSlotSprite;
                    }
                }

                legendaryCharmButton.onClick.RemoveAllListeners();
            }

            // Update regular charm buttons with actual charms
            for (int i = 0; i < Mathf.Min(inventory.charms.Count, charmSlotButtons.Length); i++)
            {
                GameObject charm = inventory.charms[i];
                if (charm != null)
                {
                    BaseCharm charmComponent = charm.GetComponent<BaseCharm>();
                    Button button = charmSlotButtons[i];

                    if (charmComponent != null && button != null)
                    {
                        // Enable the button
                        button.interactable = true;

                        // Find and update the image
                        Image[] images = button.GetComponentsInChildren<Image>();
                        foreach (Image img in images)
                        {
                            if (img.gameObject != button.gameObject)
                            {
                                img.sprite = charmComponent.charmPicture;
                                img.color = new Color(1, 1, 1, 1); // Make fully visible
                                break;
                            }
                        }

                        // Store charm reference in the button
                        CharmButton charmButtonComponent = button.GetComponent<CharmButton>();
                        if (charmButtonComponent != null)
                        {
                            charmButtonComponent.UpdateInfo(charm);
                        }
                        else
                        {
                            // Using onClick event
                            button.onClick.AddListener(() => {
                                // Show charm details
                                if (charmNameText != null)
                                    charmNameText.text = charmComponent.charmName;

                                if (charmDescriptionText != null)
                                    charmDescriptionText.text = charmComponent.charmDescription;
                            });
                        }
                    }
                }
            }

            // Update legendary charm button
            if (legendaryCharmButton != null && inventory.legendaryCharms.Count > 0)
            {
                GameObject legCharm = inventory.legendaryCharms[0];
                if (legCharm != null)
                {
                    BaseCharm legCharmComponent = legCharm.GetComponent<BaseCharm>();

                    if (legCharmComponent != null)
                    {
                        // Enable the button
                        legendaryCharmButton.interactable = true;

                        // Find and update the image
                        Image[] images = legendaryCharmButton.GetComponentsInChildren<Image>();
                        foreach (Image img in images)
                        {
                            if (img.gameObject != legendaryCharmButton.gameObject)
                            {
                                img.sprite = legCharmComponent.charmPicture;
                                img.color = new Color(1, 1, 1, 1);
                                break;
                            }
                        }

                        // Store charm reference in the button
                        CharmButton charmButtonComponent = legendaryCharmButton.GetComponent<CharmButton>();
                        if (charmButtonComponent != null)
                        {
                            charmButtonComponent.UpdateInfo(legCharm);
                        }
                        else
                        {
                            // Using onClick event
                            legendaryCharmButton.onClick.AddListener(() => {
                                if (charmNameText != null)
                                    charmNameText.text = legCharmComponent.charmName;

                                if (charmDescriptionText != null)
                                    charmDescriptionText.text = legCharmComponent.charmDescription;
                            });
                        }
                    }
                }
            }

            LogDebug("Charm display updated in pause menu");
        }


        bool IsChildOfGameObject(GameObject child, GameObject parent)
        {
            Transform childTransform = child.transform;
            Transform parentTransform = parent.transform;

            while (childTransform != null)
            {
                if (childTransform == parentTransform)
                {
                    return true;
                }

                childTransform = childTransform.parent;
            }

            return false;
        }

        void OnInventorySlotHoverEnter(int slotIndex)
        {
            if (slotIndex == currentInventorySlotIndex)
            {
                return;
            }

            slotIsHovering[slotIndex] = true;

            Button slotButton = inventorySlotButtons[slotIndex];

            if (slotButton == null)
            {
                return;
            }

            RectTransform buttonRect = slotButton.GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                return;
            }

            Image buttonImage = slotButton.GetComponent<Image>();
            Image[] childImages = slotButton.GetComponentsInChildren<Image>();

            if (slotHoverCoroutines.ContainsKey(slotIndex) && slotHoverCoroutines[slotIndex] != null)
            {
                StopCoroutine(slotHoverCoroutines[slotIndex]);
            }

            slotHoverCoroutines[slotIndex] = StartCoroutine(AnimateSlotHover(slotIndex, buttonRect, buttonImage, childImages, true));
        }

        void OnInventorySlotHoverExit(int slotIndex)
        {
            slotIsHovering[slotIndex] = false;

            Button slotButton = inventorySlotButtons[slotIndex];

            if (slotButton == null)
            {
                return;
            }

            RectTransform buttonRect = slotButton.GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                return;
            }

            Image buttonImage = slotButton.GetComponent<Image>();
            Image[] childImages = slotButton.GetComponentsInChildren<Image>();

            if (slotHoverCoroutines.ContainsKey(slotIndex) && slotHoverCoroutines[slotIndex] != null)
            {
                StopCoroutine(slotHoverCoroutines[slotIndex]);
            }

            slotHoverCoroutines[slotIndex] = StartCoroutine(AnimateSlotHover(slotIndex, buttonRect, buttonImage, childImages, false));
        }

        void OnInventorySlotSubmit(int slotIndex)
        {
            Button slotButton = inventorySlotButtons[slotIndex];

            if (slotButton == null)
            {
                return;
            }

            RectTransform buttonRect = slotButton.GetComponent<RectTransform>();

            if (buttonRect == null)
            {
                return;
            }

            PlaySlotSubmitAnimation(slotIndex, buttonRect);
        }

        void OnInventorySlotClick(int slotIndex)
        {
            OnInventorySlotSubmit(slotIndex);
        }

        void PlaySlotSubmitAnimation(int slotIndex, RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            Vector3 originalScale = Vector3.one;

            if (slotOriginalScales.ContainsKey(rectTransform))
            {
                originalScale = slotOriginalScales[rectTransform];
            }
            else
            {
                originalScale = rectTransform.localScale;
                slotOriginalScales[rectTransform] = originalScale;
            }

            rectTransform.localScale = originalScale * slotSubmitShrinkScale;

            if (slotSubmitCoroutines.ContainsKey(slotIndex) && slotSubmitCoroutines[slotIndex] != null)
            {
                StopCoroutine(slotSubmitCoroutines[slotIndex]);
            }

            slotSubmitCoroutines[slotIndex] = StartCoroutine(SlotShrinkAndBounceAnimation(slotIndex, rectTransform, originalScale));
        }

        IEnumerator AnimateSelectorFlicker(GameObject selector)
        {
            if (selector == null)
            {
                yield break;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                yield break;
            }

            Color originalColor = selectorImage.color;

            float time = 0f;

            while (true)
            {
                float alpha = Mathf.Lerp(selectorFlickerMinAlpha, selectorFlickerMaxAlpha, (Mathf.Sin(time * selectorFlickerSpeed) + 1f) * 0.5f);

                Color newColor = originalColor;
                newColor.a = alpha;
                selectorImage.color = newColor;

                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        IEnumerator AnimateSelectorBounce(GameObject selector)
        {
            if (selector == null)
            {
                yield break;
            }

            RectTransform selectorRect = selector.GetComponent<RectTransform>();

            if (selectorRect == null)
            {
                yield break;
            }

            Vector3 originalScale = selectorRect.localScale;

            float time = 0f;

            while (true)
            {
                float scale = Mathf.Lerp(selectorBouncingMinScale, selectorBouncingMaxScale, (Mathf.Sin(time * selectorBouncingSpeed) + 1f) * 0.5f);

                selectorRect.localScale = originalScale * scale;

                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        IEnumerator AnimateSlotHover(int slotIndex, RectTransform buttonRect, Image buttonImage, Image[] childImages, bool hoverIn)
        {
            if (buttonRect == null)
            {
                yield break;
            }

            Vector3 buttonOriginalScale = Vector3.one;

            if (slotOriginalScales.ContainsKey(buttonRect))
            {
                buttonOriginalScale = slotOriginalScales[buttonRect];
            }
            else
            {
                buttonOriginalScale = buttonRect.localScale;
                slotOriginalScales[buttonRect] = buttonOriginalScale;
            }

            Vector3 targetButtonScale = hoverIn ? buttonOriginalScale * (1f + slotHoverScaleIncrease) : buttonOriginalScale;

            float targetAlpha = 1f;

            if (buttonImage != null)
            {
                float originalAlpha = 1f;

                if (slotImageOriginalAlpha.ContainsKey(buttonImage))
                {
                    originalAlpha = slotImageOriginalAlpha[buttonImage];
                }
                else
                {
                    originalAlpha = buttonImage.color.a;
                    slotImageOriginalAlpha[buttonImage] = originalAlpha;
                }

                targetAlpha = hoverIn ? Mathf.Clamp01(originalAlpha + slotHoverAlphaIncrease) : originalAlpha;
            }

            float duration = 0.2f;
            float elapsedTime = 0f;

            Vector3 startButtonScale = buttonRect.localScale;

            float startAlpha = buttonImage != null ? buttonImage.color.a : 1f;

            Dictionary<Image, Vector3> startChildScales = new Dictionary<Image, Vector3>();

            foreach (Image childImage in childImages)
            {
                if (childImage != buttonImage)
                {
                    startChildScales[childImage] = childImage.rectTransform.localScale;
                }
            }

            while (elapsedTime < duration)
            {
                if (hoverIn != slotIsHovering[slotIndex])
                {
                    yield break;
                }

                float time = elapsedTime / duration;
                float smoothT = Mathf.SmoothStep(0f, 1f, time);

                buttonRect.localScale = Vector3.Lerp(startButtonScale, targetButtonScale, smoothT);

                if (buttonImage != null)
                {
                    Color color = buttonImage.color;
                    color.a = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
                    buttonImage.color = color;
                }

                foreach (Image childImage in childImages)
                {
                    if (childImage != buttonImage)
                    {
                        Vector3 childOriginalScale = Vector3.one;

                        if (slotImageOriginalScales.ContainsKey(childImage))
                        {
                            childOriginalScale = slotImageOriginalScales[childImage];
                        }
                        else
                        {
                            childOriginalScale = childImage.rectTransform.localScale;
                            slotImageOriginalScales[childImage] = childOriginalScale;
                        }

                        Vector3 childStartScale = startChildScales[childImage];
                        Vector3 childTargetScale = hoverIn ? childOriginalScale * (1f + slotHoverScaleIncrease) : childOriginalScale;

                        childImage.rectTransform.localScale = Vector3.Lerp(childStartScale, childTargetScale, smoothT);
                    }
                }

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            buttonRect.localScale = targetButtonScale;

            if (buttonImage != null)
            {
                Color color = buttonImage.color;
                color.a = targetAlpha;
                buttonImage.color = color;
            }

            foreach (Image childImage in childImages)
            {
                if (childImage != buttonImage)
                {
                    Vector3 childOriginalScale = Vector3.one;

                    if (slotImageOriginalScales.ContainsKey(childImage))
                    {
                        childOriginalScale = slotImageOriginalScales[childImage];
                    }

                    childImage.rectTransform.localScale = hoverIn ? childOriginalScale * (1f + slotHoverScaleIncrease) : childOriginalScale;
                }
            }

            slotHoverCoroutines[slotIndex] = null;
        }

        IEnumerator SlotShrinkAndBounceAnimation(int slotIndex, RectTransform rectTransform, Vector3 originalScale)
        {
            if (rectTransform == null)
            {
                yield break;
            }

            Vector3 targetScale = originalScale * slotSubmitShrinkScale;

            rectTransform.localScale = targetScale;

            yield return new WaitForSecondsRealtime(slotSubmitShrinkDuration);

            float elapsedTime = 0f;

            while (elapsedTime < slotSubmitBounceDuration)
            {
                float t = elapsedTime / slotSubmitBounceDuration;
                float curveValue = clickBounceCurve.Evaluate(t);

                Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, curveValue);
                rectTransform.localScale = currentScale;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            rectTransform.localScale = originalScale;

            slotSubmitCoroutines[slotIndex] = null;
        }

        // Debug logging method
        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[PauseMenuInventory] {message}</color>");
            }
        }
    }
}