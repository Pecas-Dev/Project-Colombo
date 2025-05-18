using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ProjectColombo.UI.Pausescreen;
using System;


namespace ProjectColombo.UI
{
    public class UIManagerV2 : MonoBehaviour
    {
        [Header("Pause Menu References")]
        [SerializeField] GameObject pauseInventoryCanvas;
        [SerializeField] GameObject inventoryTab;
        [SerializeField] GameObject settingsTab;

        [Header("Main Menu References")]
        [SerializeField] GameObject mainMenuCanvas;
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject optionsMenu;

        [Header("Non-Pausable Scenes")]
        [SerializeField] string[] nonPausableScenes = { "00_MainMenu", "01_MaskSelection" };

        [Header("Input Settings")]
        [SerializeField] float inputBufferTime = 0.2f;
        [SerializeField] Key keyboardPauseKey = Key.Escape;

        [Header("Debug")]
        [SerializeField] bool enableDebugLogs = true;

        Keyboard keyboard;
        Gamepad gamepad;
        float lastPauseTime;

        void Awake()
        {
            keyboard = Keyboard.current;
            gamepad = Gamepad.current;
            lastPauseTime = -inputBufferTime;

            SceneManager.sceneLoaded += OnSceneLoaded;
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

            if (gamepad != null && (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame))
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

            if (inventoryTab != null) inventoryTab.SetActive(true);
            if (settingsTab != null) settingsTab.SetActive(false);

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