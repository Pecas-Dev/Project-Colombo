using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ProjectColombo.UI
{
    public class OptionsMenuController : MenuController
    {
        [Header("Tab Navigation")]
        [SerializeField] GameObject[] tabScreens;
        [SerializeField] GameObject[] tabSelectionIndicators;
        [SerializeField] TextMeshProUGUI[] tabTitles;
    
    [Header("Tab Colors")]
    [SerializeField] Color tabOriginalColor = Color.black;
    [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);


        int currentTabIndex = 0;

        Coroutine[] textAnimationCoroutines;

        public override void Initialize()
        {
            base.Initialize();

            textAnimationCoroutines = new Coroutine[tabTitles.Length];

            for (int i = 0; i < tabScreens.Length; i++)
            {
                tabScreens[i].SetActive(false);
                if (tabSelectionIndicators != null && i < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[i].SetActive(false);
                }

                if (tabTitles != null && i < tabTitles.Length)
                {
                    SetTextToDefaultSize(tabTitles[i]);
                    
                    // Set initial color
                    tabTitles[i].color = tabOriginalColor;
                    
                    // Add event handlers for mouse hover and click
                    int index = i; // Capture the index for event handlers
                    
                    // Make sure the tab title is clickable
                    Button tabButton;
                    if (!tabTitles[i].gameObject.TryGetComponent<Button>(out tabButton))
                    {
                        tabButton = tabTitles[i].gameObject.AddComponent<Button>();
                        ColorBlock colors = tabButton.colors;
                        colors.disabledColor = tabOriginalColor;
                        colors.normalColor = tabOriginalColor;
                        colors.highlightedColor = tabOriginalColor; // We'll handle this with our own logic
                        colors.pressedColor = tabOriginalColor;
                        colors.selectedColor = tabOriginalColor;
                        tabButton.colors = colors;
                        
                        // Add a navigation group that includes all tabs
                        tabButton.navigation = new Navigation { mode = Navigation.Mode.Explicit };
                    }
                    
                    // Add click handler to the button
                    int buttonIndex = i;
                    tabButton.onClick.RemoveAllListeners();
                    tabButton.onClick.AddListener(() => SelectTab(buttonIndex));
                    
                    // Add EventTrigger if it doesn't exist
                    EventTrigger eventTrigger = tabTitles[i].gameObject.GetComponent<EventTrigger>();
                    if (eventTrigger == null)
                    {
                        eventTrigger = tabTitles[i].gameObject.AddComponent<EventTrigger>();
                    }
                    
                    // Clear existing triggers to avoid duplicates
                    eventTrigger.triggers.Clear();
                    
                    // Add hover events
                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => OnTabHoverEnter(index));
                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerExit, (data) => OnTabHoverExit(index));
                    
                    // Add click event
                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerClick, (data) => SelectTab(index));
                }
            }

            SelectTab(0);

            if (tabScreens.Length > 0 && currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                Button firstButton = tabScreens[currentTabIndex].GetComponentInChildren<Button>();
                if (firstButton != null)
                {
                    uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                    if (uiInputSwitcher != null)
                    {
                        uiInputSwitcher.SetFirstSelectedButton(firstButton.gameObject);
                    }
                }
            }
        }

        public override void Show()
        {
            base.Show();

            if (gameInputSO != null)
            {
                gameInputSO.EnableUIMode();
            }

            if (tabScreens.Length > 0 && currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                Button firstButton = tabScreens[currentTabIndex].GetComponentInChildren<Button>();
                if (firstButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstButton.gameObject);

                    if (uiInputSwitcher == null)
                    {
                        uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                    }

                    if (uiInputSwitcher != null)
                    {
                        uiInputSwitcher.SetFirstSelectedButton(firstButton.gameObject);
                    }
                }
            }
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void HandleInput()
        {
            if (gameInputSO != null)
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

        void NavigateLeft()
        {
            int newIndex = currentTabIndex - 1;

            if (newIndex < 0)
            {
                newIndex = tabScreens.Length - 1;
            }

            SelectTab(newIndex);
        }

        void NavigateRight()
        {
            int newIndex = (currentTabIndex + 1) % tabScreens.Length;

            SelectTab(newIndex);
        }
        
        void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((data) => { action((BaseEventData)data); });
            trigger.triggers.Add(entry);
        }
        
        void OnTabHoverEnter(int index)
        {
            // Don't change color if this is the selected tab
            if (index == currentTabIndex)
            {
                return;
            }
            
            // Change color on hover
            if (tabTitles != null && index < tabTitles.Length)
            {
                tabTitles[index].color = tabHoverColor;
            }
        }
        
        void OnTabHoverExit(int index)
        {
            // Don't change color if this is the selected tab
            if (index == currentTabIndex)
            {
                return;
            }
            
            // Reset color when mouse leaves
            if (tabTitles != null && index < tabTitles.Length)
            {
                tabTitles[index].color = tabOriginalColor;
            }
        }

        void SelectTab(int index)
        {
            if (index < 0 || index >= tabScreens.Length)
            {
                return;
            }

            if (currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                tabScreens[currentTabIndex].SetActive(false);

                if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[currentTabIndex].SetActive(false);
                }

                if (tabTitles != null && currentTabIndex < tabTitles.Length)
                {
                    if (textAnimationCoroutines[currentTabIndex] != null)
                    {
                        StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                    }

                    textAnimationCoroutines[currentTabIndex] = StartCoroutine(
                        AnimateTextSize(tabTitles[currentTabIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));
                    
                    // Reset color of previous tab
                    tabTitles[currentTabIndex].color = tabOriginalColor;
                }
            }

            currentTabIndex = index;
            tabScreens[currentTabIndex].SetActive(true);

            if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
            {
                tabSelectionIndicators[currentTabIndex].SetActive(true);
            }

            if (tabTitles != null && currentTabIndex < tabTitles.Length)
            {
                if (textAnimationCoroutines[currentTabIndex] != null)
                {
                    StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                }

                textAnimationCoroutines[currentTabIndex] = StartCoroutine(AnimateTextSize(tabTitles[currentTabIndex], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));
                
                // Set selected tab color
                tabTitles[currentTabIndex].color = tabOriginalColor;
            }

            Button firstButton = tabScreens[currentTabIndex].GetComponentInChildren<Button>();

            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);

                if (uiInputSwitcher == null)
                {
                    uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                }

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(firstButton.gameObject);
                }
            }
        }
    }
}