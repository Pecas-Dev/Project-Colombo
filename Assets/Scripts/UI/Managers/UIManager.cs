using UnityEngine;
using ProjectColombo.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ProjectColombo.GameInputSystem;
using ProjectColombo.UI.Pausescreen;
using System.Collections;
using ProjectColombo.GameManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameInputSO gameInputSO;

    [Header("Debug Settings")]
    [SerializeField] bool enableDebugLogs = false;

    MenuController currentActiveMenu;
    Dictionary<string, MenuController> menuCache = new Dictionary<string, MenuController>();

    bool hasInitializedScene = false;

    bool isProcessingInputChange = false;
    bool isClosingPauseMenu = false;

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
        StartCoroutine(ScheduleMenuSearches());
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

        MenuController[] allMenus = FindObjectsByType<MenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (enableDebugLogs)
        {
            System.Text.StringBuilder menusList = new System.Text.StringBuilder();
            menusList.AppendLine($"[UIManager] Found {allMenus.Length} menu controllers in scene {SceneManager.GetActiveScene().name}:");

            foreach (MenuController menu in allMenus)
            {
                if (menu != null)
                {
                    string menuPath = GetGameObjectPath(menu.gameObject);
                    menusList.AppendLine($"- {menu.GetType().Name} at {menuPath}");
                }
            }

            Debug.Log(menusList.ToString());
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

    void SearchForMenusAgain()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[UIManager] Performing scheduled search for menu controllers...");
        }

        MenuController[] allMenus = FindObjectsByType<MenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<MenuController> newMenus = new List<MenuController>();

        foreach (MenuController menu in allMenus)
        {
            if (menu != null)
            {
                string menuType = menu.GetType().Name;

                if (!menuCache.ContainsKey(menuType))
                {
                    newMenus.Add(menu);
                    menuCache[menuType] = menu;
                    menu.Initialize();
                    menu.Hide();
                }
            }
        }

        if (enableDebugLogs && newMenus.Count > 0)
        {
            System.Text.StringBuilder newMenusLog = new System.Text.StringBuilder();
            newMenusLog.AppendLine($"[UIManager] Found {newMenus.Count} new menu controllers:");

            foreach (MenuController menu in newMenus)
            {
                string menuPath = GetGameObjectPath(menu.gameObject);
                newMenusLog.AppendLine($"- {menu.GetType().Name} at {menuPath}");
            }

            Debug.Log(newMenusLog.ToString());
        }
        else if (enableDebugLogs)
        {
            Debug.Log("[UIManager] No new menu controllers found in additional search.");
        }
    }


    void Update()
    {
        if (currentActiveMenu != null)
        {
            currentActiveMenu.HandleInput();
        }

        if (isProcessingInputChange || isClosingPauseMenu)
        {
            return;
        }

        if (gameInputSO != null && gameInputSO.playerInputActions != null && gameInputSO.playerInputActions.UI.Cancel.WasPressedThisFrame())
        {
            if (currentActiveMenu != null)
            {
                if (currentActiveMenu is PauseMenuInventoryController pauseMenuController || currentActiveMenu is PauseMenuSettingsController settingsController)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ResumeGame();
                    }

                    if (enableDebugLogs)
                    {
                        Debug.Log("[UIManager] Closing pause menu on cancel input and resuming game time");
                    }

                    currentActiveMenu = null;
                }
                else if (currentActiveMenu.GetType().Name != "MainMenuController")
                {
                    ShowMainMenu();
                }
            }
        }
    }

    IEnumerator ScheduleMenuSearches()
    {
        yield return new WaitForSeconds(5f);
        SearchForMenusAgain();

        yield return new WaitForSeconds(5f);
        SearchForMenusAgain();

        yield return new WaitForSeconds(5f);
        SearchForMenusAgain();
    }

    void EnablePauseInput()
    {
        if (gameInputSO != null)
        {
            gameInputSO.EnableInput(ProjectColombo.GameInputSystem.InputActionType.Pause);
            gameInputSO.ResetPausePressed();

            if (enableDebugLogs)
            {
                Debug.Log("[UIManager] Re-enabled pause input after delay");
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
        MenuController menu = GetMenu<OptionsMenuController>();
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

    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}