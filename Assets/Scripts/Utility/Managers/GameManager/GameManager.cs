using UnityEngine;
using ProjectColombo.UI;
using System.Collections;
using ProjectColombo.Inventory;
using UnityEngine.SceneManagement;
using ProjectColombo.Objects.Masks;
using ProjectColombo.UI.Pausescreen;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement.Stats;


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

        void Awake()
        {
            GlobalStats globalStats = GetComponent<GlobalStats>();
            globalStats.enabled = true;

            gameInput.Initialize();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

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

            if (uiManagerV2 != null)
            {
                uiManagerV2.InitializeReferences();
            }
        }

        void EnsureCorrectCharmSwapSystemActive()
        {

            if (newCharmSwapCanvas == null)
            {
                newCharmSwapCanvas = GameObject.Find("CharmSwapCanvas");
                LogDebug("Found CharmSwapCanvas: " + (newCharmSwapCanvas != null));
            }

            if (charmSwapMenuController == null && newCharmSwapCanvas != null)
            {
                Transform charmSwapControllerTransform = newCharmSwapCanvas.transform.Find("CharmSwapController");
                if (charmSwapControllerTransform != null)
                {
                    charmSwapMenuController = charmSwapControllerTransform.GetComponent<CharmSwapMenuController>();
                    directCharmSwapMenuReference = charmSwapControllerTransform.gameObject;
                    LogDebug("Found CharmSwapMenuController: " + (charmSwapMenuController != null));
                }
                else
                {
                    LogDebug("CharmSwapController not found as child of CharmSwapCanvas!");
                }
            }

            if (newCharmSwapCanvas != null)
            {
                newCharmSwapCanvas.SetActive(true);
                LogDebug("CharmSwapCanvas activated");

                if (directCharmSwapMenuReference != null)
                {
                    directCharmSwapMenuReference.SetActive(false);
                    LogDebug("CharmSwapController initially hidden");
                }
            }
        }

        void Update()
        {
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

            if (gameInput.GetInputPressed(PlayerInputAction.Pause))
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

            if (gameIsPaused && gameInput.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                ResumeGame();
            }

            if (gameInput.IsPlayerMapEnabled && enableInputActionMapLogs)
            {
                Debug.Log("PLAYERRRRRRRRRR!");
            }
            if (gameInput.IsUIMapEnabled && enableInputActionMapLogs)
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
            GlobalStats globalStats = GetComponent<GlobalStats>();
            globalStats.enabled = true;

            ResumeGame();

            if (scene.name == "00_MainMenu")
            {
                LogDebug("Main menu scene loaded - forcing UI manager initialization");

                if (uiManagerV2 != null)
                {
                    uiManagerV2.InitializeReferences();

                    StartCoroutine(ForceShowMainMenuAfterDelay());
                }
                else
                {
                    LogDebug("UIManagerV2 is null!");
                }

                if (gameInput != null)
                {
                    gameInput.SwitchToUI();
                    LogDebug("Switched to UI input for main menu");
                }
            }

            if (scene.name == "03_LevelTwo")
            {
                ResetAllEchoMissions();
            }

            if (scene.name == "01_Tutorial" || scene.name == "00_MainMenu" || scene.name == "03_LevelTwo")
            {
                ResetCharmButtonColors();
            }

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

            EnsureCorrectCharmSwapSystemActive();

            if (uiManagerV2 != null)
            {
                uiManagerV2.InitializeReferences();
            }
        }

        public void PlayCloseTransition()
        {
            if (transition != null)
            {
                transition.updateMode = AnimatorUpdateMode.UnscaledTime;

                transition.Play("Close");
                LogDebug("Playing Close transition animation");
            }
        }

        public void PauseGame(bool showPause = true)
        {
            LogDebug("PauseGame called");

            gameInput.SwitchToUI();
            Time.timeScale = 0;
            gameIsPaused = true;

            if (showPause)
            {
                if (uiManagerV2 != null)
                {
                    uiManagerV2.ShowPauseMenu();
                }
                else if (useNewPauseMenu)
                {
                    if (newPauseMenuCanvas != null)
                    {
                        newPauseMenuCanvas.SetActive(true);
                    }
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
                uiManagerV2.HidePauseMenu();
            }
            else if (useNewPauseMenu)
            {
                if (newPauseMenuCanvas != null)
                {
                    newPauseMenuCanvas.SetActive(false);
                }
            }
            else if (pauseMenuUI != null)
            {
                LogDebug("Hiding old pause menu");
                pauseMenuUI.SetActive(false);
            }

            if (gameInput != null)
            {
                gameInput.SwitchToGameplay();
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
            gameInput.DisableAllInputs();
            LogDebug("Temporarily disabled all inputs for reset");
        }

        void FinishInputReset()
        {
            if (gameInput != null && gameInput.inputActions != null)
            {

                if (SceneManager.GetActiveScene().name == MAIN_MENU)
                {
                    gameInput.SwitchToUI();
                }
                else
                {
                    gameInput.SwitchToGameplay();
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

            if (newCharmSwapCanvas == null)
            {
                newCharmSwapCanvas = GameObject.Find("CharmSwapCanvas");
            }

            if (charmSwapMenuController == null && newCharmSwapCanvas != null)
            {
                Transform charmSwapControllerTransform = newCharmSwapCanvas.transform.Find("CharmSwapController");
                if (charmSwapControllerTransform != null)
                {
                    charmSwapMenuController = charmSwapControllerTransform.GetComponent<CharmSwapMenuController>();
                    directCharmSwapMenuReference = charmSwapControllerTransform.gameObject;
                }
            }


            if (newCharmSwapCanvas != null)
            {
                LogDebug("Activating new charm swap canvas");
                newCharmSwapCanvas.SetActive(true);

                if (directCharmSwapMenuReference != null)
                {
                    directCharmSwapMenuReference.SetActive(false);
                }
            }

            if (newCharmSwapCanvas != null)
            {
                LogDebug("Deactivating new charm swap canvas");
                newCharmSwapCanvas.SetActive(false);
            }

        }

        void ResetAllEchoMissions()
        {
            if (GetComponent<PlayerInventory>() != null)
            {
                PlayerInventory inventory = GetComponent<PlayerInventory>();

                if (inventory.maskSlot != null && inventory.maskSlot.transform.childCount > 0)
                {
                    BaseMask mask = inventory.maskSlot.transform.GetChild(0).GetComponent<BaseMask>();
                    if (mask != null)
                    {
                        if (mask.echoUnlocked)
                        {
                            System.Reflection.FieldInfo echoUnlockedField = typeof(BaseMask).GetField("echoUnlocked");
                            if (echoUnlockedField != null)
                            {
                                echoUnlockedField.SetValue(mask, false);
                                LogDebug("Reset echoUnlocked flag for mask: " + mask.maskName);
                            }
                        }

                        mask.ResetEchoMission();
                        LogDebug("Reset echo mission for equipped mask: " + mask.maskName);
                    }
                }
            }
        }

        void ResetCharmButtonColors()
        {
            PauseMenuInventoryTabController inventoryTabController = FindFirstObjectByType<PauseMenuInventoryTabController>();

            if (inventoryTabController != null)
            {
                inventoryTabController.ResetAllCharmButtonColorsToWhite();
                LogDebug($"Reset charm button colors to white for scene: {SceneManager.GetActiveScene().name}");
            }
            else
            {
                LogDebug("Could not find PauseMenuInventoryTabController to reset charm button colors");
            }
        }

        IEnumerator ForceShowMainMenuAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.3f);

            LogDebug("Forcing main menu show after delay");

            if (uiManagerV2 != null)
            {
                uiManagerV2.ShowMainMenu();
                LogDebug("Called ShowMainMenu on UIManagerV2");
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