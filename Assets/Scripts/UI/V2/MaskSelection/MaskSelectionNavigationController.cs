using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using ProjectColombo.GameInputSystem;

namespace ProjectColombo.UI.MaskSelection
{
    public class MaskSelectionNavigationController : MenuController
    {
        [Header("Mask Selection References")]
        [SerializeField] Button[] maskButtons;
        [SerializeField] GameObject firstSelectedButton;
        [SerializeField] GameObject moreInfoPanel;

        [Header("Navigation Settings")]
        [SerializeField] bool enableHorizontalWrapping = true;
        [SerializeField] float rawInputThreshold = 0.5f;
        [SerializeField] float rawInputCooldown = 0.3f;

        [Header("MoreInfo Settings")]
        [SerializeField] float moreInfoActivationDelay = 0.4f;

        [Header("Mouse Settings")]
        [SerializeField] bool ignoreMouseHover = true;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        int currentSelectedIndex = 0;

        float lastRawInputTime;
        float lastMoreInfoActivationTime;
        float lastSubmitTime;

        bool isActive = false;
        bool isMoreInfoActive = false;

        GameObject lastSelectedGameObject;

        UINavigationManager navigationManager;
        MaskSelectionNavigationExtension navigationExtension;

        #region Unity Lifecycle

        void Awake()
        {
            FindMaskButtons();
            Initialize();
        }

        void Start()
        {
            navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager == null)
            {
                LogWarning("UINavigationManager not found!");
            }

            navigationExtension = GetComponent<MaskSelectionNavigationExtension>();
            if (navigationExtension == null)
            {
                navigationExtension = gameObject.AddComponent<MaskSelectionNavigationExtension>();
                LogDebug("Added MaskSelectionNavigationExtension component");
            }
        }

        void OnEnable()
        {
            ActivateNavigation();
        }

        void OnDisable()
        {
            DeactivateNavigation();
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive)
            {
                return;
            }

