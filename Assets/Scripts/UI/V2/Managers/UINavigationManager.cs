using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ProjectColombo.GameInputSystem;

namespace ProjectColombo.UI
{
    public enum UINavigationState
    {
        None,
        MainMenu,
        OptionsMenu,
        OptionsMenuGraphicsTab,
        OptionsMenuAudioTab,
        OptionsMenuControlsTab,
        PauseInventoryTab,
        PauseStatsTab,
        PauseSettingsTab,
        MaskSelection,
        HUD,
        CharmSwapScreen
    }

    public class UINavigationManager : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;
        [SerializeField] float navigationCooldown = 0.2f;

        [Header("Main Menu References")]
        [SerializeField] GameObject mainMenuCanvas;
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject optionsMenu;
        [SerializeField] GameObject optionsGraphicsTab;
        [SerializeField] GameObject optionsAudioTab;
        [SerializeField] GameObject optionsControlsTab;

        [Header("Pause Menu References")]
        [SerializeField] GameObject pauseInventoryCanvas;
        [SerializeField] GameObject inventoryTab;
        [SerializeField] GameObject statsTab;
        [SerializeField] GameObject settingsTab;

        [Header("Charm Swap References")]
        [SerializeField] GameObject charmSwapCanvas;
        [SerializeField] GameObject charmSwapScreen;

        [Header("Excluded Buttons (Tabs)")]
        [SerializeField] List<Button> excludedButtons = new List<Button>();


        //[Header("Mask Selection References")]
        //[SerializeField] GameObject maskSelectionCanvas;

        Dictionary<UINavigationState, GameObject> firstSelectables = new Dictionary<UINavigationState, GameObject>();

        Dictionary<UINavigationState, GameObject> lastSelectables = new Dictionary<UINavigationState, GameObject>();

        UINavigationState currentState = UINavigationState.None;
        UINavigationState previousState = UINavigationState.None;

        Stack<UINavigationState> stateHistory = new Stack<UINavigationState>();

        EventSystem eventSystem;

        float lastNavigationTime;

        public static UINavigationManager Instance { get; private set; }

