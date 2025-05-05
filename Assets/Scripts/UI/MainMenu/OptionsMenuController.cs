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

        private TextMeshProUGUI[] tabTexts;

        [Header("Tab Colors")]
        [SerializeField] Color tabOriginalColor = Color.black;
        [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);


        int currentTabIndex = 0;

        Coroutine[] textAnimationCoroutines;

        public override void Initialize()
        {
            base.Initialize();

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
        }
    }
}