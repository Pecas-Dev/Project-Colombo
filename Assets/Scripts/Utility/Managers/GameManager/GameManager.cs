using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectColombo.UI.Pausescreen;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement.Stats;


namespace ProjectColombo.GameManagement
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameInputSO gameInput;

        [Tooltip("Old pause menu reference")]
        public GameObject pauseMenuUI;

        [Tooltip("New pause menu reference (should be the PauseInventoryCanvas)")]
        [SerializeField] GameObject newPauseMenuCanvas;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;
        [SerializeField] bool enableInputActionMapLogs = false;

        [Header("Pause Menu System")]
        [Tooltip("Toggle between old and new pause menu systems")]
        [SerializeField] bool useNewPauseMenu = false;

        [Header("Transition")]
        [SerializeField] Animator transition;


        public bool gameIsPaused = false;


        // SCENE NAMES
        static string MAIN_MENU = "00_MainMenu";


        protected GameObject directPauseMenuReference;
        protected PauseMenuInventoryController pauseMenuController;

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

            float time = Time.timeScale;

            //Debug.Log(time);
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
        }

        public void RegisterPauseMenu(GameObject pauseMenu, PauseMenuInventoryController controller)
        {
            if (!useNewPauseMenu)
                return;

            directPauseMenuReference = pauseMenu;
            pauseMenuController = controller;
            LogDebug("Pause menu directly registered");
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

        private void HandleNewPauseMenuActivation()
        {
            if (pauseMenuController == null)
            {
                FindPauseMenuController();
            }

            if (pauseMenuController != null)
            {
                LogDebug("Using new pause menu system");

                if (directPauseMenuReference != null)
                {
                    directPauseMenuReference.SetActive(true);
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

        private void HandleOldPauseMenuActivation()
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

            gameInput.DisableUIMode();
            gameInput.EnableAllInputs();

            if (SceneManager.GetActiveScene().name == MAIN_MENU)
            {
                gameInput.EnableUIMode();
            }

            Time.timeScale = 1;
            gameIsPaused = false;

            if (useNewPauseMenu && pauseMenuController != null)
            {
                LogDebug("Hiding new pause menu");
                pauseMenuController.Hide();

                if (directPauseMenuReference != null)
                {
                    directPauseMenuReference.SetActive(false);
                }
            }
            else if (pauseMenuUI != null)
            {
                LogDebug("Hiding old pause menu");
                pauseMenuUI.SetActive(false);
            }
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

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF00>[GameManager] {message}</color>");
            }
        }
    }
}