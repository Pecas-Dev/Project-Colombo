using ProjectColombo.GameManagement;
using ProjectColombo.UI;
using UnityEngine;

public class CharmSwapMenuRegister : MonoBehaviour
{
    /*[SerializeField] CharmSwapMenuController controller;
    [SerializeField] bool enableDebugLogs = true;

    static bool hasRegistered = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponentInChildren<CharmSwapMenuController>(true);

            if (controller == null && enableDebugLogs)
            {
                Debug.LogError("[CharmSwapMenuRegister] Failed to find CharmSwapMenuController in children!");
            }
        }

        if (controller != null)
        {
            controller.Initialize();
        }

        RegisterWithGameManager();
        RegisterWithUIManager();
    }

    void RegisterWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            LogDebug("Registering with GameManager...");
            LogDebug("Registration with GameManager successful");
        }
        else
        {
            LogDebug("GameManager not found, retrying...");
            Invoke("RegisterWithGameManager", 0.5f);
        }
    }

    void RegisterWithUIManager()
    {
        if (!hasRegistered && UIManager.Instance != null && controller != null)
        {
            LogDebug("Registering with UIManager...");

            var registeredController = UIManager.Instance.GetMenu<CharmSwapMenuController>();

            if (registeredController != null)
            {
                LogDebug("Controller successfully registered with UIManager");
                hasRegistered = true;
            }
            else
            {
                UIManager.Instance.RegisterMenu(controller);

                registeredController = UIManager.Instance.GetMenu<CharmSwapMenuController>();
                if (registeredController != null)
                {
                    LogDebug("Controller successfully registered with UIManager via direct registration");
                    hasRegistered = true;
                }
                else
                {
                    LogDebug("Registration failed, retrying...");
                    Invoke("RegisterWithUIManager", 0.5f);
                }
            }
        }
        else if (UIManager.Instance == null)
        {
            LogDebug("UIManager not found, retrying...");
            Invoke("RegisterWithUIManager", 0.5f);
        }
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"<color=#FFAA00>[CharmSwapMenuRegister] {message}</color>");
        }
    }*/
}