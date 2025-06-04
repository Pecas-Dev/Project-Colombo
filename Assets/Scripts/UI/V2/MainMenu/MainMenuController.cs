using ProjectColombo.GameManagement.Events;
using ProjectColombo.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MenuController
{
    [Header("Main Menu Specific")]
    [SerializeField] Image[] clefImages;
    [SerializeField] Button[] buttons;
    [SerializeField] TextMeshProUGUI[] buttonTexts;
    public int nextScene;
    [SerializeField] AudioSource[] audioSources;

    [Header("UI Manager Reference")]
    [SerializeField] UIManagerV2 uiManagerV2;

    [Header("Debug Settings")]
    [SerializeField] bool enableDebugLogs = true;

    int currentSelectedIndex = -1;

    static int lastSelectedButtonIndex = 0;

    public bool hasBeenInitialized = false;


    Coroutine[] sizeAnimationCoroutines;



    MainMenuNavigationExtension navigationExtension;



    void OnEnable()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LogDebug($"OnEnable called in scene: {currentScene}");

        if (currentScene != "00_MainMenu")
        {
            LogDebug("Not in main menu scene - deactivating");
            gameObject.SetActive(false);
            return;
        }

        LogDebug("In main menu scene - ensuring activation");

        hasBeenInitialized = false;

        StartCoroutine(DelayedShow());
    }

    public override void Initialize()
    {
        base.Initialize();

        if (uiManagerV2 == null)
        {
            uiManagerV2 = FindFirstObjectByType<UIManagerV2>();
        }

        if (buttons.Length != clefImages.Length)
        {
            Debug.LogWarning("Number of buttons does not match number of images.");
        }

        if (buttons.Length != buttonTexts.Length)
        {
            Debug.LogWarning("Number of buttons does not match number of text components.");
        }

        sizeAnimationCoroutines = new Coroutine[buttonTexts.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                int index = i;

                buttons[i].onClick.AddListener(() => SelectButton(index));

                EventTrigger eventTrigger = buttons[i].gameObject.GetComponent<EventTrigger>();

                if (eventTrigger == null)
                {
                    eventTrigger = buttons[i].gameObject.AddComponent<EventTrigger>();
                }

                AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, (data) => { SelectButton(index); });
                AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => { SelectButton(index); });
            }
        }

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] != null)
            {
                SetTextToDefaultSize(buttonTexts[i]);
            }
        }

        DeactivateAllImages();

        int initialSelection = 0;
        if (lastSelectedButtonIndex >= 0 && lastSelectedButtonIndex < buttons.Length)
        {
            initialSelection = lastSelectedButtonIndex;
            LogDebug($"Using preserved selection: button {initialSelection}");
        }

        if (buttons.Length > 0 && buttons[initialSelection] != null)
        {
            currentSelectedIndex = -1;
            SelectButton(initialSelection);

            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (uiInputSwitcher != null)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[initialSelection].gameObject);
            }

            LogDebug($"Initialized with button {initialSelection} selected and clef visible");
        }

        if (enableMenuNavigation)
        {
            StartCoroutine(DelayedNavigationSetup());
        }

        navigationExtension = GetComponent<MainMenuNavigationExtension>();

        if (navigationExtension == null)
        {
            navigationExtension = gameObject.AddComponent<MainMenuNavigationExtension>();

            if (buttons != null && buttons.Length > 0)
            {
                navigationExtension.GetType().GetField("firstSelectedObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(navigationExtension, buttons[initialSelection].gameObject);
            }
        }
    }

    protected override void SetupButtonNavigation()
    {
        if (buttons == null || buttons.Length == 0)
        {
            return;
        }

        LogDebug("Setting up navigation for " + buttons.Length + " buttons");

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0)
            {
                nav.selectOnUp = buttons[i - 1];
                LogDebug($"Button {button.name} selectOnUp = {buttons[i - 1].name}");
            }
            else if (wrapNavigation)
            {
                nav.selectOnUp = buttons[buttons.Length - 1];
                LogDebug($"Button {button.name} selectOnUp = {buttons[buttons.Length - 1].name} (wrap)");
            }

            if (i < buttons.Length - 1)
            {
                nav.selectOnDown = buttons[i + 1];
                LogDebug($"Button {button.name} selectOnDown = {buttons[i + 1].name}");
            }
            else if (wrapNavigation)
            {
                nav.selectOnDown = buttons[0];
                LogDebug($"Button {button.name} selectOnDown = {buttons[0].name} (wrap)");
            }

            button.navigation = nav;
        }

        if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            LogDebug("Set initial selection to " + buttons[0].name);
        }
    }

    public override void Show()
    {
        LogDebug("Show() called - FORCING main menu display");

        bool wasAlreadyInitialized = hasBeenInitialized;
        int previousSelection = currentSelectedIndex;

        hasBeenInitialized = false;

        base.Show();

        if (menuContainer != null)
        {
            menuContainer.SetActive(true);
            LogDebug("Menu container ACTIVATED");
        }

        LogDebug("FORCING initialization");

        Initialize();
        hasBeenInitialized = true;

        if (gameInputSO != null)
        {
            gameInputSO.SwitchToUI();
            LogDebug("Switched to UI input");
        }

        if (buttons != null && buttons.Length > 0)
        {
            int buttonToSelect = 0;

            if (wasAlreadyInitialized && previousSelection >= 0 && previousSelection < buttons.Length)
            {
                buttonToSelect = previousSelection;
                LogDebug($"Preserving previous selection: button {buttonToSelect}");
            }
            else if (lastSelectedButtonIndex >= 0 && lastSelectedButtonIndex < buttons.Length)
            {
                buttonToSelect = lastSelectedButtonIndex;
                LogDebug($"Using last selected button: {buttonToSelect}");
            }

            lastSelectedButtonIndex = buttonToSelect;

            currentSelectedIndex = -1;
            SelectButton(buttonToSelect);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(buttons[buttonToSelect].gameObject);
                LogDebug($"Set EventSystem selection to button {buttonToSelect}");
            }

            UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (inputSwitcher != null)
            {
                inputSwitcher.SetFirstSelectedButton(buttons[buttonToSelect].gameObject);
                inputSwitcher.ForceSelectButton(buttons[buttonToSelect].gameObject);
                LogDebug("Setup input switcher");
            }
        }
        StartCoroutine(EnsureMainMenuNavigationStateWithoutSelectionReset());

        LogDebug("Main menu Show() COMPLETED - should be fully active now");
    }

    IEnumerator EnsureMainMenuNavigationStateWithoutSelectionReset()
    {
        yield return new WaitForEndOfFrame();

        UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();

        if (navigationManager != null && buttons != null && buttons.Length > 0)
        {
            int currentSelection = currentSelectedIndex >= 0 ? currentSelectedIndex : 0;
            navigationManager.RegisterFirstSelectable(UINavigationState.MainMenu, buttons[currentSelection].gameObject);
            navigationManager.SetNavigationState(UINavigationState.MainMenu);
            LogDebug($"Ensured MainMenu navigation state with current selection: button {currentSelection}");
        }

        yield return new WaitForSecondsRealtime(0.1f);

        if (navigationManager != null && navigationManager.GetCurrentState() != UINavigationState.MainMenu)
        {
            navigationManager.SetNavigationState(UINavigationState.MainMenu);
            LogDebug("Force-set MainMenu state after delay check (without selection reset)");
        }
    }

    IEnumerator EnsureMainMenuNavigationState()
    {
        yield return new WaitForEndOfFrame();

        UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();

        if (navigationManager != null && buttons != null && buttons.Length > 0)
        {
            navigationManager.RegisterFirstSelectable(UINavigationState.MainMenu, buttons[0].gameObject);
            navigationManager.SetNavigationState(UINavigationState.MainMenu);
            LogDebug("Ensured MainMenu navigation state is set");
        }

        yield return new WaitForSecondsRealtime(0.1f);

        if (navigationManager != null && navigationManager.GetCurrentState() != UINavigationState.MainMenu)
        {
            navigationManager.SetNavigationState(UINavigationState.MainMenu);
            LogDebug("Force-set MainMenu state after delay check");
        }
    }

    IEnumerator DelayedShow()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.15f);

        LogDebug("Delayed show triggering");
        Show();

        yield return new WaitForSecondsRealtime(0.1f);

        UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();

        if (navigationManager != null && navigationManager.GetCurrentState() != UINavigationState.MainMenu)
        {
            LogDebug("MainMenu state was lost after Show() - re-setting");
            if (buttons != null && buttons.Length > 0)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.MainMenu, buttons[0].gameObject);
            }
            navigationManager.SetNavigationState(UINavigationState.MainMenu);
        }
    }


    void RefreshAnimations()
    {
        if (currentSelectedIndex >= 0 && currentSelectedIndex < buttons.Length)
        {
            int previousIndex = currentSelectedIndex;
            currentSelectedIndex = -1;
            SelectButton(previousIndex);

            EventSystem.current.SetSelectedGameObject(buttons[currentSelectedIndex].gameObject);

            if (uiInputSwitcher == null)
            {
                uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
            }

            if (uiInputSwitcher != null)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[currentSelectedIndex].gameObject);
            }

            LogDebug("Refreshed animations for button index " + currentSelectedIndex);
        }
        else if (buttons.Length > 0)
        {
            SelectButton(0);
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);

            if (uiInputSwitcher != null)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
            }
        }
    }

    void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => { action((BaseEventData)data); });
        trigger.triggers.Add(entry);
    }

    void SelectButton(int index)
    {
        if (currentSelectedIndex == index)
        {
            return;
        }

        if (currentSelectedIndex >= 0)
        {
            if (currentSelectedIndex < clefImages.Length && clefImages[currentSelectedIndex] != null)
            {
                clefImages[currentSelectedIndex].gameObject.SetActive(false);
            }

            if (currentSelectedIndex < buttonTexts.Length && buttonTexts[currentSelectedIndex] != null)
            {
                if (sizeAnimationCoroutines[currentSelectedIndex] != null)
                {
                    StopCoroutine(sizeAnimationCoroutines[currentSelectedIndex]);
                }

                sizeAnimationCoroutines[currentSelectedIndex] = StartCoroutine(AnimateTextSize(buttonTexts[currentSelectedIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));
            }
        }

        currentSelectedIndex = index;
        lastSelectedButtonIndex = index;

        if (index < clefImages.Length && clefImages[index] != null)
        {
            clefImages[index].gameObject.SetActive(true);
        }

        if (index < buttonTexts.Length && buttonTexts[index] != null)
        {
            if (sizeAnimationCoroutines[index] != null)
            {
                StopCoroutine(sizeAnimationCoroutines[index]);
            }

            sizeAnimationCoroutines[index] = StartCoroutine(AnimateTextSize(buttonTexts[index], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));
        }

        if (index < buttons.Length && buttons[index] != null)
        {
            RectTransform rectTransform = buttons[index].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                PlayButtonClickAnimation(rectTransform);
            }
        }

        LogDebug("Selected button index " + index);
    }

    void DeactivateAllImages()
    {
        foreach (Image image in clefImages)
        {
            if (image != null)
            {
                image.gameObject.SetActive(false);
            }
        }
    }

    public void SelectButtonByIndex(int index)
    {
        if (index >= 0 && index < buttons.Length)
        {
            LogDebug("SelectButtonByIndex called with index " + index);
            SelectButton(index);

            if (buttons[index] != null)
            {
                EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
            }
        }
    }

    public void OpenOptionsMenu()
    {
        if (uiManagerV2 != null)
        {
            uiManagerV2.ShowOptionsMenu();
        }
        else
        {
            Debug.LogWarning("UIManagerV2 reference missing!");

            Transform optionsMenu = transform.parent?.Find("OptionsMenu");
            if (optionsMenu != null)
            {
                menuContainer.SetActive(false);
                optionsMenu.gameObject.SetActive(true);

                OptionsMenuController optionsController = optionsMenu.GetComponent<OptionsMenuController>();
                if (optionsController != null)
                {
                    optionsController.Show();
                }
            }
        }
    }

    public void StartGame()
    {
        if (gameInputSO != null)
        {
            gameInputSO.SwitchToGameplay();
        }

        CustomEvents.GameReset();
        StartCoroutine(ToFirstLevel());
    }

    public void ExitGame()
    {
        StartCoroutine(CloseGame());
    }

    IEnumerator ToFirstLevel()
    {
        if (transitionAnimation != null)
        {
            transitionAnimation.Play("Close");
        }

        yield return new WaitForSecondsRealtime(2.5f);

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator CloseGame()
    {
        if (transitionAnimation != null)
        {
            transitionAnimation.Play("Close");
        }

        if (audioSources.Length > 0 && audioSources[0] != null)
        {
            float startVolume = audioSources[0].volume;
            float duration = 2.75f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float volumePercent = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
                audioSources[0].volume = volumePercent;

                yield return null;
            }

            audioSources[0].volume = 0f;
        }

        yield return new WaitForSecondsRealtime(4.0f);

        Application.Quit();

        Debug.Log("Game is closed!");
    }

    IEnumerator ForceButtonSelection(UIInputSwitcher inputSwitcher)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.05f);

        inputSwitcher.RefreshEventSystemReference();

        if (buttons.Length > lastSelectedButtonIndex && buttons[lastSelectedButtonIndex] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[lastSelectedButtonIndex].gameObject);
            inputSwitcher.SetFirstSelectedButton(buttons[lastSelectedButtonIndex].gameObject);
            inputSwitcher.ForceSelectButton(buttons[lastSelectedButtonIndex].gameObject);
        }
        else if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            inputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
            inputSwitcher.ForceSelectButton(buttons[0].gameObject);
        }
    }

    IEnumerator DelayedNavigationSetup()
    {
        yield return new WaitForEndOfFrame();

        SetupButtonNavigation();

        if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"<color=#00AAFF>[MainMenuController] {message}</color>");
        }
    }
}