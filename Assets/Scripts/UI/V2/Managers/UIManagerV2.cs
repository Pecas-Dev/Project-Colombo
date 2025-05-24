using System;
using UnityEngine;
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
            if (mainMenuCanvas != null)
            {
                bool isMainMenuScene = scene.name == "00_MainMenu";
                mainMenuCanvas.SetActive(isMainMenuScene);

                if (isMainMenuScene)
                {
                    if (mainMenu != null)
                    {
                        mainMenu.SetActive(true);
                    }

                    if (optionsMenu != null)
                    {
                        optionsMenu.SetActive(false);
                    }

                    MainMenuController mainMenuController = mainMenu?.GetComponent<MainMenuController>();
                    if (mainMenuController != null)
                    {
                        mainMenuController.Show();
                    }
                }
            }
        }

        public void ShowMainMenu()
        {
            if (mainMenuCanvas == null || mainMenu == null)
            {
                LogDebug("Error: Main Menu Canvas or Main Menu reference missing");
                return;
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
                LogDebug("Called Show() on MainMenuController");
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

            if (gamepad != null && /*(gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame) ||*/ gameInputSO.inputActions.Player.Pause.WasPressedThisFrame())
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
            if (pauseInventoryCanvas == null)
            {
                pauseInventoryCanvas = GameObject.Find("PauseInventoryCanvas");
                if (pauseInventoryCanvas == null)
                {
                    LogDebug("Could not find PauseInventoryCanvas");
                }
            }

            if (inventoryTab == null && pauseInventoryCanvas != null)
            {
                inventoryTab = FindChildWithName(pauseInventoryCanvas, "InventoryTab");
                if (inventoryTab == null)
                {
                    LogDebug("Could not find InventoryTab");
                }
            }

            if (settingsTab == null && pauseInventoryCanvas != null)
            {
                settingsTab = FindChildWithName(pauseInventoryCanvas, "SettingsTab");
                if (settingsTab == null)
                {
                    LogDebug("Could not find SettingsTab");
                }
            }

            if (mainMenuCanvas == null)
            {
                mainMenuCanvas = GameObject.Find("MainMenuCanvas");
                if (mainMenuCanvas == null)
                {
                    LogDebug("Could not find MainMenuCanvas");
                }
            }

            if (mainMenu == null && mainMenuCanvas != null)
            {
                mainMenu = FindChildWithName(mainMenuCanvas, "MainMenu");
                if (mainMenu == null)
                {
                    LogDebug("Could not find MainMenu");
                }
            }

            if (optionsMenu == null && mainMenuCanvas != null)
            {
                optionsMenu = FindChildWithName(mainMenuCanvas, "OptionsMenu");
                if (optionsMenu == null)
                {
                    LogDebug("Could not find OptionsMenu");
                }
            }

            if (charmSwapCanvas == null)
            {
                charmSwapCanvas = GameObject.Find("CharmSwapCanvas");
                if (charmSwapCanvas == null)
                {
                    LogDebug("Could not find CharmSwapCanvas");
                }
            }

            if (charmSwapScreen == null && charmSwapCanvas != null)
            {
                charmSwapScreen = FindChildWithName(charmSwapCanvas, "CharmSwapController");
                if (charmSwapScreen == null)
                {
                    LogDebug("Could not find CharmSwapController");
                }
            }

            LogDebug("UI references initialized");
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