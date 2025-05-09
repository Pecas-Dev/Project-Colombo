using UnityEngine;
using ProjectColombo.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ProjectColombo.GameInputSystem;
using ProjectColombo.UI.Pausescreen;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameInputSO gameInputSO;

    [Header("Debug Settings")]
    [SerializeField] bool enableDebugLogs = false;

    MenuController currentActiveMenu;
    Dictionary<string, MenuController> menuCache = new Dictionary<string, MenuController>();

    bool hasInitializedScene = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasInitializedScene = false;
        currentActiveMenu = null;
        menuCache.Clear();
        FindAndInitializeMenus();
    }

    void Start()
    {
        FindAndInitializeMenus();
    }

    void FindAndInitializeMenus()
    {
        if (hasInitializedScene)
        {
            return;
        }

        MenuController[] allMenus = FindObjectsByType<MenuController>(FindObjectsSortMode.None);

        if (enableDebugLogs)
        {
            Debug.Log($"[UIManager] Found {allMenus.Length} menu controllers in scene {SceneManager.GetActiveScene().name}");
        }

        foreach (MenuController menu in allMenus)
        {
            if (menu != null)
            {
                string menuType = menu.GetType().Name;
                menuCache[menuType] = menu;

                menu.Initialize();
                menu.Hide();

                if (enableDebugLogs)
                {
                    Debug.Log($"[UIManager] Initialized and cached menu: {menuType}");
                }
            }
        }

        if (menuCache.TryGetValue("MainMenuController", out MenuController mainMenu))
        {
            ShowMenu(mainMenu);
        }

        hasInitializedScene = true;
    }

    void Update()
    {
        if (currentActiveMenu != null)
        {
            currentActiveMenu.HandleInput();
        }

        if (gameInputSO != null && gameInputSO.playerInputActions != null && gameInputSO.playerInputActions.UI.Cancel.WasPressedThisFrame())
        {
            if (currentActiveMenu != null && currentActiveMenu.GetType().Name != "MainMenuController")
            {
                ShowMainMenu();
            }
        }
    }

    public void RegisterPauseMenu(GameObject pauseMenuObject)
    {
        if (pauseMenuObject == null)
        {
            return;
        }

        PauseMenuInventoryController pauseController = pauseMenuObject.GetComponentInChildren<PauseMenuInventoryController>(true);

        if (pauseController != null)
        {
            string menuType = pauseController.GetType().Name;
            menuCache[menuType] = pauseController;
            pauseController.Initialize();
            pauseController.Hide();

            if (enableDebugLogs)
            {
                Debug.Log($"[UIManager] Successfully registered pause menu from specified GameObject");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[UIManager] Failed to find PauseMenuInventoryController in the specified GameObject");
            }
        }
    }

    public void ShowMenu(MenuController menuToShow)
    {
        if (menuToShow == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[UIManager] Attempted to show null menu");
            }

            return;
        }

        if (currentActiveMenu != null)
        {
            currentActiveMenu.Hide();
        }

        menuToShow.Show();
        currentActiveMenu = menuToShow;

        if (enableDebugLogs)
        {
            Debug.Log($"[UIManager] Showing menu: {menuToShow.GetType().Name}");
        }
    }

    public void ShowMainMenu()
    {
        MenuController menu = GetMenu<MainMenuController>();
        if (menu != null)
        {
            ShowMenu(menu);
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[UIManager] MainMenuController not found in the current scene.");
        }
    }

    public void ShowOptionsMenu()
    {
        MenuController menu = GetMenu<PauseMenuInventoryController>();
        if (menu != null)
        {
            ShowMenu(menu);
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[UIManager] OptionsMenuController not found in the current scene.");
        }
    }

    public T GetMenu<T>() where T : MenuController
    {
        string menuType = typeof(T).Name;

        if (menuCache.TryGetValue(menuType, out MenuController cachedMenu))
        {
            return cachedMenu as T;
        }

        T foundMenu = FindFirstObjectByType<T>();

        if (foundMenu != null)
        {
            menuCache[menuType] = foundMenu;
            foundMenu.Initialize();
            foundMenu.Hide();

            if (enableDebugLogs)
            {
                Debug.Log($"[UIManager] Found and cached menu: {menuType}");
            }
        }

        return foundMenu;
    }

    public void ShowMenuByType<T>() where T : MenuController
    {
        T menu = GetMenu<T>();
        if (menu != null)
        {
            ShowMenu(menu);
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[UIManager] Menu of type {typeof(T).Name} not found in the current scene.");
        }
    }

    public void ReinitializeMenus()
    {
        Dictionary<string, MenuController> persistentMenus = new Dictionary<string, MenuController>();

        foreach (var entry in menuCache)
        {
            if (entry.Value != null && entry.Value.gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistentMenus.Add(entry.Key, entry.Value);
                entry.Value.Reinitialize();
            }
        }

        menuCache.Clear();

        foreach (var entry in persistentMenus)
        {
            menuCache.Add(entry.Key, entry.Value);
        }

        FindAndInitializeMenus();
    }

    public MenuController GetCurrentMenu()
    {
        return currentActiveMenu;
    }

    public bool HasMenu<T>() where T : MenuController
    {
        return GetMenu<T>() != null;
    }
}