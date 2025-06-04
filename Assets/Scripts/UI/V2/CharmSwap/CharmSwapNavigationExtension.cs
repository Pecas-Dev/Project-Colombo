using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


namespace ProjectColombo.UI
{
    public class CharmSwapNavigationExtension : MonoBehaviour
    {
        [Header("Charm Buttons")]
        [SerializeField] Button[] charmButtons;
        [SerializeField] Button legendaryCharmButton;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        UINavigationManager navigationManager;
        CharmSwapMenuController charmSwapController;

        void Awake()
        {
            charmSwapController = GetComponent<CharmSwapMenuController>();

            if (charmSwapController == null)
            {
                LogError("CharmSwapMenuController not found on same GameObject!");
            }
        }

        void Start()
        {
            navigationManager = FindFirstObjectByType<UINavigationManager>();

            if (navigationManager == null)
            {
                LogError("UINavigationManager not found!");
            }

            SetupCharmButtonNavigation();
        }

        void OnEnable()
        {
            StartCoroutine(DelayedRegisterAndSetState());
        }

        public bool IsLegendaryReplacementMode()
        {
            if (charmSwapController == null)
            {
                return false;
            }

            return charmSwapController.isLegendaryReplacementMode;
        }

        IEnumerator DelayedForceLegendarySelection()
        {
            yield return null;

            if (legendaryCharmButton != null)
            {
                EventSystem.current.SetSelectedGameObject(legendaryCharmButton.gameObject);
                LogDebug("Forced selection to legendary button via navigation extension");
            }
        }

        IEnumerator DelayedRegisterAndSetState()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            if (navigationManager != null)
            {
                Button firstAvailableButton = GetFirstAvailableButton();
                if (firstAvailableButton != null)
                {
                    navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, firstAvailableButton.gameObject);
                    navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
                    LogDebug($"Registered first selectable and set state: {firstAvailableButton.name}");

                    EventSystem.current.SetSelectedGameObject(firstAvailableButton.gameObject);
                    LogDebug($"Set initial selection to: {firstAvailableButton.name}");
                }

                yield return new WaitForSecondsRealtime(0.1f);

                if (navigationManager.GetCurrentState() != UINavigationState.CharmSwapScreen)
                {
                    LogDebug("CharmSwapScreen state was lost - re-setting");
                    navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
                }
            }
            else
            {
                LogError("NavigationManager is null in DelayedRegisterAndSetState!");
            }
        }

        public void EnsureNavigationState()
        {
            if (navigationManager != null)
            {
                Button firstAvailableButton = GetFirstAvailableButton();
                if (firstAvailableButton != null)
                {
                    navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, firstAvailableButton.gameObject);
                }
                navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
                LogDebug("Ensured CharmSwapScreen navigation state is set");
            }
        }

        void SetupCharmButtonNavigation()
        {
            if (charmButtons == null || charmButtons.Length == 0)
            {
                LogError("Charm buttons array is null or empty!");
                return;
            }

            LogDebug("Setting up charm button navigation with proper slot mapping...");

            for (int i = 0; i < charmButtons.Length && i < 4; i++)
            {
                if (charmButtons[i] == null)
                {
                    continue;
                }

                Navigation nav = charmButtons[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = null;
                nav.selectOnDown = null;

                switch (i)
                {
                    case 0: // SlotCharm_2 (array index 0, slot index 1)
                        nav.selectOnLeft = legendaryCharmButton; // SlotCharm_1 (Legendary)
                        nav.selectOnRight = (charmButtons.Length > 1) ? charmButtons[1] : legendaryCharmButton; // SlotCharm_3 or wrap to legendary
                        break;

                    case 1: // SlotCharm_3 (array index 1, slot index 2)
                        nav.selectOnLeft = charmButtons[0]; // SlotCharm_2
                        nav.selectOnRight = (charmButtons.Length > 2) ? charmButtons[2] : legendaryCharmButton; // SlotCharm_4 or wrap to legendary
                        break;

                    case 2: // SlotCharm_4 (array index 2, slot index 3)
                        nav.selectOnLeft = charmButtons[1]; // SlotCharm_3
                        nav.selectOnRight = (charmButtons.Length > 3) ? charmButtons[3] : legendaryCharmButton; // SlotCharm_5 or wrap to legendary
                        break;

                    case 3: // SlotCharm_5 (array index 3, slot index 4)
                        nav.selectOnLeft = charmButtons[2]; // SlotCharm_4
                        nav.selectOnRight = legendaryCharmButton; // SlotCharm_1 (Legendary)
                        break;
                }

                charmButtons[i].navigation = nav;
                LogDebug($"Setup navigation for charm button {i} (SlotCharm_{i + 2})");
            }

            if (legendaryCharmButton != null)
            {
                Navigation legendaryNav = legendaryCharmButton.navigation;
                legendaryNav.mode = Navigation.Mode.Explicit;
                legendaryNav.selectOnUp = null;
                legendaryNav.selectOnDown = null;

                if (charmButtons.Length >= 4)
                {
                    legendaryNav.selectOnLeft = charmButtons[3];
                    legendaryNav.selectOnRight = charmButtons[0];
                }
                else if (charmButtons.Length > 0)
                {
                    legendaryNav.selectOnLeft = charmButtons[charmButtons.Length - 1];
                    legendaryNav.selectOnRight = charmButtons[0];
                }
                else
                {
                    legendaryNav.selectOnLeft = null;
                    legendaryNav.selectOnRight = null;
                }

                legendaryCharmButton.navigation = legendaryNav;
                LogDebug("Setup navigation for legendary button (SlotCharm_1)");
            }

            bool legendaryMode = IsLegendaryReplacementMode();
            LogDebug($"Charm button navigation setup completed (Legendary mode: {legendaryMode})");
        }

        public void RefreshNavigationForMode()
        {
            SetupCharmButtonNavigation();
            LogDebug("Navigation refreshed for current mode");
        }

        Button GetFirstAvailableButton()
        {
            if (legendaryCharmButton != null && legendaryCharmButton.interactable && legendaryCharmButton.gameObject.activeInHierarchy)
            {
                LogDebug("Returning legendary button as first available (SlotCharm_1)");
                return legendaryCharmButton;
            }

            if (charmButtons != null)
            {
                for (int i = 0; i < charmButtons.Length; i++)
                {
                    if (charmButtons[i] != null && charmButtons[i].interactable && charmButtons[i].gameObject.activeInHierarchy)
                    {
                        LogDebug($"Returning regular button {i} as first available (SlotCharm_{i + 2})");
                        return charmButtons[i];
                    }
                }
            }

            LogDebug("No available buttons found");
            return null;
        }

        public void RefreshNavigation()
        {
            SetupCharmButtonNavigation();
            LogDebug("Navigation refreshed");
        }

        public void OnCharmSwapActivated()
        {
            Button firstButton = GetFirstAvailableButton();
            if (firstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
                LogDebug($"Selected first available button: {firstButton.name} (Legendary mode: {IsLegendaryReplacementMode()})");

                if (IsLegendaryReplacementMode() && firstButton == legendaryCharmButton)
                {
                    LogDebug("Confirmed legendary button selection in legendary mode");
                }
            }
        }

        public void OnCharmSwapDeactivated()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            LogDebug("Charm swap navigation deactivated");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF9900>[CharmSwapNavigationExtension] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[CharmSwapNavigationExtension] {message}");
        }
    }
}