            HandleSelectionChange();
            HandleSubmitInput();
            HandleCancelInput();
            EnsureValidSelection();
        }

        #endregion

        #region MoreInfo Panel Management

        void ToggleMoreInfo()
        {
            if (isMoreInfoActive)
            {
                HideMoreInfo();
            }
            else
            {
                ShowMoreInfo();
            }
        }

        void ShowMoreInfo()
        {
            if (moreInfoPanel == null)
            {
                LogWarning("MoreInfo panel is not assigned!");
                return;
            }

            moreInfoPanel.SetActive(true);
            isMoreInfoActive = true;

            UpdateMoreInfoDisplay();

            LogDebug("MoreInfo panel activated");
        }

        void HideMoreInfo()
        {
            if (moreInfoPanel == null)
            {
                return;
            }

            moreInfoPanel.SetActive(false);
            isMoreInfoActive = false;

            LogDebug("MoreInfo panel deactivated");
        }

        void UpdateMoreInfoDisplay()
        {
            if (moreInfoPanel == null || !isMoreInfoActive)
            {
                return;
            }

            if (currentSelectedIndex < 0 || currentSelectedIndex >= maskButtons.Length)
            {
                return;
            }

            Button currentButton = maskButtons[currentSelectedIndex];
            if (currentButton == null)
            {
                return;
            }

            MaskButton maskButton = currentButton.GetComponent<MaskButton>();
            if (maskButton == null || maskButton.maskPrefab == null)
            {
                LogWarning("Current button has no MaskButton component or mask prefab!");
                return;
            }

            LogDebug($"MoreInfo display updated for mask: {maskButton.maskPrefab.name}");
        }

        #endregion

        #region Initialization

        public override void Initialize()
        {
            base.Initialize();

            FindMaskButtons();
            SetupMaskButtonNavigation();
            SetupButtonEventTriggers();

            if (gameInputSO == null)
            {
                gameInputSO = FindFirstObjectByType<GameInputSO>();
                if (gameInputSO == null)
                {
                    LogError("GameInputSO not found!");
                }
            }

            LogDebug("MaskSelectionNavigationController initialized");
        }

        void FindMaskButtons()
        {
            if (maskButtons == null || maskButtons.Length == 0)
            {
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
                    Button[] foundButtons = maskContainer.GetComponentsInChildren<Button>();
                    maskButtons = foundButtons;
                    LogDebug($"Found {maskButtons.Length} mask buttons in Mask container");
                }
                else
                {
                    LogError("Could not find Mask container in hierarchy!");
                }
            }

            if (moreInfoPanel == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Transform moreInfoTransform = canvas.transform.Find("MoreInfo");
                    if (moreInfoTransform != null)
                    {
                        moreInfoPanel = moreInfoTransform.gameObject;
                        LogDebug($"Found MoreInfo panel: {moreInfoPanel.name}");
                    }
                    else
                    {
                        LogWarning("MoreInfo panel not found in Canvas!");
                    }
                }
            }

            if (firstSelectedButton == null && maskButtons != null && maskButtons.Length > 0)
            {
                firstSelectedButton = maskButtons[0].gameObject;
                LogDebug($"Set first selected button to: {firstSelectedButton.name}");
            }
        }

        void SetupMaskButtonNavigation()
        {
            if (maskButtons == null || maskButtons.Length == 0)
            {
                LogError("No mask buttons found for navigation setup!");
                return;
            }

            if (maskButtons.Length != 4)
            {
                LogWarning($"Expected 4 mask buttons, but found {maskButtons.Length}. Navigation may not work as expected.");
            }

            LogDebug($"Setting up explicit horizontal navigation for {maskButtons.Length} buttons");

            for (int i = 0; i < maskButtons.Length; i++)
            {
                if (maskButtons[i] == null) continue;

                Navigation navigation = maskButtons[i].navigation;
                navigation.mode = Navigation.Mode.Explicit;

                navigation.selectOnUp = null;
                navigation.selectOnDown = null;

                switch (i)
                {
                    case 0: // Button 0
                        navigation.selectOnLeft = maskButtons[3];  // Button 3
                        navigation.selectOnRight = maskButtons[1]; // Button 1
                        break;

                    case 1: // Button 1  
                        navigation.selectOnLeft = maskButtons[0];  // Button 0
                        navigation.selectOnRight = maskButtons[2]; // Button 2
                        break;

                    case 2: // Button 2
                        navigation.selectOnLeft = maskButtons[1];  // Button 1
                        navigation.selectOnRight = maskButtons[3]; // Button 3
                        break;

                    case 3: // Button 3
                        navigation.selectOnLeft = maskButtons[2];  // Button 2
                        navigation.selectOnRight = maskButtons[0]; // Button 0
                        break;

                    default:
                        LogWarning($"Unexpected button index: {i}");
                        break;
                }

                maskButtons[i].navigation = navigation;
                LogDebug($"Setup explicit navigation for button {i}: Left={navigation.selectOnLeft?.name}, Right={navigation.selectOnRight?.name}");
            }

            LogDebug("Explicit horizontal navigation setup completed");
        }

        void HandleSelectionChange()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != lastSelectedGameObject)
            {
                // Find which button index is selected
                for (int i = 0; i < maskButtons.Length; i++)
                {
                    if (maskButtons[i] != null && maskButtons[i].gameObject == currentSelected)
                    {
                        OnButtonSelected(i);
                        LogDebug($"Selection changed to button {i} via EventSystem");
                        break;
                    }
                }

                lastSelectedGameObject = currentSelected;
            }
        }

        void SetupButtonEventTriggers()
        {
            if (maskButtons == null)
            {
                return;
            }

            for (int i = 0; i < maskButtons.Length; i++)
            {
                if (maskButtons[i] == null)
                {
                    continue;
                }

                int buttonIndex = i;

                maskButtons[i].onClick.RemoveAllListeners();
                LogDebug($"Cleared onClick listeners for button {buttonIndex}");

                EventTrigger eventTrigger = maskButtons[i].GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = maskButtons[i].gameObject.AddComponent<EventTrigger>();
                }

                eventTrigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Select
                };
                selectEntry.callback.AddListener((data) => OnButtonSelected(buttonIndex));
                eventTrigger.triggers.Add(selectEntry);

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnterEntry.callback.AddListener((data) => OnPointerEnter(buttonIndex));
                eventTrigger.triggers.Add(pointerEnterEntry);

                LogDebug($"Setup event triggers for button {buttonIndex}");
            }
        }

        void OnPointerEnter(int buttonIndex)
        {
            if (ignoreMouseHover)
            {
                LogDebug($"Mouse hover ignored for button {buttonIndex} (ignoreMouseHover = true)");
                return;
            }

            UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
            if (inputSwitcher != null && !inputSwitcher.IsUsingController())
            {
                OnButtonSelected(buttonIndex);
                LogDebug($"Mouse hover triggered selection for button {buttonIndex}");
            }
        }

        #endregion

        #region Public Methods

        public void ActivateNavigation()
        {
            isActive = true;

            if (gameInputSO != null)
            {
                gameInputSO.SwitchToUI();
            }

            if (moreInfoPanel != null)
            {
                moreInfoPanel.SetActive(false);
                isMoreInfoActive = false;
                LogDebug("MoreInfo panel set to disabled on navigation activation");
            }

            SetInitialSelection();

            LogDebug("Mask selection navigation activated");
        }

        public void DeactivateNavigation()
        {
            isActive = false;
            LogDebug("Mask selection navigation deactivated");
        }

        public void SetInitialSelection()
        {
            if (maskButtons == null || maskButtons.Length == 0)
            {
                LogWarning("No mask buttons available for initial selection");
                return;
            }

            currentSelectedIndex = 0;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(maskButtons[0].gameObject);

                MaskButton firstMaskButton = maskButtons[0].GetComponent<MaskButton>();

                if (firstMaskButton != null)
                {
                    firstMaskButton.SetVisualState(true);
                }

                UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                if (inputSwitcher != null)
                {
                    inputSwitcher.SetFirstSelectedButton(maskButtons[0].gameObject);
                    inputSwitcher.ForceSelectButton(maskButtons[0].gameObject);
                }

                LogDebug($"Set initial selection to button 0: {maskButtons[0].name}");
            }
        }

        public void ForceSelectButton(int index)
        {
            if (index < 0 || index >= maskButtons.Length)
            {
                LogWarning($"Invalid button index: {index}");
                return;
            }

            currentSelectedIndex = index;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(maskButtons[index].gameObject);
                LogDebug($"Forced selection to button {index}: {maskButtons[index].name}");
            }
        }

        #endregion

        #region Input Handling

        void HandleRawInput()
        {
            return;
        }

        void HandleSubmitInput()
        {
            if (Time.unscaledTime - lastSubmitTime < rawInputCooldown)
            {
                return;
            }

            bool submitPressed = false;

            if (gameInputSO != null && gameInputSO.inputActions.UI.Submit.WasPressedThisFrame())
            {
                submitPressed = true;
                LogDebug("Submit input detected via UI.Submit");
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                submitPressed = true;
                LogDebug("Submit input detected via Gamepad South button");
            }

            if (submitPressed)
            {
                lastSubmitTime = Time.unscaledTime;

                if (isMoreInfoActive)
                {
                    if (Time.unscaledTime - lastMoreInfoActivationTime >= moreInfoActivationDelay)
                    {
                        SelectCurrentMask();
                    }
                    else
                    {
                        LogDebug("Submit ignored - MoreInfo activation delay not met");
                    }
                }
                else
                {
                    ShowMoreInfo();
                    lastMoreInfoActivationTime = Time.unscaledTime;
                }
            }
        }

        void SelectCurrentMask()
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= maskButtons.Length)
            {
                LogWarning("Invalid current selection index for mask selection!");
                return;
            }

            Button currentButton = maskButtons[currentSelectedIndex];
            if (currentButton == null)
            {
                LogWarning("Current button is null!");
                return;
            }

            MaskButton maskButton = currentButton.GetComponent<MaskButton>();
            if (maskButton == null)
            {
                LogWarning("Current button has no MaskButton component!");
                return;
            }

            LogDebug($"Selecting mask via navigation controller: {maskButton.maskPrefab?.name}");

            maskButton.SelectMask();
        }

        void HandleCancelInput()
        {
            bool cancelPressed = false;

            if (gameInputSO != null && gameInputSO.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                cancelPressed = true;
                LogDebug("Cancel input detected via UI.Cancel");
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                cancelPressed = true;
                LogDebug("Cancel input detected via Gamepad East button");
            }

            if (cancelPressed && isMoreInfoActive)
            {
                HideMoreInfo();
            }
        }

        void ProcessHorizontalInput(float horizontalInput)
        {
            if (maskButtons == null || maskButtons.Length <= 1)
            {
                return;
            }

            int newIndex = currentSelectedIndex;

            if (horizontalInput > 0)
            {
                switch (currentSelectedIndex)
                {
                    case 0: newIndex = 1; break; // Button 0  Button 1
                    case 1: newIndex = 2; break; // Button 1  Button 2  
                    case 2: newIndex = 3; break; // Button 2  Button 3
                    case 3: newIndex = 0; break; // Button 3  Button 0 (wrap around)
                    default:
                        LogWarning($"Unexpected current index for right navigation: {currentSelectedIndex}");
                        break;
                }
            }
            else if (horizontalInput < 0)
            {
                switch (currentSelectedIndex)
                {
                    case 0: newIndex = 3; break; // Button 0  Button 3 (wrap around)
                    case 1: newIndex = 0; break; // Button 1  Button 0
                    case 2: newIndex = 1; break; // Button 2  Button 1
                    case 3: newIndex = 2; break; // Button 3  Button 2
                    default:
                        LogWarning($"Unexpected current index for left navigation: {currentSelectedIndex}");
                        break;
                }
            }

            if (newIndex != currentSelectedIndex && newIndex >= 0 && newIndex < maskButtons.Length)
            {
                NavigateToButton(newIndex);
            }
        }

        void NavigateToButton(int index)
        {
            if (index < 0 || index >= maskButtons.Length)
            {
                return;
            }

            currentSelectedIndex = index;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(maskButtons[index].gameObject);
                LogDebug($"Navigated to button {index}: {maskButtons[index].name}");
            }
        }

        void EnsureValidSelection()
        {
            if (EventSystem.current == null || maskButtons == null)
            {
                return;
            }

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected == null && !isMoreInfoActive)
            {
                if (maskButtons.Length > 0 && maskButtons[0] != null)
                {
                    EventSystem.current.SetSelectedGameObject(maskButtons[0].gameObject);
                    LogDebug("Restored selection to first button");
                }
            }
        }

        #endregion

        #region Event Handlers

        void OnButtonSelected(int index)
        {
            if (index < 0 || index >= maskButtons.Length)
            {
                return;
            }

            for (int i = 0; i < maskButtons.Length; i++)
            {
                if (i != index)
                {
                    MaskButton maskButton = maskButtons[i].GetComponent<MaskButton>();
                    if (maskButton != null)
                    {
                        maskButton.SetUnselected();
                    }
                }
            }

            MaskButton selectedMaskButton = maskButtons[index].GetComponent<MaskButton>();

            if (selectedMaskButton != null)
            {
                selectedMaskButton.SetVisualState(true);
            }

            currentSelectedIndex = index;
            LogDebug($"Button selected: {index} - {maskButtons[index].name}");
        }

        #endregion

        #region MenuController Overrides

        public override void Show()
        {
            base.Show();
            ActivateNavigation();

            if (navigationManager != null && firstSelectedButton != null)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.MaskSelection, firstSelectedButton);
                navigationManager.SetNavigationState(UINavigationState.MaskSelection);
                LogDebug("Registered with UINavigationManager for MaskSelection state");
            }
        }

        public override void Hide()
        {
            base.Hide();
            DeactivateNavigation();

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
            }
        }

        public override void HandleInput()
        {
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF00AA>[MaskSelectionNavigationController] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[MaskSelectionNavigationController] {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"[MaskSelectionNavigationController] {message}");
        }

        #endregion
    }
}