using UnityEngine;
using UnityEngine.SceneManagement;
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

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            if (useNewPauseMenu)
            {
                FindPauseMenuController();

                if (pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                }
            }

            EnsureCorrectCharmSwapSystemActive();
        }

        void FindPauseMenuController()
        {
            if (!useNewPauseMenu)
            {
                return;
            }


            LogDebug("Searching for PauseMenuInventoryController...");

            if (newPauseMenuCanvas != null)
            {
                directPauseMenuReference = newPauseMenuCanvas;
                pauseMenuController = newPauseMenuCanvas.GetComponentInChildren<PauseMenuInventoryController>(true);

                if (pauseMenuController != null)
                {
                    LogDebug("Found PauseMenuInventoryController via direct reference");
                    return;
                }
            }

            GameObject pauseCanvas = GameObject.Find("PauseInventoryCanvas");

            if (pauseCanvas != null)
            {
                pauseMenuController = pauseCanvas.GetComponentInChildren<PauseMenuInventoryController>(true);
                if (pauseMenuController != null)
                {
                    directPauseMenuReference = pauseCanvas;
                    LogDebug("Found PauseMenuInventoryController via GameObject.Find");
                    return;
                }
            }

            pauseMenuController = FindFirstObjectByType<PauseMenuInventoryController>(FindObjectsInactive.Include);

            if (pauseMenuController != null)
            {
                directPauseMenuReference = pauseMenuController.transform.parent.gameObject;
                LogDebug("Found PauseMenuInventoryController via FindFirstObjectByType");
                return;
            }

            if (UIManager.Instance != null)
            {
                var controller = UIManager.Instance.GetMenu<PauseMenuInventoryController>();
                if (controller != null)
                {
                    pauseMenuController = controller;
                    directPauseMenuReference = controller.transform.parent.gameObject;
                    LogDebug("Found PauseMenuInventoryController via UIManager");
                    return;
                }
            }

            LogDebug("PauseMenuInventoryController not found - will use old pause menu system");
        }

        public void FindCharmSwapMenuController()
        {
            if (!useNewCharmSwapUI)
            {
                return;
            }

            LogDebug("Searching for CharmSwapMenuController...");

            if (newCharmSwapCanvas != null)
            {
                directCharmSwapMenuReference = newCharmSwapCanvas;
                charmSwapMenuController = newCharmSwapCanvas.GetComponentInChildren<CharmSwapMenuController>(true);

                if (charmSwapMenuController != null)
                {
                    LogDebug("Found CharmSwapMenuController via direct reference");
                    return;
                }
            }

            GameObject charmSwapCanvas = GameObject.Find("CharmSwapCanvas");

            if (charmSwapCanvas != null)
            {
                charmSwapMenuController = charmSwapCanvas.GetComponentInChildren<CharmSwapMenuController>(true);
                if (charmSwapMenuController != null)
                {
                    directCharmSwapMenuReference = charmSwapCanvas;
                    LogDebug("Found CharmSwapMenuController via GameObject.Find");
                    return;
                }
            }

            charmSwapMenuController = FindFirstObjectByType<CharmSwapMenuController>(FindObjectsInactive.Include);

            if (charmSwapMenuController != null)
            {
                directCharmSwapMenuReference = charmSwapMenuController.transform.parent.gameObject;
                LogDebug("Found CharmSwapMenuController via FindFirstObjectByType");
                return;
            }

            if (UIManager.Instance != null)
            {
                var controller = UIManager.Instance.GetMenu<CharmSwapMenuController>();
                if (controller != null)
                {
                    charmSwapMenuController = controller;
                    directCharmSwapMenuReference = controller.transform.parent.gameObject;
                    LogDebug("Found CharmSwapMenuController via UIManager");
                    return;
                }
            }

            LogDebug("CharmSwapMenuController not found - will use old charm swap system");
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

            if (directCharmSwapMenuReference == null)
            {
                FindCharmSwapMenuController();
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
            if (gameInput.PausePressed)
            {
                if (gameIsPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }

                gameInput.ResetPausePressed();
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
                Invoke("FindPauseMenuController", 0.1f);
            }

            if (useNewPauseMenu)
            {
                directPauseMenuReference = null;
                pauseMenuController = null;
                Invoke("FindPauseMenuController", 0.1f);
            }

            if (useNewCharmSwapUI)
            {
                directCharmSwapMenuReference = null;
                charmSwapMenuController = null;
                Invoke("FindCharmSwapMenuController", 0.2f);
            }

            Invoke("EnsureCorrectCharmSwapSystemActive", 0.3f);
        }

        public void RegisterPauseMenu(GameObject pauseMenu, PauseMenuInventoryController controller)
        {
            if (!useNewPauseMenu)
            {
                return;
            }

            directPauseMenuReference = pauseMenu;
            pauseMenuController = controller;
            LogDebug("Pause menu directly registered");
        }

        public void RegisterCharmSwapMenu(GameObject charmSwapMenu, CharmSwapMenuController controller)
        {
            if (!useNewCharmSwapUI)
            {
                return;
            }

            directCharmSwapMenuReference = charmSwapMenu;
            charmSwapMenuController = controller;
            LogDebug("Charm swap menu directly registered");
        }

        public void PauseGame(bool showPause = true)
        {
            LogDebug("PauseGame called");

            gameInput.EnableUIMode();
            Time.timeScale = 0;
            gameIsPaused = true;

            if (showPause)
            {
                if (useNewPauseMenu)
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
            if (pauseMenuController == null)
            {
                FindPauseMenuController();
            }

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

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMenuByType<PauseMenuInventoryController>();
                }
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

            if (useNewPauseMenu && pauseMenuController != null)
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
        }

        void StartInputResetSequence()
        {
            if (isResettingInputs) return;

            isResettingInputs = true;

            if (gameInput != null)
            {
                gameInput.ResetPausePressed();
            }

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
                gameInput.playerInputActions.Player.Enable();

                gameInput.EnableInput(ProjectColombo.GameInputSystem.InputActionType.Pause);

                gameInput.ResetPausePressed();

                if (SceneManager.GetActiveScene().name == MAIN_MENU)
                {
                    gameInput.EnableUIMode();
                }
                else
                {
                    gameInput.DisableUIMode();
                    gameInput.EnableAllInputs();
                }

                LogDebug("Finished input reset sequence");
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
                FindPauseMenuController();
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

            if (directCharmSwapMenuReference == null)
            {
                FindCharmSwapMenuController();
            }

            if (useNewCharmSwapUI)
            {
                LogDebug("Switched to NEW charm swap menu system");
                FindCharmSwapMenuController();

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