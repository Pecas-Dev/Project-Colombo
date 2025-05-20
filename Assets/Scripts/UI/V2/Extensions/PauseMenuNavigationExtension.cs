using UnityEngine;
using UnityEngine.UI;


namespace ProjectColombo.UI.Pausescreen
{

    public class PauseMenuNavigationExtension : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] GameObject inventoryFirstSelected;
        [SerializeField] GameObject statsFirstSelected;
        [SerializeField] GameObject settingsFirstSelected;

        [Header("Tab References")]
        [SerializeField] GameObject inventoryTab;
        [SerializeField] GameObject statsTab;
        [SerializeField] GameObject settingsTab;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        PauseMenuManager pauseMenuManager;
        UINavigationManager navigationManager;

        int currentTabIndex = 0;

        UINavigationState[] tabStates = new UINavigationState[]
       {
            UINavigationState.PauseInventoryTab,
            UINavigationState.PauseStatsTab,
            UINavigationState.PauseSettingsTab
       };

         void Awake()
        {
            pauseMenuManager = GetComponent<PauseMenuManager>();

            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                GameObject navigationManagerObj = new GameObject("UINavigationManager");
                navigationManager = navigationManagerObj.AddComponent<UINavigationManager>();
                LogDebug("Created UINavigationManager");
            }

            if (inventoryTab == null)
            {
                inventoryTab = transform.Find("InventoryTab")?.gameObject ?? transform.Find("PauseInventory")?.gameObject;

            }

            if (statsTab == null)
            {
                statsTab = transform.Find("StatsTab")?.gameObject ?? transform.Find("PauseStats")?.gameObject;

            }

            if (settingsTab == null)
            {
                settingsTab = transform.Find("SettingsTab")?.gameObject ?? transform.Find("PauseSettings")?.gameObject;

            }
        }

        void Start()
        {
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
            if (inventoryFirstSelected == null && inventoryTab != null)
            {
                Button[] buttons = inventoryTab.GetComponentsInChildren<Button>(true);
                if (buttons.Length > 0)
                {
                    inventoryFirstSelected = buttons[0].gameObject;
                    LogDebug($"Found inventory first selectable: {inventoryFirstSelected.name}");
                }
            }


            if (settingsFirstSelected == null && settingsTab != null)
            {
                Button[] buttons = settingsTab.GetComponentsInChildren<Button>(true);

                if (buttons.Length > 0)
                {
                    settingsFirstSelected = buttons[0].gameObject;
                    LogDebug($"Found settings first selectable: {settingsFirstSelected.name}");
                }
            }
        }

        void RegisterSelectables()
        {
            if (navigationManager == null)
            {
                return;
            }

            if (inventoryFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.PauseInventoryTab, inventoryFirstSelected);
            }

            if (statsFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.PauseStatsTab, statsFirstSelected);
            }

            if (settingsFirstSelected != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.PauseSettingsTab, settingsFirstSelected);
            }
        }

        void UpdateNavigationState()
        {
            if (navigationManager == null)
            {
                return;
            }

            if (inventoryTab != null && inventoryTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseInventoryTab);
                currentTabIndex = 0;
            }
            else if (statsTab != null && statsTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseStatsTab);
                currentTabIndex = 1;
            }
            else if (settingsTab != null && settingsTab.activeInHierarchy)
            {
                navigationManager.SetNavigationState(UINavigationState.PauseSettingsTab);
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
                Debug.Log($"<color=#FF00FF>[PauseMenuNavigationExtension] {message}</color>");
            }
        }
    }
}