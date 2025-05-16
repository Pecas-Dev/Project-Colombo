using ProjectColombo.GameManagement;
using ProjectColombo.UI.Pausescreen;
using UnityEngine;

public class PauseMenuRegister : MonoBehaviour
{
    /*[SerializeField] PauseMenuInventoryController pauseMenuController;
    [SerializeField] bool enableDebugLogs = true;

    private static bool hasRegistered = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (pauseMenuController == null)
        {
            pauseMenuController = GetComponentInChildren<PauseMenuInventoryController>(true);

            if (pauseMenuController == null && enableDebugLogs)
            {
                Debug.LogError("[PauseMenuRegistrar] Failed to find PauseMenuInventoryController in children!");
            }
        }

        RegisterWithGameManager();

        RegisterWithUIManager();
    }

    void RegisterWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            LogDebug("Attempting to register with GameManager...");


            LogDebug("Registered with GameManager successfully");
        }
        else
        {
            LogDebug("GameManager not available for registration yet");

            Invoke("RegisterWithGameManager", 0.5f);
        }
    }

    void RegisterWithUIManager()
    {
        if (!hasRegistered && UIManager.Instance != null && pauseMenuController != null)
        {
            LogDebug("Registering PauseMenuInventoryController with UIManager...");

            string menuType = pauseMenuController.GetType().Name;

            var testGet = UIManager.Instance.GetMenu<PauseMenuInventoryController>();

            if (testGet != null)
            {
                LogDebug("PauseMenuInventoryController found and cached by UIManager!");
                hasRegistered = true;
            }
            else
            {
                LogDebug("UIManager failed to find PauseMenuInventoryController, will retry...");
                Invoke("RegisterWithUIManager", 0.5f);
            }
        }
        else if (UIManager.Instance == null)
        {
            LogDebug("UIManager not available for registration yet");
            Invoke("RegisterWithUIManager", 0.5f);
        }
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"<color=#FF5500>[PauseMenuRegistrar] {message}</color>");
        }
    }*/
}
