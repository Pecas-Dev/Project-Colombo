using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ProjectColombo.GameInputSystem;
using UnityEngine.SceneManagement;

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
        CharmSwapScreen,
        PickUpScreen,
        Shop
    }

    public enum UISoundType
    {
        Accept,
        Back,
        Tab,
        Select,
        SelectInventory,
        PurchseShop,
        CharmSwapAccept,
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

        [Header("UI Sound Settings")]
        [SerializeField] AudioSource uiAudioSource;
        [SerializeField] AudioClip acceptSound;
        [SerializeField] AudioClip charmSwapAcceptSound;
        [SerializeField] AudioClip[] inventorySelectSound;
        [SerializeField] AudioClip purchaseSound;
        [SerializeField] AudioClip backSound;
        [SerializeField] AudioClip tabSound;
        [SerializeField] AudioClip[] selectSounds;

        [Header("Excluded Buttons (Tabs)")]
        [SerializeField] List<Button> excludedButtons = new List<Button>();


        //[Header("Mask Selection References")]
        //[SerializeField] GameObject maskSelectionCanvas;

        Dictionary<UINavigationState, GameObject> firstSelectables = new Dictionary<UINavigationState, GameObject>();

        Dictionary<UINavigationState, GameObject> lastSelectables = new Dictionary<UINavigationState, GameObject>();

        Dictionary<UINavigationState, HashSet<UISoundType>> allowedSounds;

        UINavigationState currentState = UINavigationState.None;
        UINavigationState previousState = UINavigationState.None;

        Stack<UINavigationState> stateHistory = new Stack<UINavigationState>();

        EventSystem eventSystem;

        float lastNavigationTime;

        public static UINavigationManager Instance { get; private set; }

        #region Unity Lifecycle Methods

        void Awake()
        {
            InitializeUISounds();

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
            InitializeUISounds();
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

            CheckAndEnforcePlayerInputMode();

            HandleUISoundInput();

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

            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (currentSceneName == "02_MaskSelection")
            {
                LogDebug("In MaskSelection scene - preserving navigation state");

                yield return new WaitForSecondsRealtime(0.15f);

                if (currentState == UINavigationState.None)
                {
                    SetNavigationState(UINavigationState.MaskSelection);
                    LogDebug("Set navigation state to MaskSelection as fallback");
                }
            }
            else if (currentSceneName == "00_MainMenu")
            {
                LogDebug("In MainMenu scene - preserving navigation state");

                yield return new WaitForSecondsRealtime(0.15f);

                if (currentState == UINavigationState.None)
                {
                    SetNavigationState(UINavigationState.MainMenu);
                    LogDebug("Set navigation state to MainMenu as fallback");
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.2f);

                if (currentState == UINavigationState.None)
                {
                    LogDebug("No UI state claimed - setting to None");
                }
                else
                {
                    LogDebug($"UI state already claimed: {currentState} - preserving it");
                }
            }
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
                mainMenuCanvas = GameObject.FindGameObjectWithTag("MainMenu");

                if (mainMenuCanvas == null)
                {
                    GameObject gameManagerObject = GameObject.Find("GameManager");
                    if (gameManagerObject != null)
                    {
                        Transform mainMenuCanvasTransform = gameManagerObject.transform.Find("MainMenuCanvas");
                        if (mainMenuCanvasTransform != null)
                        {
                            mainMenuCanvas = mainMenuCanvasTransform.gameObject;
                        }
                    }
                }
            }

            if (mainMenuCanvas != null)
            {
                if (mainMenu == null)
                {
                    MainMenuController mainMenuController = mainMenuCanvas.GetComponentInChildren<MainMenuController>();
                    if (mainMenuController != null)
                    {
                        mainMenu = mainMenuController.gameObject;
                    }
                }

                if (optionsMenu == null)
                {
                    OptionsMenuController optionsMenuController = mainMenuCanvas.GetComponentInChildren<OptionsMenuController>();
                    if (optionsMenuController != null)
                    {
                        optionsMenu = optionsMenuController.gameObject;

                        if (optionsGraphicsTab == null)
                        {
                            Transform tabsTransform = optionsMenu.transform.Find("Tabs");
                            if (tabsTransform != null)
                            {
                                optionsGraphicsTab = FindChildWithName(tabsTransform.gameObject, "GraphicsTab");
                            }
                        }

                        if (optionsAudioTab == null)
                        {
                            Transform tabsTransform = optionsMenu.transform.Find("Tabs");
                            if (tabsTransform != null)
                            {
                                optionsAudioTab = FindChildWithName(tabsTransform.gameObject, "AudioTab");
                            }
                        }

                        if (optionsControlsTab == null)
                        {
                            Transform tabsTransform = optionsMenu.transform.Find("Tabs");
                            if (tabsTransform != null)
                            {
                                optionsControlsTab = FindChildWithName(tabsTransform.gameObject, "ControlsTab");
                            }
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

            LogDebug("References initialized");
        }

        void InitializeUISounds()
        {
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.GetComponent<AudioSource>();
                if (uiAudioSource == null)
                {
                    uiAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            uiAudioSource.playOnAwake = false;
            uiAudioSource.loop = false;

            allowedSounds = new Dictionary<UINavigationState, HashSet<UISoundType>>();

            allowedSounds[UINavigationState.None] = new HashSet<UISoundType>();

            allowedSounds[UINavigationState.MainMenu] = new HashSet<UISoundType>
            {
                UISoundType.Accept,
                UISoundType.Select
            };

            allowedSounds[UINavigationState.OptionsMenu] = new HashSet<UISoundType>
            {
                UISoundType.Back
            };

            allowedSounds[UINavigationState.OptionsMenuGraphicsTab] = new HashSet<UISoundType>
            {
                UISoundType.Back
            };

            allowedSounds[UINavigationState.OptionsMenuAudioTab] = new HashSet<UISoundType>
            {
                UISoundType.Back
            };


            allowedSounds[UINavigationState.OptionsMenuControlsTab] = new HashSet<UISoundType>
            {
                UISoundType.Back
            };

            allowedSounds[UINavigationState.PauseInventoryTab] = new HashSet<UISoundType>
            {
                UISoundType.Back,
                UISoundType.Tab,
                UISoundType.SelectInventory
            };

            allowedSounds[UINavigationState.PauseStatsTab] = new HashSet<UISoundType>
            {
                UISoundType.Back,
                UISoundType.Tab
            };

            allowedSounds[UINavigationState.PauseSettingsTab] = new HashSet<UISoundType>
            {
                UISoundType.Accept,
                UISoundType.Back,
                UISoundType.Tab,
                UISoundType.Select
            };

            allowedSounds[UINavigationState.MaskSelection] = new HashSet<UISoundType>
            {
                UISoundType.Select,
                UISoundType.Accept,
                UISoundType.Back
            };

            allowedSounds[UINavigationState.HUD] = new HashSet<UISoundType>();

            allowedSounds[UINavigationState.CharmSwapScreen] = new HashSet<UISoundType>
            {
                UISoundType.Select,
                UISoundType.CharmSwapAccept,
                UISoundType.Back
            };

            allowedSounds[UINavigationState.PickUpScreen] = new HashSet<UISoundType>
            {
                UISoundType.Accept,
                UISoundType.Back
            };

            allowedSounds[UINavigationState.Shop] = new HashSet<UISoundType>
            {
                UISoundType.Select,
                UISoundType.Back,
                UISoundType.PurchseShop
            };

            LogDebug("UI Sounds initialized");
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

        public void PreserveCurrentSelection()
        {
            if (eventSystem != null && eventSystem.currentSelectedGameObject != null && currentState != UINavigationState.None)
            {
                lastSelectables[currentState] = eventSystem.currentSelectedGameObject;
                LogDebug($"Preserved current selection for state {currentState}: {eventSystem.currentSelectedGameObject.name}");
            }
        }
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
            {
                return;
            }

            LogDebug($"Changing navigation state: {currentState} -> {newState}");

            if (currentState == UINavigationState.MainMenu && eventSystem != null && eventSystem.currentSelectedGameObject != null)
            {
                lastSelectables[currentState] = eventSystem.currentSelectedGameObject;
                LogDebug($"Preserved MainMenu selection: {eventSystem.currentSelectedGameObject.name}");
            }

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
                    gameInput.SwitchToUI();
                }
            }

            if (newState == UINavigationState.MainMenu && eventSystem != null && eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                LogDebug("MainMenu state set, but valid selection already exists - not restoring");
            }
            else
            {
                RestoreSelectionForCurrentState();
            }
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

        void HandleUISoundInput()
        {
            if (gameInput == null || gameInput.inputActions == null)
            {
                return;
            }

            HandleCancelInput();

            if (gameInput.inputActions.UI.Submit.WasPressedThisFrame())
            {
                if (currentState == UINavigationState.CharmSwapScreen)
                {
                    PlayUISound(UISoundType.CharmSwapAccept);
                }
                else
                {
                    PlayUISound(UISoundType.Accept);
                }
            }

            if (gameInput.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                PlayUISound(UISoundType.Back);
            }

            if (gameInput.inputActions.UI.MoveLeftShoulder.WasPressedThisFrame() || gameInput.inputActions.UI.MoveRightShoulder.WasPressedThisFrame())
            {
                PlayUISound(UISoundType.Tab);
            }

            if (gameInput.inputActions.UI.Navigate.WasPressedThisFrame())
            {
                if (currentState == UINavigationState.PauseInventoryTab)
                {
                    PlayUISound(UISoundType.SelectInventory);
                }
                else
                {
                    PlayUISound(UISoundType.Select);
                }
            }
        }

        public void PlayUISound(UISoundType soundType)
        {
            if (uiAudioSource == null)
                return;

            if (!IsSoundAllowedInCurrentState(soundType))
            {
                LogDebug($"Sound {soundType} not allowed in state {currentState}");
                return;
            }

            AudioClip clipToPlay = null;

            switch (soundType)
            {
                case UISoundType.Accept:
                    clipToPlay = acceptSound;
                    break;
                case UISoundType.Back:
                    clipToPlay = backSound;
                    break;
                case UISoundType.Tab:
                    clipToPlay = tabSound;
                    break;
                case UISoundType.Select:
                    if (selectSounds != null && selectSounds.Length > 0)
                    {
                        clipToPlay = selectSounds[Random.Range(0, selectSounds.Length)];
                    }
                    break;
                case UISoundType.SelectInventory:
                    if (selectSounds != null && selectSounds.Length > 0)
                    {
                        clipToPlay = inventorySelectSound[Random.Range(0, selectSounds.Length)];
                    }
                    break;
                case UISoundType.CharmSwapAccept:
                    clipToPlay = charmSwapAcceptSound;
                    break;
                case UISoundType.PurchseShop:
                    clipToPlay = purchaseSound;
                    break;
            }

            if (clipToPlay != null)
            {
                uiAudioSource.PlayOneShot(clipToPlay);
                LogDebug($"Played UI sound: {soundType}");
            }
            else
            {
                LogWarning($"No audio clip assigned for sound type: {soundType}");
            }
        }

        void HandleCancelInput()
        {
            if (gameInput == null || gameInput.inputActions == null)
            {
                return;
            }


            if (gameInput.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                HashSet<UINavigationState> cancelResetsToNoneStates = new HashSet<UINavigationState>
                {
                    UINavigationState.CharmSwapScreen,
                    UINavigationState.PauseInventoryTab,
                    UINavigationState.PauseStatsTab,
                    UINavigationState.PauseSettingsTab,
                    UINavigationState.PickUpScreen
                };

                if (cancelResetsToNoneStates.Contains(currentState))
                {
                    LogDebug($"Cancel pressed in state {currentState} - resetting to None");
                    SetNavigationState(UINavigationState.None);
                }
                else
                {
                    LogDebug($"Cancel pressed in state {currentState} - no state reset needed");
                }
            }
        }


        bool IsSoundAllowedInCurrentState(UISoundType soundType)
        {
            if (allowedSounds == null || !allowedSounds.ContainsKey(currentState))
            {
                return false;
            }

            return allowedSounds[currentState].Contains(soundType);
        }

        void RestoreSelectionForCurrentState()
        {
            if (currentState == UINavigationState.None || currentState == UINavigationState.HUD)
            {
                return;
            }

            GameObject objectToSelect = null;

            if (currentState == UINavigationState.MainMenu)
            {
                if (eventSystem != null && eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.activeInHierarchy)
                {
                    LogDebug($"MainMenu already has valid selection: {eventSystem.currentSelectedGameObject.name} - not changing");
                    return;
                }
            }

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

        void CheckAndEnforcePlayerInputMode()
        {
            if (gameInput == null || gameInput.inputActions == null)
            {
                return;
            }

            if (gameInput.inputActions.Player.enabled && !gameInput.inputActions.UI.enabled)
            {
                HashSet<UINavigationState> protectedUIStates = new HashSet<UINavigationState>
                {
                    UINavigationState.Shop,
                    UINavigationState.CharmSwapScreen,
                    UINavigationState.PickUpScreen,
                    UINavigationState.PauseInventoryTab,
                    UINavigationState.PauseStatsTab,
                    UINavigationState.PauseSettingsTab
                };

                if (currentState != UINavigationState.None && currentState != UINavigationState.HUD && !protectedUIStates.Contains(currentState))
                {
                    StartCoroutine(WaitBeforeChangingToNone());
                }
            }
        }

        IEnumerator WaitBeforeChangingToNone()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            LogDebug($"Detected Player input mode while in {currentState} - forcing state to None");
            SetNavigationState(UINavigationState.None);
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