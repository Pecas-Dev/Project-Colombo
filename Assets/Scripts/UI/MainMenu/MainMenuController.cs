using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectColombo.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MenuController
{
    [Header("Main Menu Specific")]
    [SerializeField] Image[] clefImages;
    [SerializeField] Button[] buttons;
    [SerializeField] TextMeshProUGUI[] buttonTexts;
    public int nextScene;
    [SerializeField] AudioSource[] audioSources;

    [Header("UI Manager Reference")]
    [SerializeField] private UIManagerV2 uiManagerV2;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;

    int currentSelectedIndex = -1;

    static int lastSelectedButtonIndex = 0;


    Coroutine[] sizeAnimationCoroutines;

    private bool hasBeenInitialized = false;

    private void OnEnable()
    {

        // This ensures animations run whenever the menu is activated
        if (hasBeenInitialized)
        {
            // Select the last active button instead of always the first one
            SelectButton(lastSelectedButtonIndex);
            RefreshAnimations();
        }
        else
        {
            Initialize();
            hasBeenInitialized = true;
        }

        // Force controller mode & button selection
        UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

        if (inputSwitcher != null)
        {
            // Force button selection in next frame
            StartCoroutine(ForceButtonSelection(inputSwitcher));
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        // Find UIManagerV2 if not assigned
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

        if (buttons.Length > 0 && buttons[0] != null)
        {
            SelectButton(0);

            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (uiInputSwitcher != null)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
            }
        }

        if (enableMenuNavigation)
        {
            StartCoroutine(DelayedNavigationSetup());
        }
    }

    // Override SetupButtonNavigation to work with our specific buttons array
    protected override void SetupButtonNavigation()
    {
        // Skip base implementation
        if (buttons == null || buttons.Length == 0) return;

        // Set up explicit navigation between main menu buttons
        LogDebug("Setting up navigation for " + buttons.Length + " buttons");

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // Configure vertical navigation (up and down)
            if (i > 0)
            {
                nav.selectOnUp = buttons[i - 1];
                LogDebug($"Button {button.name} selectOnUp = {buttons[i - 1].name}");
            }
            else if (wrapNavigation)
            {
                // Wrap around to the last button
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
                // Wrap around to the first button
                nav.selectOnDown = buttons[0];
                LogDebug($"Button {button.name} selectOnDown = {buttons[0].name} (wrap)");
            }

            button.navigation = nav;
        }

        // Set initial button selection if we have buttons
        if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            LogDebug("Set initial selection to " + buttons[0].name);
        }
    }

    public override void Show()
    {
        base.Show();

        // Make sure we're visible
        if (menuContainer != null)
        {
            menuContainer.SetActive(true);
        }

        RefreshAnimations();
    }

    private void RefreshAnimations()
    {
        // Ensure proper button selection
        if (currentSelectedIndex >= 0 && currentSelectedIndex < buttons.Length)
        {
            // Force reselection to refresh animations
            int previousIndex = currentSelectedIndex;
            currentSelectedIndex = -1;
            SelectButton(previousIndex);

            // Make sure EventSystem knows which button is selected
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
            // Default to first button if no selection
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
        lastSelectedButtonIndex = index; // Store this selection for next time

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

        // Visual feedback through button animation
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
        // Use UIManagerV2 instead of the old UIManager
        if (uiManagerV2 != null)
        {
            uiManagerV2.ShowOptionsMenu();
        }
        else
        {
            Debug.LogWarning("UIManagerV2 reference missing!");

            // Try to find and activate the options menu directly as fallback
            Transform optionsMenu = transform.parent?.Find("OptionsMenu");
            if (optionsMenu != null)
            {
                menuContainer.SetActive(false);
                optionsMenu.gameObject.SetActive(true);

                // Initialize the options menu if possible
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
            gameInputSO.playerInputActions.UI.Disable();
        }

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

        yield return new WaitForSecondsRealtime(0.5f);

        Application.Quit();

        Debug.Log("Game is closed!");
    }

    IEnumerator ForceButtonSelection(UIInputSwitcher inputSwitcher)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.05f);

        // Refresh the EventSystem reference
        inputSwitcher.RefreshEventSystemReference();

        // Force select the last selected button instead of always the first one
        if (buttons.Length > lastSelectedButtonIndex && buttons[lastSelectedButtonIndex] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[lastSelectedButtonIndex].gameObject);
            inputSwitcher.SetFirstSelectedButton(buttons[lastSelectedButtonIndex].gameObject);
            inputSwitcher.ForceSelectButton(buttons[lastSelectedButtonIndex].gameObject);
        }
        else if (buttons.Length > 0 && buttons[0] != null) // Fallback to first button if needed
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            inputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
            inputSwitcher.ForceSelectButton(buttons[0].gameObject);
        }
    }

    IEnumerator DelayedNavigationSetup()
    {
        // Wait for next frame to ensure all components are ready
        yield return new WaitForEndOfFrame();
        SetupButtonNavigation();

        // Force select the first button
        if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"<color=#00AAFF>[MainMenuController] {message}</color>");
        }
    }
}