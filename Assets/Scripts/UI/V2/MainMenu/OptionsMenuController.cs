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
        [SerializeField] Button[] tabButtons;

        [Header("UI Manager Reference")]
        [SerializeField] UIManagerV2 uiManagerV2;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        TextMeshProUGUI[] tabTexts;

        [Header("Tab Colors")]
        [SerializeField] Color tabOriginalColor = Color.black;
        [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);

        int currentTabIndex = 0;

        bool hasBeenInitialized = false;


        Coroutine[] textAnimationCoroutines;



        OptionsMenuNavigationExtension navigationExtension;


        void OnEnable()
        {
            if (hasBeenInitialized)
            {
                RefreshUI();
            }
            else
            {
                Initialize();
                hasBeenInitialized = true;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (uiManagerV2 == null)
            {
                uiManagerV2 = FindFirstObjectByType<UIManagerV2>();
            }

            tabTexts = new TextMeshProUGUI[tabButtons.Length];
            textAnimationCoroutines = new Coroutine[tabButtons.Length];

            for (int i = 0; i < tabScreens.Length; i++)
            {
                tabScreens[i].SetActive(false);
                if (tabSelectionIndicators != null && i < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[i].SetActive(false);
                }
            }

            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null)
                {
                    tabTexts[i] = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                    if (tabTexts[i] != null)
                    {
                        SetTextToDefaultSize(tabTexts[i]);
                        tabTexts[i].color = tabOriginalColor;
                    }

                    int buttonIndex = i;

                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => SelectTab(buttonIndex));

                    ColorBlock colors = tabButtons[i].colors;
                    colors.colorMultiplier = 1f;
                    colors.disabledColor = Color.white;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = Color.white;
                    colors.pressedColor = Color.white;
                    colors.selectedColor = Color.white;
                    tabButtons[i].colors = colors;

                    EventTrigger eventTrigger = tabButtons[i].gameObject.GetComponent<EventTrigger>();

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

            navigationExtension = GetComponent<OptionsMenuNavigationExtension>();

            if (navigationExtension == null)
            {
                navigationExtension = gameObject.AddComponent<OptionsMenuNavigationExtension>();

                if (tabScreens != null && tabScreens.Length >= 3)
                {
                    navigationExtension.GetType().GetField("graphicsTab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, tabScreens[0]);

                    navigationExtension.GetType().GetField("audioTab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, tabScreens[1]);

                    navigationExtension.GetType().GetField("controlsTab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, tabScreens[2]);
                }
            }
        }

        protected override void SetupButtonNavigation()
        {
            if (tabButtons == null || tabButtons.Length == 0) return;

            LogDebug("Setting up navigation for " + tabButtons.Length + " tab buttons");

            for (int i = 0; i < tabButtons.Length; i++)
            {
                Button button = tabButtons[i];
                if (button == null) continue;

                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.Explicit;

                if (i > 0)
                {
                    nav.selectOnLeft = tabButtons[i - 1];
                    LogDebug($"Tab {button.name} selectOnLeft = {tabButtons[i - 1].name}");
                }
                else if (wrapNavigation)
                {
                    nav.selectOnLeft = tabButtons[tabButtons.Length - 1];
                    LogDebug($"Tab {button.name} selectOnLeft = {tabButtons[tabButtons.Length - 1].name} (wrap)");
                }

                if (i < tabButtons.Length - 1)
                {
                    nav.selectOnRight = tabButtons[i + 1];
                    LogDebug($"Tab {button.name} selectOnRight = {tabButtons[i + 1].name}");
                }
                else if (wrapNavigation)
                {
                    nav.selectOnRight = tabButtons[0];
                    LogDebug($"Tab {button.name} selectOnRight = {tabButtons[0].name} (wrap)");
                }

                button.navigation = nav;
            }

            for (int i = 0; i < tabScreens.Length; i++)
            {
                if (tabScreens[i] == null) continue;

                Button[] contentButtons = tabScreens[i].GetComponentsInChildren<Button>(true);
                if (contentButtons.Length == 0) continue;

                LogDebug($"Setting up {contentButtons.Length} content buttons for tab {i}");

                for (int j = 0; j < contentButtons.Length; j++)
                {
                    Button button = contentButtons[j];
                    Navigation nav = button.navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    if (j > 0)
                    {
                        nav.selectOnUp = contentButtons[j - 1];
                    }
                    else if (wrapNavigation)
                    {
                        nav.selectOnUp = contentButtons[contentButtons.Length - 1];
                    }

                    if (j < contentButtons.Length - 1)
                    {
                        nav.selectOnDown = contentButtons[j + 1];
                    }
                    else if (wrapNavigation)
                    {
                        nav.selectOnDown = contentButtons[0];
                    }

                    button.navigation = nav;
                }
            }

            if (tabButtons.Length > 0 && tabButtons[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(tabButtons[0].gameObject);
                LogDebug("Set initial selection to tab " + tabButtons[0].name);
            }
        }

        public override void Show()
        {
            base.Show();

            if (menuContainer != null)
            {
                menuContainer.SetActive(true);
            }

            if (gameInputSO != null)
            {
                gameInputSO.EnableUIMode();
            }

            RefreshUI();
        }

        void RefreshUI()
        {
            int previousIndex = currentTabIndex;

            currentTabIndex = -1;

            SelectTab(previousIndex);

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

            if (menuContainer != null)
            {
                menuContainer.SetActive(false);
            }
        }

        public override void HandleInput()
        {
            base.HandleInput();

            if (gameInputSO != null && gameInputSO.playerInputActions != null)
            {
                if (gameInputSO.playerInputActions.UI.MoveLeftShoulder.WasPressedThisFrame() && CanNavigate())
                {
                    NavigateLeft();
                    LogDebug("Left shoulder button pressed - navigating left");
                }
                else if (gameInputSO.playerInputActions.UI.MoveRightShoulder.WasPressedThisFrame() && CanNavigate())
                {
                    NavigateRight();
                    LogDebug("Right shoulder button pressed - navigating right");
                }

                if (gameInputSO.playerInputActions.UI.Cancel.WasPressedThisFrame())
                {
                    LogDebug("Cancel button pressed - returning to main menu");
                    ReturnToMainMenu();
                }
            }
        }

        void ReturnToMainMenu()
        {
            LogDebug("Returning to main menu");

            if (uiManagerV2 != null)
            {
                Hide();
                uiManagerV2.ShowMainMenu();
            }
            else
            {
                Hide();

                if (transform.parent != null)
                {
                    Transform mainMenuTransform = transform.parent.Find("MainMenu");
                    if (mainMenuTransform != null)
                    {
                        GameObject mainMenu = mainMenuTransform.gameObject;
                        mainMenu.SetActive(true);

                        MainMenuController mainMenuController = mainMenu.GetComponent<MainMenuController>();
                        if (mainMenuController != null)
                        {
                            mainMenuController.Show();
                        }
                    }
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
                tabTexts[index].color = tabOriginalColor;
            }
        }

        void SelectTab(int index)
        {
            if (index < 0 || index >= tabScreens.Length)
            {
                return;
            }

            if (currentTabIndex == index)
            {
                return;
            }

            LogDebug("Selecting tab " + index);

            if (currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                tabScreens[currentTabIndex].SetActive(false);

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

                    textAnimationCoroutines[currentTabIndex] = StartCoroutine(
                    AnimateTextSize(tabTexts[currentTabIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));

                    tabTexts[currentTabIndex].color = tabOriginalColor;
                }
            }

            currentTabIndex = index;
            tabScreens[currentTabIndex].SetActive(true);

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

                tabTexts[currentTabIndex].color = tabOriginalColor;
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

            if (tabButtons[index] != null)
            {
                RectTransform rectTransform = tabButtons[index].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    PlayButtonClickAnimation(rectTransform);
                }
            }

            if (navigationExtension != null)
            {
                navigationExtension.OnTabChanged(index);
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FFAA00>[OptionsMenuController] {message}</color>");
            }
        }
    }
}