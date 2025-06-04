using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace ProjectColombo.UI.MaskSelection
{
    public class MaskSelectionNavigationExtension : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] GameObject firstSelectedObject;
        [SerializeField] bool autoRegisterOnEnable = true;
        [SerializeField] float registrationDelay = 0.1f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        MaskSelectionNavigationController maskNavigationController;
        UINavigationManager navigationManager;

        #region Unity Lifecycle

        void Awake()
        {
            maskNavigationController = GetComponent<MaskSelectionNavigationController>();
            if (maskNavigationController == null)
            {
                LogError("MaskSelectionNavigationController not found on same GameObject!");
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
                StartCoroutine(DelayedRegistration());
            }
        }

        #endregion

        #region Registration

        IEnumerator DelayedRegistration()
        {
            yield return new WaitForSecondsRealtime(0.25f); 

            if (gameObject.activeInHierarchy)
            {
                RegisterWithNavigationManager();

                yield return new WaitForSecondsRealtime(0.1f);

                if (navigationManager != null && navigationManager.GetCurrentState() != UINavigationState.MaskSelection)
                {
                    LogWarning("MaskSelection state was lost - re-registering");
                    RegisterWithNavigationManager();
                }
            }
        }

        public void RegisterWithNavigationManager()
        {
            FindFirstSelectableIfNeeded();

            if (firstSelectedObject == null)
            {
                LogWarning("No selectable object found to register for mask selection!");
                return;
            }

            if (navigationManager != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.MaskSelection, firstSelectedObject);
                navigationManager.SetNavigationState(UINavigationState.MaskSelection);

                LogDebug($"Registered mask selection navigation with MaskSelection state and first selectable: {firstSelectedObject.name}");
            }
            else
            {
                UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;

                if (eventSystem != null && firstSelectedObject != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectedObject);
                    LogDebug("Used EventSystem fallback for mask selection navigation");
                }
            }

            if (maskNavigationController != null)
            {
                maskNavigationController.ActivateNavigation();
            }
        }

        public void UnregisterFromNavigationManager()
        {
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
            }

            if (maskNavigationController != null)
            {
                maskNavigationController.DeactivateNavigation();
            }

            LogDebug("Unregistered mask selection navigation and reset state to None");
        }

        #endregion

        #region Public Methods

        public void SetFirstSelectable(GameObject selectable)
        {
            firstSelectedObject = selectable;
            LogDebug($"Set first selectable to: {selectable?.name ?? "null"}");
        }

        public void RefreshNavigation()
        {
            if (maskNavigationController != null)
            {
                maskNavigationController.Initialize();
                LogDebug("Refreshed mask selection navigation");
            }
        }

        #endregion

        #region Private Methods

        void FindFirstSelectableIfNeeded()
        {
            if (firstSelectedObject == null)
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);

                Transform maskContainer = transform.Find("Mask");
                if (maskContainer == null)
                {
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        maskContainer = canvas.transform.Find("Mask");
                    }
                }

                if (maskContainer != null)
                {
                    Button[] maskButtons = maskContainer.GetComponentsInChildren<Button>(true);
                    if (maskButtons.Length > 0)
                    {
                        buttons = maskButtons;
                        LogDebug($"Found {buttons.Length} buttons in Mask container");
                    }
                }

                foreach (Button button in buttons)
                {
                    string buttonName = button.name.ToLower();
                    if (buttonName.Contains("button 1") ||
                        buttonName.Contains("first") ||
                        buttonName.Contains("mask"))
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
                    LogWarning("No selectable object found in mask selection hierarchy!");
                }
            }
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#AAFF00>[MaskSelectionNavigationExtension] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[MaskSelectionNavigationExtension] {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"[MaskSelectionNavigationExtension] {message}");
        }

        #endregion
    }
}