using ProjectColombo.GameInputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using ProjectColombo.UI;
using UnityEngine;


public class RadialMenuManager : MonoBehaviour
{
    [Header("Scenes where RadialMenu is active")]
    [SerializeField]
    string[] activeScenes = new string[]
    {
        "02_LevelOne",
        "03_LevelTwo",
        "04_LevelThree",
        "05_Church"
    };

    [Header("RadialMenu Prefab")]
    [SerializeField] GameObject radialMenuCanvasPrefab;

    [Header("Input Settings")]
    [SerializeField] float holdTimeToActivate = 0.5f;

    [Header("Game Input")]
    [SerializeField] GameInputSO gameInputSO;

    RadialMenuController radialMenuController;
    GameObject instantiatedRadialMenu;

    bool isActivating = false;
    bool isRadialMenuActive = false;
    bool wasButtonPressed = false;
    float holdStartTime;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instantiatedRadialMenu != null)
        {
            Destroy(instantiatedRadialMenu);
            instantiatedRadialMenu = null;
            radialMenuController = null;
        }

        if (ShouldActivateRadialMenuInScene(scene.name))
        {
            SpawnRadialMenu();
        }
    }

    bool ShouldActivateRadialMenuInScene(string sceneName)
    {
        foreach (string activeScene in activeScenes)
        {
            if (sceneName == activeScene)
            {
                return true;
            }
        }

        return false;
    }

    void SpawnRadialMenu()
    {
        if (radialMenuCanvasPrefab != null)
        {
            instantiatedRadialMenu = Instantiate(radialMenuCanvasPrefab);

            radialMenuController = instantiatedRadialMenu.GetComponentInChildren<RadialMenuController>();

            if (radialMenuController != null)
            {
                radialMenuController.Initialize();
                radialMenuController.Hide();
            }
            else
            {
                Debug.LogError("RadialMenuManager: RadialMenuCanvasPrefab must have a RadialMenuControllerV2 component either on the canvas or on a child object!");
            }
        }
        else
        {
            Debug.LogError("RadialMenuManager: RadialMenuCanvasPrefab is not assigned!");
        }
    }

    void Update()
    {
        if (radialMenuController != null && ShouldActivateRadialMenuInScene(SceneManager.GetActiveScene().name))
        {
            if (UIManager.Instance != null && UIManager.Instance.GetCurrentMenu() == null)
            {
                HandleActivationInput();
            }
        }

        if (isRadialMenuActive && radialMenuController != null)
        {
            radialMenuController.HandleInput();
        }
    }

    void HandleActivationInput()
    {
        if (gameInputSO != null && gameInputSO.playerInputActions != null)
        {
            if (gameInputSO.playerInputActions.Player.ActivateRadial.IsPressed())
            {
                if (!isRadialMenuActive)
                {
                    if (!wasButtonPressed)
                    {
                        wasButtonPressed = true;
                        isActivating = true;
                        holdStartTime = Time.time;
                    }

                    if (isActivating)
                    {
                        float currentTime = Time.time;
                        float timeHeld = currentTime - holdStartTime;

                        if (timeHeld >= holdTimeToActivate)
                        {
                            ActivateRadialMenu();
                        }
                    }
                }
            }
            else if (gameInputSO.playerInputActions.Player.ActivateRadial.WasReleasedThisFrame())
            {
                wasButtonPressed = false;
                isActivating = false;

                if (isRadialMenuActive)
                {
                    DeactivateRadialMenu();
                }
            }
        }
    }

    void ActivateRadialMenu()
    {
        isRadialMenuActive = true;
        isActivating = false;

        if (radialMenuController != null)
        {
            radialMenuController.Show();

            StartCoroutine(WatchForButtonRelease());
        }
    }

    IEnumerator WatchForButtonRelease()
    {
        yield return null;

        while (isRadialMenuActive)
        {
            if (gameInputSO != null && gameInputSO.playerInputActions != null)
            {
                var control = gameInputSO.playerInputActions.Player.ActivateRadial.controls[0];

                if (!control.IsPressed())
                {
                    DeactivateRadialMenu();
                    yield break;
                }
            }

            yield return null;
        }
    }

    void DeactivateRadialMenu()
    {
        isRadialMenuActive = false;
        isActivating = false;
        wasButtonPressed = false; // Reset this to ensure we can detect the next press

        if (radialMenuController != null)
        {
            if (radialMenuController.radialMenuShader != null)
            {
                int selectedSection = radialMenuController.radialMenuShader.CurrentSelectedSection;

                if (selectedSection != -1)
                {
                    Debug.Log($"Selected Section: {selectedSection}");
                }
            }

            radialMenuController.Hide();
            
            if (gameInputSO != null)
            {
                gameInputSO.DisableUIMode();
            }
        }

        // Add a small delay before we can activate again to prevent accidental reactivation
        StartCoroutine(ResetButtonStateAfterDelay(0.1f));
    }

    IEnumerator ResetButtonStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // If the button is still being held down after our delay, 
        // we need to wait until it's fully released before allowing reactivation
        if (gameInputSO != null && gameInputSO.playerInputActions != null)
        {
            var control = gameInputSO.playerInputActions.Player.ActivateRadial.controls[0];
            
            // Wait until the button is fully released
            while (control.IsPressed())
            {
                yield return null;
            }
        }
    }
}