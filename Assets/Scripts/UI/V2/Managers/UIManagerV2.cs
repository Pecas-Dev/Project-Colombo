using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ProjectColombo.UI.Pausescreen;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI
{
    public class UIManagerV2 : MonoBehaviour
    {
        [Header("Pause Menu References")]
        [SerializeField] GameObject pauseInventoryCanvas;
        [SerializeField] GameObject inventoryTab;
        [SerializeField] GameObject settingsTab;

        [Header("Charm Swap References")]
        [SerializeField] GameObject charmSwapCanvas;
        [SerializeField] GameObject charmSwapScreen;

        [Header("Main Menu References")]
        [SerializeField] GameObject mainMenuCanvas;
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject optionsMenu;

        [Header("Non-Pausable Scenes")]
        [SerializeField] string[] nonPausableScenes = { "00_MainMenu", "02_MaskSelection" };

        [Header("Input Settings")]
        [SerializeField] float inputBufferTime = 0.2f;
        [SerializeField] Key keyboardPauseKey = Key.Escape;

        [Header("Game Input")]
        [SerializeField] GameInputSO gameInputSO;


        [Header("Debug")]
        [SerializeField] bool enableDebugLogs = true;


        float lastPauseTime;


        Keyboard keyboard;
        Gamepad gamepad;


        UINavigationManager navigationManager;


        void Awake()
        {
            keyboard = Keyboard.current;
            gamepad = Gamepad.current;
            lastPauseTime = -inputBufferTime;

            SceneManager.sceneLoaded += OnSceneLoaded;

            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                GameObject navigationManagerObj = new GameObject("UINavigationManager");
                navigationManager = navigationManagerObj.AddComponent<UINavigationManager>();
                if (enableDebugLogs)
                {
                    LogDebug("Created UINavigationManager");
                }
            }
        }

        void Update()
        {
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogDebug($"Scene loaded: {scene.name}");

            if (scene.name == "00_MainMenu")
            {
                LogDebug("Main menu scene detected - activating main menu");
                StartCoroutine(ActivateMainMenuForScene());
            }
            else
            {
                LogDebug($"Non-main menu scene ({scene.name}) - hiding main menu");
                if (mainMenuCanvas != null)
                {
                    mainMenuCanvas.SetActive(false);
                }
            }
        }

        IEnumerator ActivateMainMenuForScene()
        {
            LogDebug("Starting main menu activation for scene");

            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            InitializeReferences();

            yield return new WaitForEndOfFrame();

            if (mainMenuCanvas != null)
            {
                LogDebug("Activating MainMenuCanvas");
                mainMenuCanvas.SetActive(true);

                if (mainMenu != null)
                {
                    LogDebug("Activating MainMenu");
                    mainMenu.SetActive(true);

                    MainMenuController controller = mainMenu.GetComponent<MainMenuController>();
                    if (controller != null)
                    {
                        LogDebug("Calling Show() on MainMenuController");
                        controller.Show();
                    }
                    else
                    {
                        LogDebug("MainMenuController component not found!");
                    }
                }
                else
                {
                    LogDebug("MainMenu reference is null!");
                }

                // Hide options menu
                if (optionsMenu != null)
                {
                    optionsMenu.SetActive(false);
                    LogDebug("Options menu hidden");
                }
            }
            else
            {
                LogDebug("MainMenuCanvas reference is null!");
            }

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.MainMenu);
                LogDebug("Navigation state set to MainMenu");
            }

            LogDebug("Main menu activation completed");
        }

        public void ShowMainMenu()
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "00_MainMenu")
            {
                LogDebug($"ShowMainMenu called but current scene is {currentScene} - ignoring request");
                return;
            }

            LogDebug("ShowMainMenu called - forcing main menu activation");

            if (mainMenuCanvas == null || mainMenu == null)
            {
                LogDebug("Error: Main Menu Canvas or Main Menu reference missing - attempting to find references");

                InitializeReferences();

                if (mainMenuCanvas == null || mainMenu == null)
                {
                    LogDebug("Still missing references after initialization attempt");
                    return;
                }
            }

            mainMenuCanvas.SetActive(true);
            mainMenu.SetActive(true);

            if (optionsMenu != null)
            {
                optionsMenu.SetActive(false);
            }

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.MainMenu);
            }

            MainMenuController mainMenuController = mainMenu.GetComponent<MainMenuController>();

            if (mainMenuController != null)
            {
                mainMenuController.Show();
                LogDebug("Successfully called Show() on MainMenuController");
            }
            else
            {
                LogDebug("MainMenuController component not found on main menu GameObject!");
            }
        }

        public void ShowOptionsMenu()
        {
            if (mainMenuCanvas == null || optionsMenu == null)
            {
                LogDebug("Error: Main Menu Canvas or Options Menu reference missing");
                return;
            }

            mainMenuCanvas.SetActive(true);
            optionsMenu.SetActive(true);

            if (mainMenu != null)
            {
                mainMenu.SetActive(false);
            }

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.OptionsMenuGraphicsTab);
            }

            OptionsMenuController optionsController = optionsMenu.GetComponent<OptionsMenuController>();

            if (optionsController != null)
            {
                optionsController.Show();
                LogDebug("Called Show() on OptionsMenuController");
            }
        }

        public void HideOptionsMenu()
        {
            if (optionsMenu != null)
            {
                OptionsMenuController optionsController = optionsMenu.GetComponent<OptionsMenuController>();
                if (optionsController != null)
                {
                    optionsController.Hide();
                    LogDebug("Called Hide() on OptionsMenuController");
                }

                optionsMenu.SetActive(false);
            }

            ShowMainMenu();
        }

        public bool CheckPauseInput()
        {
            if (Time.unscaledTime - lastPauseTime < inputBufferTime)
            {
                return false;
            }

            bool pausePressed = false;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                pausePressed = true;
            }

            if (gamepad != null && (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame) || gameInputSO.inputActions.Player.Pause.WasPressedThisFrame())
            {
                pausePressed = true;
            }

            if (pausePressed)
            {
                lastPauseTime = Time.unscaledTime;
                LogDebug("Pause input detected");
            }

            return pausePressed;
        }

        public bool CanPauseInCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            return !Array.Exists(nonPausableScenes, scene => scene == currentScene);
        }

        public void ShowPauseMenu()
        {
            if (pauseInventoryCanvas == null)
            {
                LogDebug("Error: Pause Inventory Canvas reference missing");
                return;
            }

            pauseInventoryCanvas.SetActive(true);

            if (inventoryTab != null)
            {
                inventoryTab.SetActive(true);
            }
            if (settingsTab != null)
            {
                settingsTab.SetActive(false);
            }


            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseInventoryTab);
            }

            PauseCanvasManager canvasManager = pauseInventoryCanvas.GetComponent<PauseCanvasManager>();

            if (canvasManager != null)
            {
                canvasManager.ShowGlobalElements();
                LogDebug("Showing global elements via PauseCanvasManager");
            }

            PauseMenuInventoryController controller = inventoryTab?.GetComponent<PauseMenuInventoryController>();
            if (controller != null)
            {
                controller.Show();
                LogDebug("Called Show() on PauseMenuInventoryController");
            }

            LogDebug("Pause menu shown");
        }

        public void HidePauseMenu()
        {
            if (pauseInventoryCanvas != null)
            {
                PauseCanvasManager canvasManager = pauseInventoryCanvas.GetComponent<PauseCanvasManager>();
                if (canvasManager != null)
                {
                    canvasManager.HideGlobalElements();
                    canvasManager.HideAllTabs();
                    LogDebug("Hidden elements via PauseCanvasManager");
                }
                else
                {
                    if (inventoryTab != null) inventoryTab.SetActive(false);
                    if (settingsTab != null) settingsTab.SetActive(false);
                    pauseInventoryCanvas.SetActive(false);
                    LogDebug("Manually hidden pause menu elements");
                }
            }

            LogDebug("Pause menu hidden");
        }

        public void ShowCharmSwapScreen()
        {
            if (charmSwapCanvas == null)
            {
                LogDebug("Error: Charm Swap Canvas reference missing");
                return;
            }

            charmSwapCanvas.SetActive(true);

            if (charmSwapScreen != null)
            {
                charmSwapScreen.SetActive(true);
            }

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
            }

            CharmSwapMenuController charmSwapController = charmSwapScreen?.GetComponent<CharmSwapMenuController>();
            if (charmSwapController != null)
            {
                charmSwapController.Show();
                LogDebug("Called Show() on CharmSwapMenuController");
            }

            LogDebug("Charm swap screen shown");
        }

        public void HideCharmSwapScreen()
        {
            if (charmSwapCanvas != null)
            {
                CharmSwapMenuController charmSwapController = charmSwapScreen?.GetComponent<CharmSwapMenuController>();
                if (charmSwapController != null)
                {
                    charmSwapController.Hide();
                    LogDebug("Called Hide() on CharmSwapMenuController");
                }

                if (charmSwapScreen != null)
                {
                    charmSwapScreen.SetActive(false);
                }

                charmSwapCanvas.SetActive(false);
            }

            LogDebug("Charm swap screen hidden");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF00FF>[UIManagerV2] {message}</color>");
            }
        }

        public void InitializeReferences()
        {
            LogDebug("Initializing UI references - looking for children of GameManager");

            GameObject gameManagerObj = GameObject.Find("GameManager");

            if (gameManagerObj == null)
            {
                LogDebug("GameManager object not found!");
                return;
            }

            if (mainMenuCanvas == null)
            {
                Transform mainMenuCanvasTransform = gameManagerObj.transform.Find("MainMenuCanvas");
                if (mainMenuCanvasTransform != null)
                {
                    mainMenuCanvas = mainMenuCanvasTransform.gameObject;
                    LogDebug("Found MainMenuCanvas as child of GameManager");
                }
                else
                {
                    LogDebug("MainMenuCanvas not found as child of GameManager!");
                }
            }

            if (mainMenuCanvas != null)
            {
                if (mainMenu == null)
                {
                    Transform mainMenuTransform = mainMenuCanvas.transform.Find("MainMenu");
                    if (mainMenuTransform != null)
                    {
                        mainMenu = mainMenuTransform.gameObject;
                        LogDebug("Found MainMenu as child of MainMenuCanvas");
                    }
                    else
                    {
                        LogDebug("MainMenu not found as child of MainMenuCanvas!");
                    }
                }

                if (optionsMenu == null)
                {
                    Transform optionsMenuTransform = mainMenuCanvas.transform.Find("OptionsMenu");
                    if (optionsMenuTransform != null)
                    {
                        optionsMenu = optionsMenuTransform.gameObject;
                        LogDebug("Found OptionsMenu as child of MainMenuCanvas");
                    }
                    else
                    {
                        LogDebug("OptionsMenu not found as child of MainMenuCanvas!");
                    }
                }
            }

            if (pauseInventoryCanvas == null)
            {
                Transform pauseCanvasTransform = gameManagerObj.transform.Find("PauseInventoryCanvas");
                if (pauseCanvasTransform != null)
                {
                    pauseInventoryCanvas = pauseCanvasTransform.gameObject;
                    LogDebug("Found PauseInventoryCanvas as child of GameManager");
                }
                else
                {
                    LogDebug("PauseInventoryCanvas not found as child of GameManager!");
                }
            }

            if (pauseInventoryCanvas != null)
            {
                if (inventoryTab == null)
                {
                    Transform inventoryTabTransform = pauseInventoryCanvas.transform.Find("PauseInventory");
                    if (inventoryTabTransform != null)
                    {
                        inventoryTab = inventoryTabTransform.gameObject;
                        LogDebug("Found InventoryTab (PauseInventory) as child of PauseInventoryCanvas");
                    }
                    else
                    {
                        LogDebug("PauseInventory not found as child of PauseInventoryCanvas!");
                    }
                }

                if (settingsTab == null)
                {
                    Transform settingsTabTransform = pauseInventoryCanvas.transform.Find("PauseSettings");
                    if (settingsTabTransform != null)
                    {
                        settingsTab = settingsTabTransform.gameObject;
                        LogDebug("Found SettingsTab (PauseSettings) as child of PauseInventoryCanvas");
                    }
                    else
                    {
                        LogDebug("PauseSettings not found as child of PauseInventoryCanvas!");
                    }
                }
            }

            if (charmSwapCanvas == null)
            {
                Transform charmSwapCanvasTransform = gameManagerObj.transform.Find("CharmSwapCanvas");
                if (charmSwapCanvasTransform != null)
                {
                    charmSwapCanvas = charmSwapCanvasTransform.gameObject;
                    LogDebug("Found CharmSwapCanvas as child of GameManager");
                }
                else
                {
                    LogDebug("CharmSwapCanvas not found as child of GameManager!");
                }
            }

            if (charmSwapScreen == null && charmSwapCanvas != null)
            {
                Transform charmSwapScreenTransform = charmSwapCanvas.transform.Find("CharmSwapController");
                if (charmSwapScreenTransform != null)
                {
                    charmSwapScreen = charmSwapScreenTransform.gameObject;
                    LogDebug("Found CharmSwapController as child of CharmSwapCanvas");
                }
                else
                {
                    LogDebug("CharmSwapController not found as child of CharmSwapCanvas!");
                }
            }

            LogDebug("UI references initialization completed");
        }

        GameObject FindChildWithName(GameObject parent, string name)
        {
            Transform result = parent.transform.Find(name);
            if (result != null) return result.gameObject;

            foreach (Transform child in parent.transform)
            {
                if (child.name == name) return child.gameObject;
            }

            return null;
        }
    }
}