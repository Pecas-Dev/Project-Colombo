using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;


namespace ProjectColombo.Shop
{
    public class ShopKeeper : MonoBehaviour
    {
        [Header("Shop UI References")]
        public GameObject shopScreen;
        public GameObject shopIndicator;

        [Header("Navigation Components")]
        [SerializeField] ShopNavigationController navigationController;
        [SerializeField] ShopNavigationExtension navigationExtension;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        bool playerInRange;
        GameInputSO gameInput;

        #region Unity Lifecycle

        void Start()
        {
            InitializeReferences();
            SetupInitialState();
        }

        void Update()
        {
            HandleInteractionInput();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = true;
                shopIndicator.SetActive(true);
                other.GetComponent<PlayerStateMachine>().closeShop = this;
                LogDebug("Player entered shop range");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = false;
                shopIndicator.SetActive(false);
                other.GetComponent<PlayerStateMachine>().closeShop = null;
                LogDebug("Player exited shop range");
            }
        }

        #endregion

        #region Initialization

        void InitializeReferences()
        {
            gameInput = GameManager.Instance.gameInput;

            if (gameInput == null)
            {
                LogError("GameInputSO not found on GameManager!");
            }

            SetupNavigationComponents();

            LogDebug("References initialized");
        }

        void SetupNavigationComponents()
        {
            if (navigationController == null)
            {
                navigationController = GetComponentInChildren<ShopNavigationController>();

                if (navigationController == null && shopScreen != null)
                {
                    navigationController = shopScreen.GetComponent<ShopNavigationController>();

                    if (navigationController == null)
                    {
                        navigationController = shopScreen.AddComponent<ShopNavigationController>();
                        LogDebug("Created ShopNavigationController on shop screen");
                    }
                }
            }

            if (navigationExtension == null)
            {
                navigationExtension = GetComponentInChildren<ShopNavigationExtension>();

                if (navigationExtension == null && shopScreen != null)
                {
                    navigationExtension = shopScreen.GetComponent<ShopNavigationExtension>();

                    if (navigationExtension == null)
                    {
                        navigationExtension = shopScreen.AddComponent<ShopNavigationExtension>();
                        LogDebug("Created ShopNavigationExtension on shop screen");
                    }
                }
            }

            LogDebug($"Navigation components setup - Controller: {navigationController != null}, Extension: {navigationExtension != null}");
        }

        void SetupInitialState()
        {
            shopScreen.SetActive(false);
            shopIndicator.SetActive(false);
            LogDebug("Initial state configured");
        }

        #endregion

        #region Input Handling

        void HandleInteractionInput()
        {
            if (playerInRange && gameInput != null && gameInput.GetInputPressed(PlayerInputAction.Interact))
            {
                OpenShop();
            }
        }

        #endregion

        #region Public Shop Methods

        public void OpenShop()
        {
            LogDebug("Opening shop");

            GameManager.Instance.PauseGame(false);

            shopScreen.SetActive(true);
            shopIndicator.SetActive(false);

            ShopScreen shopScreenComponent = shopScreen.GetComponentInChildren<ShopScreen>();
            if (shopScreenComponent != null)
            {
                shopScreenComponent.SetDiscount(0f);
                StartCoroutine(shopScreenComponent.SetFirstSelected());
            }

            StartCoroutine(DelayedShopSetup());

            CustomEvents.ShopOpen(this);

            LogDebug("Shop opened and navigation setup initiated");
        }

        IEnumerator DelayedShopSetup()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.15f);

            if (navigationController != null)
            {
                navigationController.ActivateShopNavigation();
            }

            if (navigationExtension != null)
            {
                navigationExtension.RegisterWithNavigationManager();
            }

            LogDebug("Shop navigation setup completed");
        }

        public void CloseShop()
        {
            LogDebug("Closing shop");

            if (navigationController != null)
            {
                navigationController.DeactivateShopNavigation();
            }

            if (navigationExtension != null)
            {
                navigationExtension.UnregisterFromNavigationManager();
            }

            GameManager.Instance.ResumeGame();

            shopScreen.SetActive(false);
            shopIndicator.SetActive(true);

            ShopScreen shopScreenComponent = shopScreen.GetComponentInChildren<ShopScreen>();
            if (shopScreenComponent != null)
            {
                shopScreenComponent.SetDiscount(0f);
            }

            CustomEvents.ShopClose();

            LogDebug("Shop closed and navigation deactivated");
        }

        public void CloseShopScreen()
        {
            shopScreen.SetActive(false);
            LogDebug("Shop screen closed (direct method)");
        }

        public void OpenShopScreen()
        {
            GameManager.Instance.PauseGame(false);
            shopScreen.SetActive(true);

            ShopScreen shopScreenComponent = shopScreen.GetComponent<ShopScreen>();
            if (shopScreenComponent != null)
            {
                StartCoroutine(shopScreenComponent.SetFirstSelected());
            }

            LogDebug("Shop screen opened (direct method)");
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FFFF00>[ShopKeeper] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[ShopKeeper] {message}");
        }

        #endregion
    }
}