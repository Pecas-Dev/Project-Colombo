using ProjectColombo.UI.Pausescreen;
using ProjectColombo.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseCanvasManager : MonoBehaviour
{
    [Tooltip("Global UI elements that should be hidden when not paused")]
    [SerializeField] GameObject[] globalElements;

    [Tooltip("Enable debug logging")]
    [SerializeField] bool enableDebugLogs = false;

    private void Awake()
    {
        HideGlobalElements();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideGlobalElements();
    }

    public void HideGlobalElements()
    {
        if (globalElements != null)
        {
            foreach (GameObject element in globalElements)
            {
                if (element != null)
                {
                    element.SetActive(false);

                    if (enableDebugLogs)
                    {
                        Debug.Log($"<color=#FF5500>[PauseCanvasManager] Disabled global element: {element.name}</color>");
                    }
                }
            }
        }
    }

    public void ShowGlobalElements()
    {
        if (globalElements != null)
        {
            foreach (GameObject element in globalElements)
            {
                if (element != null)
                {
                    element.SetActive(true);

                    if (enableDebugLogs)
                    {
                        Debug.Log($"<color=#FF5500>[PauseCanvasManager] Enabled global element: {element.name}</color>");
                    }
                }
            }
        }
    }

    public void HideAllTabs()
    {
        MenuController[] controllers = GetComponentsInChildren<MenuController>(true);

        foreach (MenuController controller in controllers)
        {
            if (controller != null)
            {
                controller.gameObject.SetActive(false);

                if (enableDebugLogs)
                {
                    Debug.Log($"<color=#FF5500>[PauseCanvasManager] Deactivated controller: {controller.GetType().Name}</color>");
                }
            }
        }

        PauseMenuSettingsController settingsController = GetComponentInChildren<PauseMenuSettingsController>(true);
        
        if (settingsController != null)
        {
            settingsController.gameObject.SetActive(false);

            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF5500>[PauseCanvasManager] Specifically deactivated settings controller</color>");
            }
        }

        PauseMenuInventoryController inventoryController = GetComponentInChildren<PauseMenuInventoryController>(true);
        
        if (inventoryController != null)
        {
            inventoryController.gameObject.SetActive(false);

            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF5500>[PauseCanvasManager] Specifically deactivated inventory controller</color>");
            }
        }
    }
}

