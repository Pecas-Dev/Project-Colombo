using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using ProjectColombo.GameInputSystem;
using ProjectColombo.UI.MaskSelection;


namespace ProjectColombo.UI
{
    public class MaskSelectionManager : MonoBehaviour
    {
        [Header("Navigation References")]
        [SerializeField] Button defaultButton;
        [SerializeField] MaskSelectionNavigationController navigationController;
        [SerializeField] GameInputSO gameInputSO;

        [Header("Screen References")]
        [SerializeField] MaskCanvas maskCanvas;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        UINavigationManager uiNavigationManager;

        void Awake()
        {
            FindReferences();
        }

        void Start()
        {
            InitializeScreen();
        }

        void OnEnable()
        {
            StartCoroutine(DelayedInitialization());
        }

        void FindReferences()
        {
            if (navigationController == null)
            {
                navigationController = GetComponent<MaskSelectionNavigationController>();
                if (navigationController == null)
                {
                    navigationController = FindFirstObjectByType<MaskSelectionNavigationController>();
                }
            }

            if (maskCanvas == null)
            {
                maskCanvas = GetComponent<MaskCanvas>();
                if (maskCanvas == null)
                {
                    maskCanvas = FindFirstObjectByType<MaskCanvas>();
                }
            }

            if (gameInputSO == null)
            {
                gameInputSO = FindFirstObjectByType<GameInputSO>();
            }

            uiNavigationManager = FindFirstObjectByType<UINavigationManager>();

            if (defaultButton == null)
            {
                Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (Button button in buttons)
                {
                    MaskButton maskButton = button.GetComponent<MaskButton>();
                    if (maskButton != null)
                    {
                        defaultButton = button;
                        break;
                    }
                }
            }

            LogDebug("References found and assigned");
        }

        void InitializeScreen()
        {
            if (gameInputSO != null)
            {
                gameInputSO.SwitchToUI();
                LogDebug("Switched to UI input");
            }

            if (navigationController != null)
            {
                navigationController.Initialize();
                LogDebug("Navigation controller initialized");
            }

            if (uiNavigationManager != null && defaultButton != null)
            {
                uiNavigationManager.RegisterFirstSelectable(UINavigationState.MaskSelection, defaultButton.gameObject);
                uiNavigationManager.SetNavigationState(UINavigationState.MaskSelection);
                LogDebug("Registered with UI navigation manager");
            }

            if (maskCanvas != null)
            {
                maskCanvas.Show();
                LogDebug("Mask canvas shown");
            }

            LogDebug("Mask selection screen initialized");
        }

        IEnumerator DelayedInitialization()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            SelectDefaultButton();
        }

        public void SelectDefaultButton()
        {
            if (defaultButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                StartCoroutine(SelectButtonNextFrame());
                LogDebug($"Selecting default button: {defaultButton.name}");
            }
            else
            {
                if (defaultButton == null)
                {
                    LogWarning("Default button reference is missing!");
                }

                if (EventSystem.current == null)
                {
                    LogWarning("No EventSystem found in the scene!");
                }
            }
        }

        IEnumerator SelectButtonNextFrame()
        {
            yield return null;

            if (EventSystem.current != null && defaultButton != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);

                UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                if (inputSwitcher != null)
                {
                    inputSwitcher.SetFirstSelectedButton(defaultButton.gameObject);
                    inputSwitcher.ForceSelectButton(defaultButton.gameObject);
                }

                LogDebug($"Default button selected: {defaultButton.name}");
            }
        }

        public void SetDefaultButton(Button newDefaultButton)
        {
            defaultButton = newDefaultButton;
            SelectDefaultButton();
            LogDebug($"Default button set to: {newDefaultButton?.name ?? "null"}");
        }

        public void RefreshNavigation()
        {
            if (navigationController != null)
            {
                navigationController.Initialize();
            }

            MaskSelectionNavigationExtension navExtension = GetComponent<MaskSelectionNavigationExtension>();
            if (navExtension != null)
            {
                navExtension.RefreshNavigation();
            }

            LogDebug("Navigation system refreshed");
        }

        void OnDisable()
        {
            if (uiNavigationManager != null)
            {
                uiNavigationManager.SetNavigationState(UINavigationState.None);
            }

            LogDebug("Mask selection screen disabled");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF88>[MaskSelectionManager] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[MaskSelectionManager] {message}");
        }
    }
}