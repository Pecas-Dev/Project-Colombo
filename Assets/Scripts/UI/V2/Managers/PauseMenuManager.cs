using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Tab References")]
        [SerializeField] GameObject inventoryTab;
        [SerializeField] GameObject statsTab;
        [SerializeField] GameObject settingsTab;

        [Header("Tab Title References")]
        [SerializeField] Button inventoryTitleButton;
        [SerializeField] Button statsTitleButton;
        [SerializeField] Button settingsTitleButton;

        [Header("Tab Selection Indicators")]
        [SerializeField] GameObject[] tabSelectionIndicators;

        [Header("Global Elements")]
        [SerializeField] GameObject[] globalElements;

        //[Header("First Selected Elements")]
        //[SerializeField] GameObject inventoryFirstSelected;
        //[SerializeField] GameObject settingsFirstSelected;

        [Header("Tab Text Animation")]
        [SerializeField] float selectedMinFontSize = 75f;
        [SerializeField] float selectedMaxFontSize = 105f;
        [SerializeField] float defaultMinFontSize = 70f;
        [SerializeField] float defaultMaxFontSize = 100f;
        [SerializeField] float animationDuration = 0.3f;

        [Header("Tab Colors")]
        [SerializeField] Color tabSelectedColor = Color.white;
        [SerializeField] Color tabUnselectedColor = Color.gray;
        [SerializeField] Color tabHoverColor = new Color(0.4415095f, 0f, 0.4200771f);

        [Header("Button Click Animation")]
        [SerializeField] float clickShrinkScale = 0.8f;
        [SerializeField] float clickShrinkDuration = 0.1f;
        [SerializeField] float clickBounceDuration = 0.2f;
        [SerializeField] AnimationCurve clickBounceCurve;

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;
        [SerializeField] float tabSwitchCooldown = 0.2f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        PauseMenuInventoryTabController inventoryController;
        PauseMenuStatsTabController statsController;
        PauseMenuSettingsTabController settingsController;

        int currentTabIndex = 0;
        float lastTabSwitchTime = -1f;

        bool previousLeftShoulderState = false;
        bool previousRightShoulderState = false;

        TextMeshProUGUI[] tabTexts;
        Coroutine[] textAnimationCoroutines;
        Dictionary<RectTransform, Vector3> originalButtonScales = new Dictionary<RectTransform, Vector3>();

        GameObject[] tabScreens;
        Button[] tabButtons;


        PauseMenuNavigationExtension navigationExtension;


        void Awake()
        {
            tabScreens = new GameObject[] { inventoryTab, statsTab, settingsTab };
            tabButtons = new Button[] { inventoryTitleButton, statsTitleButton, settingsTitleButton };

            Initialize();

            navigationExtension = GetComponent<PauseMenuNavigationExtension>();

            if (navigationExtension == null)
            {
                navigationExtension = gameObject.AddComponent<PauseMenuNavigationExtension>();

                //if (inventoryFirstSelected != null)
                //{
                //    navigationExtension.GetType().GetField("inventoryFirstSelected", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, inventoryFirstSelected);
                //}
                //if (settingsFirstSelected != null)
                //{
                //    navigationExtension.GetType().GetField("settingsFirstSelected", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, settingsFirstSelected);
                //}
            }
        }

        void OnEnable()
        {
            ShowPauseMenu();
        }

        void OnDisable()
        {
            HidePauseMenu();
            StopAllAnimations();
        }

        void Update()
        {
            HandleTabSwitchInput();
        }

        void Initialize()
        {
            if (clickBounceCurve == null || clickBounceCurve.keys.Length == 0)
            {
                Keyframe[] keys = new Keyframe[3];
                keys[0] = new Keyframe(0f, 0f, 0f, 2f);
                keys[1] = new Keyframe(0.5f, 1.1f, 0f, 0f);
                keys[2] = new Keyframe(1f, 1f, -1f, 0f);
                clickBounceCurve = new AnimationCurve(keys);
            }

            if (inventoryTab != null)
            {
                inventoryController = inventoryTab.GetComponent<PauseMenuInventoryTabController>();
            }

            if (statsTab != null)
            {
                statsController = statsTab.GetComponent<PauseMenuStatsTabController>();
            }

            if (settingsTab != null)
            {
                settingsController = settingsTab.GetComponent<PauseMenuSettingsTabController>();
            }

            tabTexts = new TextMeshProUGUI[tabButtons.Length];
            textAnimationCoroutines = new Coroutine[tabButtons.Length];

            for (int i = 0; i < tabScreens.Length; i++)
            {
                if (tabScreens[i] != null)
                {
                    tabScreens[i].SetActive(false);
                }

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
                        tabTexts[i].color = tabUnselectedColor;
                    }

                    int buttonIndex = i;

                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => SwitchToTab(buttonIndex));

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

                    RectTransform rectTransform = tabButtons[i].GetComponent<RectTransform>();
                    if (rectTransform != null && !originalButtonScales.ContainsKey(rectTransform))
                    {
                        originalButtonScales[rectTransform] = rectTransform.localScale;
                    }
                }
            }

            SetupTabNavigation();

            foreach (var button in tabButtons)
            {
                if (button != null)
                {
                    Navigation nav = button.navigation;
                    nav.mode = Navigation.Mode.None;
                    button.navigation = nav;
                }
            }

            LogDebug("Pause menu manager initialized");
        }

        void SetupTabNavigation()
        {
            if (tabButtons == null || tabButtons.Length == 0) return;

            for (int i = 0; i < tabButtons.Length; i++)
            {
                Button button = tabButtons[i];
                if (button == null) continue;

                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.Explicit;

                if (i > 0)
                {
                    nav.selectOnLeft = tabButtons[i - 1];
                }
                else
                {
                    nav.selectOnLeft = tabButtons[tabButtons.Length - 1];
                }

                if (i < tabButtons.Length - 1)
                {
                    nav.selectOnRight = tabButtons[i + 1];
                }
                else
                {
                    nav.selectOnRight = tabButtons[0];
                }

                button.navigation = nav;
            }
        }
        public void ShowPauseMenu()
        {
            LogDebug("Showing pause menu");

            if (tabSelectionIndicators != null && tabSelectionIndicators.Length > 0 && tabSelectionIndicators[0] != null)
            {
                tabSelectionIndicators[0].SetActive(true);
            }

            if (tabTexts != null && tabTexts.Length > 0 && tabTexts[0] != null)
            {
                tabTexts[0].color = tabSelectedColor;
            }

            foreach (var element in globalElements)
            {
                if (element != null)
                {
                    element.SetActive(true);
                }
            }

            SwitchToTab(0);

            if (gameInput != null)
            {
                gameInput.EnableUIMode();
            }
        }

        public void HidePauseMenu()
        {
            LogDebug("Hiding pause menu");

            for (int i = 0; i < tabScreens.Length; i++)
            {
                if (tabScreens[i] != null)
                {
                    tabScreens[i].SetActive(false);
                }
            }

            foreach (var element in globalElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }
        }

        void StopAllAnimations()
        {
            if (textAnimationCoroutines == null) return;

            for (int i = 0; i < textAnimationCoroutines.Length; i++)
            {
                if (textAnimationCoroutines[i] != null)
                {
                    StopCoroutine(textAnimationCoroutines[i]);
                    textAnimationCoroutines[i] = null;
                }
            }
        }
        public void SwitchToTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabScreens.Length || tabIndex == currentTabIndex)
            {
                return;
            }

            if (Time.unscaledTime - lastTabSwitchTime < tabSwitchCooldown)
            {
                return;
            }

            lastTabSwitchTime = Time.unscaledTime;
            LogDebug($"Switching to tab {tabIndex}");

            if (currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                if (tabScreens[currentTabIndex] != null)
                {
                    tabScreens[currentTabIndex].SetActive(false);
                }

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

                    tabTexts[currentTabIndex].color = tabUnselectedColor;
                }
            }

            currentTabIndex = tabIndex;

            if (tabScreens[currentTabIndex] != null)
            {
                tabScreens[currentTabIndex].SetActive(true);
            }

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

            GameObject firstSelected = null;

            //switch (currentTabIndex)
            //{
            //    case 0:
            //        firstSelected = inventoryFirstSelected;
            //        break;
            //    case 1:
            //        break;
            //    case 2:
            //        firstSelected = settingsFirstSelected;
            //        break;
            //}

            if (firstSelected != null)
            {
                StartCoroutine(DelayedSelectGameObject(firstSelected));
            }

            if (firstSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }

            if (tabButtons[currentTabIndex] != null)
            {
                tabButtons[currentTabIndex].Select();

                RectTransform rectTransform = tabButtons[currentTabIndex].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    PlayButtonClickAnimation(rectTransform);
                }
            }

            if (navigationExtension != null)
            {
                navigationExtension.OnTabChanged(tabIndex);
            }
        }

        void HandleTabSwitchInput()
        {
            if (gameInput != null && gameInput.playerInputActions != null)
            {
                if (gameInput.playerInputActions.UI.MoveLeftShoulder.WasPressedThisFrame())
                {
                    int newTab = (currentTabIndex == 0) ? tabScreens.Length - 1 : currentTabIndex - 1;
                    SwitchToTab(newTab);
                    return;
                }
                else if (gameInput.playerInputActions.UI.MoveRightShoulder.WasPressedThisFrame())
                {
                    int newTab = (currentTabIndex + 1) % tabScreens.Length;
                    SwitchToTab(newTab);
                    return;
                }
            }

            if (Gamepad.current != null)
            {
                bool leftShoulderPressed = Gamepad.current.leftShoulder.isPressed;
                bool rightShoulderPressed = Gamepad.current.rightShoulder.isPressed;

                if (leftShoulderPressed && !previousLeftShoulderState)
                {
                    int newTab = (currentTabIndex == 0) ? tabScreens.Length - 1 : currentTabIndex - 1;
                    SwitchToTab(newTab);
                }
                else if (rightShoulderPressed && !previousRightShoulderState)
                {
                    int newTab = (currentTabIndex + 1) % tabScreens.Length;
                    SwitchToTab(newTab);
                }

                previousLeftShoulderState = leftShoulderPressed;
                previousRightShoulderState = rightShoulderPressed;
            }
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


        void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = type
            };

            entry.callback.AddListener((data) => { action((BaseEventData)data); });
            trigger.triggers.Add(entry);
        }

        void PlayButtonClickAnimation(RectTransform rectTransform)
        {
            if (rectTransform == null) return;

            Vector3 originalScale = Vector3.one;

            if (originalButtonScales.ContainsKey(rectTransform))
            {
                originalScale = originalButtonScales[rectTransform];
            }
            else
            {
                originalScale = rectTransform.localScale;
                originalButtonScales[rectTransform] = originalScale;
            }

            rectTransform.localScale = originalScale * clickShrinkScale;

            try
            {
                StartCoroutine(ShrinkAndBounceAnimation(rectTransform, originalScale));
            }
            catch (System.Exception)
            {
                Application.onBeforeRender += () => ResetButtonScale(rectTransform, originalScale);
            }
        }

        void ResetButtonScale(RectTransform rectTransform, Vector3 originalScale)
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }

            Application.onBeforeRender -= () => ResetButtonScale(rectTransform, originalScale);
        }

        IEnumerator DelayedSelectGameObject(GameObject objectToSelect)
        {
            yield return new WaitForEndOfFrame();

            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            EventSystem.current.SetSelectedGameObject(objectToSelect);

            LogDebug($"Selected {objectToSelect.name} with delay");
        }

        IEnumerator AnimateTextSize(TextMeshProUGUI text, float startMin, float startMax, float endMin, float endMax, float duration)
        {
            if (text == null) yield break;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float smoothT = t * t * (3f - 2f * t);
                float currentMin = Mathf.Lerp(startMin, endMin, smoothT);
                float currentMax = Mathf.Lerp(startMax, endMax, smoothT);

                text.enableAutoSizing = true;
                text.fontSizeMin = currentMin;
                text.fontSizeMax = currentMax;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            text.fontSizeMin = endMin;
            text.fontSizeMax = endMax;
        }


        IEnumerator ShrinkAndBounceAnimation(RectTransform rectTransform, Vector3 originalScale)
        {
            if (rectTransform == null)
            {
                yield break;
            }

            Vector3 targetScale = originalScale * clickShrinkScale;
            rectTransform.localScale = targetScale;

            yield return new WaitForEndOfFrame();

            if (!gameObject.activeInHierarchy || rectTransform == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < clickBounceDuration && gameObject.activeInHierarchy && rectTransform != null)
            {
                float t = elapsedTime / clickBounceDuration;
                float curveValue = clickBounceCurve.Evaluate(t);

                Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, curveValue);
                rectTransform.localScale = currentScale;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;

                if (!gameObject.activeInHierarchy || rectTransform == null)
                {
                    yield break;
                }
            }

            if (gameObject.activeInHierarchy && rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }
        }


        void SetTextToDefaultSize(TextMeshProUGUI text)
        {
            if (text != null)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = defaultMinFontSize;
                text.fontSizeMax = defaultMaxFontSize;
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF00FF>[PauseMenuManager] {message}</color>");
            }
        }
    }
}