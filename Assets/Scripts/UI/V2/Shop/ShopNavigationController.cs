using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement;
using ProjectColombo.UI;


namespace ProjectColombo.Shop
{
    public class ShopNavigationController : MonoBehaviour
    {
        [Header("Shop References")]
        [SerializeField] GameObject hookButton;
        [SerializeField] Button[] shopButtons;
        [SerializeField] bool enableDebugLogs = true;

        [Header("Input Settings")]
        [SerializeField] float navigationCooldown = 0.2f;
        [SerializeField] GameInputSO gameInput;

        ShopKeeper shopKeeper;
       
        UINavigationManager navigationManager;
        UIInputSwitcher inputSwitcher;

        float lastNavigationTime;
        bool isActive = false;
        GameObject lastValidSelection;

        #region Unity Lifecycle

        void Awake()
        {
            FindReferences();
            InitializeNavigation();
        }

        void Start()
        {
            SetupInputSwitcher();
        }

        void OnEnable()
        {
            if (hookButton != null && hookButton.activeInHierarchy)
            {
                StartCoroutine(DelayedInitialSelection());
            }
        }

        void Update()
        {
            if (!isActive)
            {
                return;
            }

            HandleCancelInput();
            EnsureControllerNavigation();
        }

        #endregion

        #region Initialization

        void FindReferences()
        {
            if (shopKeeper == null)
            {
                shopKeeper = FindFirstObjectByType<ShopKeeper>();
                if (shopKeeper == null)
                {
                    LogError("ShopKeeper not found! Shop navigation will not work properly.");
                }
            }

            if (gameInput == null)
            {
                gameInput = GameManager.Instance?.gameInput;
                if (gameInput == null)
                {
                    LogError("GameInputSO not found! Input handling will not work.");
                }
            }

            if (navigationManager == null)
            {
                navigationManager = FindFirstObjectByType<UINavigationManager>();
                LogDebug($"UINavigationManager found: {navigationManager != null}");
            }

            if (inputSwitcher == null)
            {
                inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                LogDebug($"UIInputSwitcher found: {inputSwitcher != null}");
            }

            LogDebug("References initialized");
        }

        void InitializeNavigation()
        {
            if (hookButton == null)
            {
                FindHookButton();
            }

            if (shopButtons == null || shopButtons.Length == 0)
            {
                FindShopButtons();
            }

            LogDebug($"Navigation initialized - Hook button: {hookButton != null}, Shop buttons: {shopButtons?.Length ?? 0}");
        }

        void FindHookButton()
        {
            Button[] allButtons = GetComponentsInChildren<Button>(true);

            foreach (Button button in allButtons)
            {
                if (button.name.ToLower().Contains("exit") ||
                    button.name.ToLower().Contains("close") ||
                    button.name.ToLower().Contains("hook"))
                {
                    hookButton = button.gameObject;
                    LogDebug($"Found hook button: {hookButton.name}");
                    break;
                }
            }

            if (hookButton == null && allButtons.Length > 0)
            {
                hookButton = allButtons[0].gameObject;
                LogDebug($"Using first button as hook: {hookButton.name}");
            }
        }

        void FindShopButtons()
        {
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            shopButtons = allButtons;
            LogDebug($"Found {shopButtons.Length} shop buttons");
        }

        void SetupInputSwitcher()
        {
            if (inputSwitcher != null && hookButton != null)
            {
                inputSwitcher.SetFirstSelectedButton(hookButton);
                LogDebug("Set hook button as first selectable for input switcher");
            }
        }

        #endregion

        #region Public Methods

        public void ActivateShopNavigation()
        {
            LogDebug("Activating shop navigation");

            isActive = true;

            EnsureHookButtonActive();

            StartCoroutine(DelayedInitialSelection());
        }

        public void DeactivateShopNavigation()
        {
            LogDebug("Deactivating shop navigation");

            isActive = false;
        }

        public void ForceHookSelection()
        {
            if (hookButton != null && hookButton.activeInHierarchy)
            {
                EventSystem eventSystem = EventSystem.current;
                if (eventSystem != null)
                {
                    eventSystem.SetSelectedGameObject(hookButton);
                    lastValidSelection = hookButton;
                    lastNavigationTime = Time.unscaledTime;

                    if (inputSwitcher != null)
                    {
                        inputSwitcher.ForceSelectButton(hookButton);
                    }

                    LogDebug("Forced selection to hook button");
                }
            }
        }

        #endregion

        #region Private Methods

        void EnsureHookButtonActive()
        {
            if (hookButton != null)
            {
                Button hookButtonComponent = hookButton.GetComponent<Button>();

                if (hookButtonComponent != null)
                {
                    hookButtonComponent.interactable = true;
                }

                hookButton.SetActive(true);
                LogDebug("Hook button ensured active and interactable");
            }
        }

        void HandleCancelInput()
        {
            bool cancelPressed = false;

            if (gameInput != null && gameInput.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                cancelPressed = true;
                LogDebug("Cancel input detected via UI.Cancel");
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                cancelPressed = true;
                LogDebug("Cancel input detected via Gamepad East button");
            }

            if (cancelPressed)
            {
                CloseShop();
            }
        }

        void EnsureControllerNavigation()
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem != null &&
                eventSystem.currentSelectedGameObject == null &&
                Time.unscaledTime - lastNavigationTime > navigationCooldown)
            {
                ForceHookSelection();
            }
            else if (eventSystem != null &&eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                lastValidSelection = eventSystem.currentSelectedGameObject;
            }
        }

        void CloseShop()
        {
            LogDebug("Closing shop via navigation controller");

            if (shopKeeper != null)
            {
                shopKeeper.CloseShop();
            }
            else
            {
                LogError("Cannot close shop - ShopKeeper reference is null!");
            }
        }

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            if (hookButton != null && hookButton.activeInHierarchy)
            {
                EventSystem eventSystem = EventSystem.current;
                if (eventSystem != null)
                {
                    eventSystem.SetSelectedGameObject(hookButton);
                    lastValidSelection = hookButton;
                    lastNavigationTime = Time.unscaledTime;

                    if (inputSwitcher != null)
                    {
                        inputSwitcher.SetFirstSelectedButton(hookButton);
                        inputSwitcher.ForceSelectButton(hookButton);
                    }

                    LogDebug("Set initial selection to hook button");
                }
            }
            else
            {
                LogWarning("Hook button is not available for initial selection");
            }
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FFAA>[ShopNavigationController] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[ShopNavigationController] {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"[ShopNavigationController] {message}");
        }

        #endregion
    }
}