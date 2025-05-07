using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryController : MenuController
    {
        [Header("Tab Navigation")]
        [SerializeField] Button[] tabButtons;
        [SerializeField] GameObject[] tabPanels;
        [SerializeField] TextMeshProUGUI[] tabTexts;
        [SerializeField] GameObject[] tabSelectionIndicators;
        [SerializeField] GameObject[] firstSelectedButtonForEachTab;

        [Header("Inventory Pentagram System")]
        [SerializeField] GameObject weaponPentagram;
        [SerializeField] GameObject maskPentagram;
        [SerializeField] GameObject potionsCharmsPentagram;
        [SerializeField] GameObject weaponClef;
        [SerializeField] GameObject maskClef;
        [SerializeField] GameObject potionsCharmsClef;
        [SerializeField] Button[] inventorySlotButtons;
        [SerializeField] GameObject[] inventorySlotSelectors;

        [Header("Tab Colors")]
        [SerializeField] Color tabSelectedColor = Color.white;
        [SerializeField] Color tabUnselectedColor = Color.gray;
        [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);


        int currentInventorySlotIndex = -1;
        int currentTabIndex = 0;


        GameObject lastSelectedButton;
        GameObject[] allClefs;

        Coroutine[] textAnimationCoroutines;

        Button currentSelectedInventoryButton;


        public override void Initialize()
        {
            base.Initialize();

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
                        int slotIndex = i;
                        inventorySlotButtons[i].onClick.AddListener(() => SelectInventorySlot(slotIndex));

                        EventTrigger eventTrigger = inventorySlotButtons[i].GetComponent<EventTrigger>();

                        if (eventTrigger == null)
                        {
                            eventTrigger = inventorySlotButtons[i].gameObject.AddComponent<EventTrigger>();
                        }

                        AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, (data) => OnInventorySlotSelected(slotIndex));
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

            SelectTab(currentTabIndex);
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
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < textAnimationCoroutines.Length; i++)
            {
                if (textAnimationCoroutines[i] != null)
                {
                    StopCoroutine(textAnimationCoroutines[i]);
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

                if (buttonToSelect == null && currentTabIndex < firstSelectedButtonForEachTab.Length)
                {
                    buttonToSelect = firstSelectedButtonForEachTab[currentTabIndex];
                }

                if (buttonToSelect != null)
                {
                    EventSystem.current.SetSelectedGameObject(buttonToSelect);
                }
            }
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

            if (firstSelectedButtonForEachTab != null && currentTabIndex < firstSelectedButtonForEachTab.Length && firstSelectedButtonForEachTab[currentTabIndex] != null)
            {
                GameObject buttonToSelect = firstSelectedButtonForEachTab[currentTabIndex];

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
                }
            }

            currentInventorySlotIndex = slotIndex;

            if (inventorySlotSelectors[currentInventorySlotIndex] != null)
            {
                inventorySlotSelectors[currentInventorySlotIndex].SetActive(true);
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
    }
}