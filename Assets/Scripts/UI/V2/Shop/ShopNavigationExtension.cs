using UnityEngine;
using UnityEngine.UI;
using ProjectColombo.UI;
using System.Collections;


namespace ProjectColombo.Shop
{
    public class ShopNavigationExtension : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] GameObject firstSelectedObject;
        [SerializeField] bool autoRegisterOnEnable = true;
        [SerializeField] float registrationDelay = 0.1f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        ShopNavigationController shopNavigationController;
        UINavigationManager navigationManager;

        #region Unity Lifecycle

        void Awake()
        {
            shopNavigationController = GetComponent<ShopNavigationController>();
            if (shopNavigationController == null)
            {
                LogError("ShopNavigationController not found on same GameObject!");
            }

            FindFirstSelectableIfNeeded();
        }

        void Start()
        {
            navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager == null)
            {
                LogWarning("UINavigationManager not found - creating basic navigation fallback");
            }
        }

        void OnEnable()
        {
            if (autoRegisterOnEnable)
            {
                Invoke("RegisterWithNavigationManager", registrationDelay);
            }
        }

        #endregion

        #region Public Methods

        public void RegisterWithNavigationManager()
        {
            StartCoroutine(DelayedRegistration());
        }

        IEnumerator DelayedRegistration()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            FindFirstSelectableIfNeeded();

            if (firstSelectedObject == null)
            {
                LogWarning("No selectable object found to register for shop!");
                yield break;
            }

            if (navigationManager != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.Shop, firstSelectedObject);

                yield return null;

                navigationManager.SetNavigationState(UINavigationState.Shop);

                LogDebug($"Registered shop navigation with Shop state and first selectable: {firstSelectedObject.name}");
            }
            else
            {
                UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
                if (eventSystem != null && firstSelectedObject != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectedObject);
                    LogDebug("Used EventSystem fallback for shop navigation");
                }
            }

            if (shopNavigationController != null)
            {
                shopNavigationController.ActivateShopNavigation();
            }
        }

        public void UnregisterFromNavigationManager()
        {
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
            }

            if (shopNavigationController != null)
            {
                shopNavigationController.DeactivateShopNavigation();
            }

            LogDebug("Unregistered shop navigation and reset state to None");
        }

        public void SetFirstSelectable(GameObject selectable)
        {
            firstSelectedObject = selectable;
            LogDebug($"Set first selectable to: {selectable?.name ?? "null"}");
        }

        #endregion

        #region Private Methods

        void FindFirstSelectableIfNeeded()
        {
            if (firstSelectedObject == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);

                foreach (Button button in buttons)
                {
                    string buttonName = button.name.ToLower();
                    if (buttonName.Contains("exit") ||
                        buttonName.Contains("close") ||
                        buttonName.Contains("hook") ||
                        buttonName.Contains("first"))
                    {
                        firstSelectedObject = button.gameObject;
                        LogDebug($"Found first selectable by name priority: {firstSelectedObject.name}");
                        return;
                    }
                }

                foreach (Button button in buttons)
                {
                    if (button.gameObject.activeInHierarchy && button.interactable)
                    {
                        firstSelectedObject = button.gameObject;
                        LogDebug($"Found first active selectable: {firstSelectedObject.name}");
                        return;
                    }
                }

                if (buttons.Length > 0)
                {
                    firstSelectedObject = buttons[0].gameObject;
                    LogDebug($"Using first button as selectable: {firstSelectedObject.name}");
                }

                if (firstSelectedObject == null)
                {
                    LogWarning("No selectable object found in shop hierarchy!");
                }
            }
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#AAFF00>[ShopNavigationExtension] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[ShopNavigationExtension] {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"[ShopNavigationExtension] {message}");
        }

        #endregion
    }
}