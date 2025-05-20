using UnityEngine;
using UnityEngine.UI;


namespace ProjectColombo.UI
{

    public class OptionsMenuNavigationExtension : MonoBehaviour
    {
        [Header("Tab References")]
        [SerializeField] GameObject graphicsTab;
        [SerializeField] GameObject audioTab;
        [SerializeField] GameObject controlsTab;

        [Header("First Selectable Objects")]
        [SerializeField] GameObject graphicsFirstSelected;
        [SerializeField] GameObject audioFirstSelected;
        [SerializeField] GameObject controlsFirstSelected;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        OptionsMenuController optionsMenuController;
        UINavigationManager navigationManager;

        int currentTabIndex = 0;

        UINavigationState[] tabStates = new UINavigationState[]
       {
            UINavigationState.OptionsMenuGraphicsTab,
            UINavigationState.OptionsMenuAudioTab,
            UINavigationState.OptionsMenuControlsTab
       };

        void Awake()
        {
            optionsMenuController = GetComponent<OptionsMenuController>();

            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                GameObject navigationManagerObj = new GameObject("UINavigationManager");
                navigationManager = navigationManagerObj.AddComponent<UINavigationManager>();
                LogDebug("Created UINavigationManager");
            }

            if (graphicsTab == null || audioTab == null || controlsTab == null)
            {
                Transform tabsTransform = transform.Find("Tabs");
                if (tabsTransform != null)
                {
                    if (graphicsTab == null)
                    {
                        graphicsTab = tabsTransform.Find("GraphicsTab")?.gameObject;
                    }

                    if (audioTab == null)
                    {
                        audioTab = tabsTransform.Find("AudioTab")?.gameObject;
                    }

                    if (controlsTab == null)
                    {
                        controlsTab = tabsTransform.Find("ControlsTab")?.gameObject;
                    }
                }
            }
        }

        void Start()
        {
            FindFirstSelectablesIfNeeded();
            RegisterSelectables();
        }

        void OnEnable()
        {
            FindFirstSelectablesIfNeeded();
            RegisterSelectables();

            UpdateNavigationState();
        }

        void FindFirstSelectablesIfNeeded()
        {
            if (graphicsFirstSelected == null && graphicsTab != null)
            {
                Button[] buttons = graphicsTab.GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0)
                {
                    graphicsFirstSelected = buttons[0].gameObject;
                    LogDebug($"Found graphics first selectable: {graphicsFirstSelected.name}");
                }
            }

            if (audioFirstSelected == null && audioTab != null)
            {
                Button[] buttons = audioTab.GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0)
                {
                    audioFirstSelected = buttons[0].gameObject;
                    LogDebug($"Found audio first selectable: {audioFirstSelected.name}");
                }
            }

            if (controlsFirstSelected == null && controlsTab != null)
            {
                Button[] buttons = controlsTab.GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0)
                {
                    controlsFirstSelected = buttons[0].gameObject;
                    LogDebug($"Found controls first selectable: {controlsFirstSelected.name}");
                }
            }
        }

        void RegisterSelectables()
        {
            if (navigationManager == null)
            {
                return;
            }

            if (graphicsFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.OptionsMenuGraphicsTab, graphicsFirstSelected);
            }

            if (audioFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.OptionsMenuAudioTab, audioFirstSelected);
            }

            if (controlsFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.OptionsMenuControlsTab, controlsFirstSelected);
            }
        }

        void UpdateNavigationState()
        {
            if (navigationManager == null)
            {
                return;
            }

            if (graphicsTab != null && graphicsTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.OptionsMenuGraphicsTab);
                currentTabIndex = 0;
            }
            else if (audioTab != null && audioTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.OptionsMenuAudioTab);
                currentTabIndex = 1;
            }
            else if (controlsTab != null && controlsTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.OptionsMenuControlsTab);
                currentTabIndex = 2;
            }
        }

        public void OnTabChanged(int tabIndex)
        {
            if (navigationManager == null || tabIndex < 0 || tabIndex >= tabStates.Length)
                return;

            currentTabIndex = tabIndex;
            navigationManager.SetNavigationState(tabStates[tabIndex]);

            LogDebug($"Switched to tab {tabIndex} with state {tabStates[tabIndex]}");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FFAA00>[OptionsMenuNavigationExtension] {message}</color>");
            }
        }
    }
}