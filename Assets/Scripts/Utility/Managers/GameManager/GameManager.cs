using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ProjectColombo.UI.Pausescreen;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.UI;


namespace ProjectColombo.GameManagement
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;
        [SerializeField] bool enableInputActionMapLogs = false;


        [Header("Pause Menu System")]
        [Tooltip("Toggle between old and new pause menu systems")]
        [SerializeField] bool useNewPauseMenu = false;

        [Tooltip("Old pause menu reference")]
        public GameObject pauseMenuUI;
        [Tooltip("New pause menu reference (should be the PauseInventoryCanvas)")]
        [SerializeField] GameObject newPauseMenuCanvas;


        [Header("Charm Swap System")]
        [Tooltip("Toggle between old and new charm swap UI systems")]
        [SerializeField] bool useNewCharmSwapUI = false;

        [SerializeField] GameObject oldCharmSwapScreen;
        [SerializeField] GameObject newCharmSwapCanvas;


        [Header("Transition")]
        [SerializeField] Animator transition;


        public bool gameIsPaused = false;

        bool isResettingInputs = false;

        float inputResetDelay = 0.1f;


        // SCENE NAMES
        static string MAIN_MENU = "00_MainMenu";


        protected GameObject directPauseMenuReference;
        protected PauseMenuInventoryController pauseMenuController;

        protected CharmSwapMenuController charmSwapMenuController;
        protected GameObject directCharmSwapMenuReference;

        // New UI Manager v2 reference
        private UIManagerV2 uiManagerV2;

        public CharmSwapMenuController CharmSwapMenuCtrl => charmSwapMenuController;
        public bool UseNewCharmSwapUI => useNewCharmSwapUI;

        void Awake()
        {
            gameInput.Initialize();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // Initialize UIManagerV2
            uiManagerV2 = GetComponent<UIManagerV2>();
            if (uiManagerV2 == null)
            {
                uiManagerV2 = gameObject.AddComponent<UIManagerV2>();
                LogDebug("Added UIManagerV2 component");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            if (useNewPauseMenu)
            {
                if (pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                }
            }

            EnsureCorrectCharmSwapSystemActive();

            // Initialize UI references
            if (uiManagerV2 != null)
            {
                uiManagerV2.InitializeReferences();
            }
        }

        void EnsureCorrectCharmSwapSystemActive()
        {
            if (oldCharmSwapScreen == null)
            {
                oldCharmSwapScreen = GameObject.Find("CharmSelectScreen");

                if (oldCharmSwapScreen == null)
                {
                    oldCharmSwapScreen = FindFirstObjectByType<CharmSelectScreen>(FindObjectsInactive.Include)?.gameObject;
                }
            }

            if (useNewCharmSwapUI)
            {
                if (directCharmSwapMenuReference != null)
                {
                    directCharmSwapMenuReference.SetActive(true);

                    if (charmSwapMenuController != null)
                    {
                        charmSwapMenuController.gameObject.SetActive(false);
                    }
                }

                if (oldCharmSwapScreen != null)
                {
                    oldCharmSwapScreen.SetActive(false);
                }
            }
            else
            {
                if (oldCharmSwapScreen != null)
                {
                    oldCharmSwapScreen.SetActive(false);
                }

                if (directCharmSwapMenuReference != null)
                {
                    directCharmSwapMenuReference.SetActive(false);
                }
            }
        }

        void Update()
        {
            // Check for pause input via UIManagerV2 (raw input)
            if (uiManagerV2 != null && uiManagerV2.CheckPauseInput())
            {
                if (gameIsPaused)
                {
                    ResumeGame();
                }
                else if (uiManagerV2.CanPauseInCurrentScene())
                {
                    PauseGame();
                }
            }

            // Check for pause through the input system as a backup
            if (gameInput.PausePressed)
            {
                if (gameIsPaused)
                {
                    ResumeGame();
                }
                else if (uiManagerV2 != null && uiManagerV2.CanPauseInCurrentScene())
                {
                    PauseGame();
                }
            }

            // Check for UI Cancel action to close pause menu
            if (gameIsPaused && gameInput.playerInputActions.UI.Cancel.WasPressedThisFrame())
            {
                ResumeGame();
            }

            if (gameInput.playerInputActions.Player.enabled == true && enableInputActionMapLogs == true)
            {
                Debug.Log("PLAYERRRRRRRRRR!");
            }
            if (gameInput.playerInputActions.UI.enabled == true && enableInputActionMapLogs == true)
            {
                Debug.Log("UIIIIIIIIIIIIIIIII!");
            }
        }

        void OnDestroy()
        {
            Time.timeScale = 1;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResumeGame();

            transition.Play("Open");

            gameInput.Uninitialize();
            gameInput.Initialize();
            gameInput.EnableAllInputs();

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                GetComponent<GlobalStats>().ResetStats();
            }

            if (useNewPauseMenu)
            {
                directPauseMenuReference = null;
                pauseMenuController = null;
            }

            if (useNewPauseMenu)
            {
                directPauseMenuReference = null;
                pauseMenuController = null;
            }

            if (useNewCharmSwapUI)
            {
                directCharmSwapMenuReference = null;
                charmSwapMenuController = null;
            }

            // Re-initialize UI references after scene change
            if (uiManagerV2 != null)
            {
                uiManagerV2.InitializeReferences();
            }
        }

        public void PauseGame(bool showPause = true)
        {
            LogDebug("PauseGame called");

            gameInput.EnableUIMode();
            Time.timeScale = 0;
            gameIsPaused = true;

            if (showPause)
            {
                if (uiManagerV2 != null)
                {
                    // Use the new UI manager to show the pause menu
                    uiManagerV2.ShowPauseMenu();
                }
                else if (useNewPauseMenu)
                {
                    HandleNewPauseMenuActivation();
                }
                else
                {
                    HandleOldPauseMenuActivation();
                }
            }
        }

        void HandleNewPauseMenuActivation()
        {
            if (pauseMenuController != null)
            {
                LogDebug("Using new pause menu system");

                pauseMenuController.gameObject.SetActive(true);

                if (directPauseMenuReference != null)
                {
                    PauseCanvasManager canvasManager = directPauseMenuReference.GetComponent<PauseCanvasManager>();
                    if (canvasManager != null)
                    {
                        canvasManager.ShowGlobalElements();
                        LogDebug("Showing global elements via PauseCanvasManager");
                    }
                }

                pauseMenuController.Show();

            }
            else
            {
                LogDebug("New pause menu not found, falling back to old system");
                HandleOldPauseMenuActivation();
            }
        }

        void HandleOldPauseMenuActivation()
        {
            if (pauseMenuUI != null)
            {
                LogDebug("Using old pause menu system");
                pauseMenuUI.SetActive(true);
                pauseMenuUI.GetComponent<PauseMenuUI>().UpdateCharms();
            }
            else
            {
                LogDebug("No pause menu available!");
            }
        }

        public void ResumeGame()
        {
            LogDebug("ResumeGame called");

            StartInputResetSequence();

            Time.timeScale = 1;
            gameIsPaused = false;

            if (uiManagerV2 != null)
            {
                // Use the new UI manager to hide the pause menu
                uiManagerV2.HidePauseMenu();
            }
            else if (useNewPauseMenu && pauseMenuController != null)
            {
                LogDebug("Hiding new pause menu");
                pauseMenuController.Hide();

                if (directPauseMenuReference != null)
                {
                    PauseCanvasManager canvasManager = directPauseMenuReference.GetComponent<PauseCanvasManager>();

                    if (canvasManager != null)
                    {
                        canvasManager.HideGlobalElements();

                        canvasManager.HideAllTabs();

                        LogDebug("Hiding global elements and all tabs via PauseCanvasManager");
                    }
                    else
                    {
                        pauseMenuController.gameObject.SetActive(false);

                        PauseMenuSettingsController settingsController = directPauseMenuReference.GetComponentInChildren<PauseMenuSettingsController>(true);

                        if (settingsController != null)
                        {
                            settingsController.gameObject.SetActive(false);
                            LogDebug("Manually deactivated settings controller");
                        }
                    }
                }
            }
            else if (pauseMenuUI != null)
            {
                LogDebug("Hiding old pause menu");
                pauseMenuUI.SetActive(false);
            }
            else if (useNewCharmSwapUI && charmSwapMenuController != null)
            {
                charmSwapMenuController.Hide();

                if (charmSwapMenuController.WasActiveBeforePause)
                {
                    charmSwapMenuController.RestoreAfterPause();
                }
                else
                {
                    charmSwapMenuController.Hide();
                }
            }

        }

        void StartInputResetSequence()
        {
            if (isResettingInputs) return;

            isResettingInputs = true;

            DisableAllInputs();

            Invoke("FinishInputReset", inputResetDelay);

            LogDebug("Started input reset sequence");
        }

        void DisableAllInputs()
        {
            if (gameInput != null && gameInput.playerInputActions != null)
            {
                gameInput.DisableInput(ProjectColombo.GameInputSystem.InputActionType.Pause);

                if (gameInput.playerInputActions.Player.enabled)
                {
                    gameInput.playerInputActions.Player.Disable();
                }

                if (gameInput.playerInputActions.UI.enabled)
                {
                    gameInput.playerInputActions.UI.Disable();
                }

                LogDebug("Temporarily disabled all inputs for reset");
            }
        }

        void FinishInputReset()
        {
            if (gameInput != null && gameInput.playerInputActions != null)
            {
                gameInput.ResetAllInputs();

                gameInput.ResetMovementInput();

                gameInput.playerInputActions.Player.Enable();

                gameInput.EnableInput(ProjectColombo.GameInputSystem.InputActionType.Pause);

                if (SceneManager.GetActiveScene().name == MAIN_MENU)
                {
                    gameInput.EnableUIMode();
                }
                else
                {
                    gameInput.DisableUIMode();
                    gameInput.EnableAllInputs();
                }

                LogDebug("Finished input reset sequence and cleared all input states");
            }

            isResettingInputs = false;
        }

        public void TogglePauseMenuSystem(bool useNew)
        {
            if (gameIsPaused)
            {
                ResumeGame();
            }

            useNewPauseMenu = useNew;

            if (useNewPauseMenu)
            {
                LogDebug("Switched to NEW pause menu system");
            }
            else
            {
                LogDebug("Switched to OLD pause menu system");
            }
        }

        public void ToggleCharmSwapSystem(bool useNew)
        {
            useNewCharmSwapUI = useNew;

            if (oldCharmSwapScreen == null)
            {
                oldCharmSwapScreen = GameObject.Find("CharmSelectScreen");

                if (oldCharmSwapScreen == null)
                {
                    oldCharmSwapScreen = FindFirstObjectByType<CharmSelectScreen>(FindObjectsInactive.Include)?.gameObject;
                }
            }

            if (useNewCharmSwapUI)
            {
                LogDebug("Switched to NEW charm swap menu system");

                if (oldCharmSwapScreen != null)
                {
                    LogDebug("Deactivating old charm swap screen");
                    oldCharmSwapScreen.SetActive(false);
                }

                if (directCharmSwapMenuReference != null)
                {
                    LogDebug("Activating new charm swap canvas");
                    directCharmSwapMenuReference.SetActive(true);

                    if (charmSwapMenuController != null)
                    {
                        charmSwapMenuController.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                LogDebug("Switched to OLD charm swap menu system");

                if (directCharmSwapMenuReference != null)
                {
                    LogDebug("Deactivating new charm swap canvas");
                    directCharmSwapMenuReference.SetActive(false);
                }

                if (oldCharmSwapScreen != null)
                {
                    LogDebug("Activating old charm swap screen (but keeping it hidden)");
                    oldCharmSwapScreen.SetActive(true);
                    oldCharmSwapScreen.SetActive(false);
                }
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF00>[GameManager] {message}</color>");
            }
        }
    }
}