        #region Unity Lifecycle Methods

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            eventSystem = EventSystem.current;

            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>();
                if (eventSystem == null)
                {
                    LogError("No EventSystem found in the scene!");
                }
            }

            if (gameInput == null)
            {
                gameInput = FindFirstObjectByType<GameInputSO>();

                if (gameInput == null)
                {
                    LogError("GameInputSO reference is missing!");
                }
            }
        }

        void Start()
        {
            InitializeReferences();
            MapFirstSelectables();
        }

        void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Update()
        {
            if (eventSystem == null)
            {
                eventSystem = EventSystem.current;

                if (eventSystem == null)
                {
                    return;
                }
            }

            if (eventSystem.currentSelectedGameObject != null && currentState != UINavigationState.None && eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                Button selectedButton = eventSystem.currentSelectedGameObject.GetComponent<Button>();

                if (selectedButton != null && excludedButtons.Contains(selectedButton))
                {
                    LogDebug($"Excluded button {selectedButton.name} was selected - restoring proper selection");
                    eventSystem.SetSelectedGameObject(null);
                    RestoreSelectionForCurrentState();
                }
                else
                {
                    lastSelectables[currentState] = eventSystem.currentSelectedGameObject;
                }
            }

            if (currentState != UINavigationState.None && currentState != UINavigationState.HUD && eventSystem.currentSelectedGameObject == null && Time.unscaledTime - lastNavigationTime > navigationCooldown)
            {
                RestoreSelectionForCurrentState();
            }
        }

        #endregion

        #region Initialization Methods

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            StartCoroutine(DelayedInitialization());
        }

        IEnumerator DelayedInitialization()
        {
            yield return null;
            InitializeReferences();
            MapFirstSelectables();

            SetNavigationState(UINavigationState.None);
        }

        public void InitializeReferences()
        {
            if (eventSystem == null)
            {
                eventSystem = EventSystem.current;
                if (eventSystem == null)
                {
                    eventSystem = FindFirstObjectByType<EventSystem>();
                }
            }

            if (mainMenuCanvas == null)
            {
                mainMenuCanvas = GameObject.Find("MainMenuCanvas");
            }

            if (mainMenuCanvas != null)
            {
                if (mainMenu == null)
                {
                    mainMenu = FindChildWithName(mainMenuCanvas, "MainMenu");
                }

                if (optionsMenu == null)
                {
                    optionsMenu = FindChildWithName(mainMenuCanvas, "OptionsMenu");
                }

                if (optionsMenu != null)
                {
                    Transform tabsTransform = optionsMenu.transform.Find("Tabs");

                    if (tabsTransform != null)
                    {
                        if (optionsGraphicsTab == null)
                        {
                            optionsGraphicsTab = FindChildWithName(tabsTransform.gameObject, "GraphicsTab");
                        }

                        if (optionsAudioTab == null)
                        {
                            optionsAudioTab = FindChildWithName(tabsTransform.gameObject, "AudioTab");
                        }

                        if (optionsControlsTab == null)
                        {
                            optionsControlsTab = FindChildWithName(tabsTransform.gameObject, "ControlsTab");
                        }
                    }
                }
            }

            if (pauseInventoryCanvas == null)
            {
                pauseInventoryCanvas = GameObject.Find("PauseInventoryCanvas");
            }

            if (pauseInventoryCanvas != null)
            {
                if (inventoryTab == null)
                {
                    inventoryTab = FindChildWithName(pauseInventoryCanvas, "InventoryTab");

                    if (inventoryTab == null)
                    {
                        inventoryTab = FindChildWithName(pauseInventoryCanvas, "PauseInventory");
                    }
                }

                if (statsTab == null)
                {
                    statsTab = FindChildWithName(pauseInventoryCanvas, "StatsTab");

                    if (statsTab == null)
                    {
                        statsTab = FindChildWithName(pauseInventoryCanvas, "PauseStats");
                    }
                }

                if (settingsTab == null)
                {
                    settingsTab = FindChildWithName(pauseInventoryCanvas, "SettingsTab");

                    if (settingsTab == null)
                    {
                        settingsTab = FindChildWithName(pauseInventoryCanvas, "PauseSettings");
                    }
                }
            }

            if (charmSwapCanvas == null)
            {
                charmSwapCanvas = GameObject.Find("CharmSwapCanvas");
            }

            if (charmSwapCanvas != null)
            {
                if (charmSwapScreen == null)
                {
                    charmSwapScreen = FindChildWithName(charmSwapCanvas, "CharmSwapController");
                }
            }

            //if (maskSelectionCanvas == null)
            //{
            //    maskSelectionCanvas = GameObject.Find("Canvas");

            //    if (maskSelectionCanvas != null && maskSelectionCanvas.GetComponent<MaskCanvas>() == null)
            //    {
            //        maskSelectionCanvas = null;
            //    }
            //}

            LogDebug("References initialized");
        }

        void MapFirstSelectables()
        {
            firstSelectables.Clear();

            if (mainMenu != null)
            {
                Button firstButton = GetFirstSelectableButton(mainMenu);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.MainMenu] = firstButton.gameObject;
                    lastSelectables[UINavigationState.MainMenu] = firstButton.gameObject;
                }
            }

            if (optionsGraphicsTab != null)
            {
                Button firstButton = GetFirstSelectableButton(optionsGraphicsTab);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.OptionsMenuGraphicsTab] = firstButton.gameObject;
                    lastSelectables[UINavigationState.OptionsMenuGraphicsTab] = firstButton.gameObject;
                }
            }

            if (optionsAudioTab != null)
            {
                Button firstButton = GetFirstSelectableButton(optionsAudioTab);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.OptionsMenuAudioTab] = firstButton.gameObject;
                    lastSelectables[UINavigationState.OptionsMenuAudioTab] = firstButton.gameObject;
                }
            }

            if (optionsControlsTab != null)
            {
                Button firstButton = GetFirstSelectableButton(optionsControlsTab);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.OptionsMenuControlsTab] = firstButton.gameObject;
                    lastSelectables[UINavigationState.OptionsMenuControlsTab] = firstButton.gameObject;
                }
            }

            if (inventoryTab != null)
            {
                Button firstButton = GetFirstSelectableButton(inventoryTab);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.PauseInventoryTab] = firstButton.gameObject;
                    lastSelectables[UINavigationState.PauseInventoryTab] = firstButton.gameObject;
                }
            }

            firstSelectables[UINavigationState.PauseStatsTab] = null;
            lastSelectables[UINavigationState.PauseStatsTab] = null;

            if (settingsTab != null)
            {
                Button firstButton = GetFirstSelectableButton(settingsTab);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.PauseSettingsTab] = firstButton.gameObject;
                    lastSelectables[UINavigationState.PauseSettingsTab] = firstButton.gameObject;
                }
            }

            if (charmSwapScreen != null)
            {
                Button firstButton = GetFirstSelectableButton(charmSwapScreen);
                if (firstButton != null)
                {
                    firstSelectables[UINavigationState.CharmSwapScreen] = firstButton.gameObject;
                    lastSelectables[UINavigationState.CharmSwapScreen] = firstButton.gameObject;
                }
            }

            //if (maskSelectionCanvas != null)
            //{
            //    Transform maskTransform = maskSelectionCanvas.transform.Find("Mask");

            //    if (maskTransform != null)
            //    {
            //        Button firstButton = GetFirstSelectableButton(maskTransform.gameObject);
            //        if (firstButton != null)
            //        {
            //            firstSelectables[UINavigationState.MaskSelection] = firstButton.gameObject;
            //            lastSelectables[UINavigationState.MaskSelection] = firstButton.gameObject;
            //        }
            //    }
            //}

            LogDebug($"Mapped {firstSelectables.Count} first selectable objects");
        }

        #endregion

        #region Public Methods

        public void ExcludeButtonFromNavigation(Button button)
        {
            if (button != null && !excludedButtons.Contains(button))
            {
                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.None;
                button.navigation = nav;

                excludedButtons.Add(button);
                LogDebug($"Button {button.name} excluded from navigation");
            }
        }

        public void ExcludeButtonsFromNavigation(Button[] buttons)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (var button in buttons)
            {
                ExcludeButtonFromNavigation(button);
            }
        }

        public void SetNavigationState(UINavigationState newState)
        {
            if (newState == currentState)
                return;

            LogDebug($"Changing navigation state: {currentState} -> {newState}");

            previousState = currentState;

            if (currentState != UINavigationState.None)
            {
                stateHistory.Push(currentState);
            }

            currentState = newState;

            if (newState != UINavigationState.None && newState != UINavigationState.HUD)
            {
                if (gameInput != null)
                {
                    gameInput.EnableUIMode();
                }
            }

            RestoreSelectionForCurrentState();
        }

        public void RegisterFirstSelectable(UINavigationState state, GameObject selectable)
        {
            if (selectable == null)
            {
                LogWarning($"Attempted to register null selectable for state {state}");
                return;
            }

            firstSelectables[state] = selectable;

            if (!lastSelectables.ContainsKey(state) || lastSelectables[state] == null)
            {
                lastSelectables[state] = selectable;
            }

            LogDebug($"Registered first selectable for state {state}: {selectable.name}");
        }


        public void UpdateLastSelectable(GameObject selectable)
        {
            if (selectable == null || currentState == UINavigationState.None)
            {
                return;
            }

            lastSelectables[currentState] = selectable;
        }


        public void ForceSelection(GameObject selectable)
        {
            if (selectable == null || eventSystem == null)
                return;

            StartCoroutine(DelayedSelect(selectable));
        }


        public void GoBack()
        {
            if (stateHistory.Count > 0)
            {
                UINavigationState previousState = stateHistory.Pop();
                SetNavigationState(previousState);
            }
            else
            {
                SetNavigationState(UINavigationState.None);
            }
        }

        public UINavigationState GetCurrentState()
        {
            return currentState;
        }


        public UINavigationState GetPreviousState()
        {
            return previousState;
        }

        #endregion

        #region Helper Methods

        void RestoreSelectionForCurrentState()
        {
            if (currentState == UINavigationState.None || currentState == UINavigationState.HUD)
            {
                return;
            }

            GameObject objectToSelect = null;

            if (lastSelectables.ContainsKey(currentState) && lastSelectables[currentState] != null && lastSelectables[currentState].activeInHierarchy)
            {
                objectToSelect = lastSelectables[currentState];
            }
            else if (firstSelectables.ContainsKey(currentState) && firstSelectables[currentState] != null && firstSelectables[currentState].activeInHierarchy)
            {
                objectToSelect = firstSelectables[currentState];
            }

            if (objectToSelect != null && eventSystem != null)
            {
                StartCoroutine(DelayedSelect(objectToSelect));
            }
            else
            {
                LogWarning($"No selectable object available for state {currentState}");
            }
        }

        IEnumerator DelayedSelect(GameObject selectable)
        {
            yield return new WaitForEndOfFrame();

            eventSystem.SetSelectedGameObject(null);

            yield return null;

            eventSystem.SetSelectedGameObject(selectable);

            lastNavigationTime = Time.unscaledTime;

            LogDebug($"Selected {selectable.name}");

            UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (inputSwitcher != null)
            {
                inputSwitcher.ForceSelectButton(selectable);
            }
        }

        Button GetFirstSelectableButton(GameObject parent)
        {
            if (parent == null)
            {
                return null;
            }

            Button[] buttons = parent.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                if (button.gameObject.name.Contains("First"))
                {
                    return button;
                }
            }

            foreach (Button button in buttons)
            {
                if (button.gameObject.activeInHierarchy && button.interactable)
                {
                    return button;
                }
            }

            return buttons.Length > 0 ? buttons[0] : null;
        }


        GameObject FindChildWithName(GameObject parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                return child.gameObject;

            }

            foreach (Transform t in parent.transform)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }

            return null;
        }


        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF00>[UINavigationManager] {message}</color>");
            }
        }


        void LogWarning(string message)
        {
            Debug.LogWarning($"[UINavigationManager] {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"[UINavigationManager] {message}");
        }

        #endregion
    }
}