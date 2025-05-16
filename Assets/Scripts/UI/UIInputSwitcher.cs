using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ProjectColombo.UI
{
    public class UIInputSwitcher : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] GameObject firstSelectedButton;
        [SerializeField] float reselectDelay = 0.1f;
        [SerializeField] float checkInterval = 0.05f; // More frequent checking for better responsiveness

        [Header("Device Detection")]
        [SerializeField] float mouseMovementThreshold = 0.5f;
        [SerializeField] float gamepadMovementThreshold = 0.2f;
        [SerializeField] float deviceSwitchDelay = 0.15f; // Time to wait after device input before switching

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = false;
        [SerializeField] bool enableVisualIndicator = false;

        // Input state tracking
        bool wasUsingMouse = false;
        bool isUsingController = false;
        bool isTransitioningDevices = false;

        // Timing variables
        float lastMouseInputTime = 0f;
        float lastControllerInputTime = 0f;
        float lastSelectionTime = 0f;
        float lastDeviceSwitchTime = 0f;

        // For managing navigation flow between buttons
        Dictionary<GameObject, List<GameObject>> adjacentButtons = new Dictionary<GameObject, List<GameObject>>();

        // References
        PlayerInput playerInput;
        EventSystem currentEventSystem;
        GameObject lastValidSelection;

        // Visual indicator for debug
        GameObject indicatorObject;

        void Awake()
        {
            // Create visual indicator if enabled
            if (enableVisualIndicator)
            {
                CreateVisualIndicator();
            }
        }

        void Start()
        {
            // Find the PlayerInput component
            playerInput = FindFirstObjectByType<PlayerInput>();
            currentEventSystem = EventSystem.current;

            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            // If we have a starting button, select it
            if (firstSelectedButton != null && currentEventSystem != null)
            {
                currentEventSystem.SetSelectedGameObject(firstSelectedButton);
                lastValidSelection = firstSelectedButton;
                LogDebug($"Initially selected: {firstSelectedButton.name}");
            }

            // Map the UI navigation structure for easier traversal if needed
            MapUINavigation();

            // Start monitoring for input device changes
            StartCoroutine(MonitorInputDeviceChanges());
        }

        void CreateVisualIndicator()
        {
            indicatorObject = new GameObject("InputDeviceIndicator");
            indicatorObject.transform.SetParent(transform);

            // Add a small UI element to indicate the current input device
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    // Create a new canvas if needed
                    GameObject canvasObj = new GameObject("DebugCanvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }
            }

            // Create an indicator that shows the current input device
            indicatorObject.transform.SetParent(canvas.transform);
            RectTransform rectTransform = indicatorObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-10, 10);
            rectTransform.sizeDelta = new Vector2(20, 20);

            Image image = indicatorObject.AddComponent<Image>();
            image.color = wasUsingMouse ? Color.white : Color.green;
        }

        void MapUINavigation()
        {
            // Find all selectable UI elements in the scene
            Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (selectables.Length > 0)
            {
                LogDebug($"Mapping UI navigation for {selectables.Length} selectable elements");

                // Build a map of the UI navigation
                foreach (Selectable selectable in selectables)
                {
                    if (selectable.gameObject.activeInHierarchy)
                    {
                        // Get adjacent buttons based on navigation
                        List<GameObject> adjacent = new List<GameObject>();

                        Navigation nav = selectable.navigation;
                        if (nav.mode == Navigation.Mode.Explicit)
                        {
                            if (nav.selectOnUp != null) adjacent.Add(nav.selectOnUp.gameObject);
                            if (nav.selectOnDown != null) adjacent.Add(nav.selectOnDown.gameObject);
                            if (nav.selectOnLeft != null) adjacent.Add(nav.selectOnLeft.gameObject);
                            if (nav.selectOnRight != null) adjacent.Add(nav.selectOnRight.gameObject);
                        }

                        adjacentButtons[selectable.gameObject] = adjacent;
                    }
                }
            }
        }

        IEnumerator MonitorInputDeviceChanges()
        {
            while (true)
            {
                // Check for mouse input
                bool mouseInput = CheckForMouseInput();

                // Check for gamepad input
                bool gamepadInput = CheckForGamepadInput();

                // Determine if we need to switch input modes
                bool shouldSwitchToMouse = mouseInput && !wasUsingMouse &&
                                           Time.unscaledTime - lastDeviceSwitchTime > deviceSwitchDelay;

                bool shouldSwitchToGamepad = gamepadInput && wasUsingMouse &&
                                             Time.unscaledTime - lastDeviceSwitchTime > deviceSwitchDelay;

                // Handle switching between mouse and gamepad
                if (shouldSwitchToMouse && !isTransitioningDevices)
                {
                    isTransitioningDevices = true;
                    SwitchToMouseInput();
                    lastDeviceSwitchTime = Time.unscaledTime;
                    StartCoroutine(ResetTransitionFlag(deviceSwitchDelay));
                }
                else if (shouldSwitchToGamepad && !isTransitioningDevices)
                {
                    isTransitioningDevices = true;
                    SwitchToGamepadInput();
                    lastDeviceSwitchTime = Time.unscaledTime;
                    StartCoroutine(ResetTransitionFlag(deviceSwitchDelay));
                }

                // Check if we need to restore a selection
                if (currentEventSystem != null &&
                    isUsingController &&
                    currentEventSystem.currentSelectedGameObject == null &&
                    Time.unscaledTime - lastSelectionTime > reselectDelay)
                {
                    // Reselect the last valid selection or first button
                    GameObject objectToSelect = lastValidSelection != null ?
                                               lastValidSelection :
                                               firstSelectedButton;

                    if (objectToSelect != null)
                    {
                        currentEventSystem.SetSelectedGameObject(objectToSelect);
                        lastSelectionTime = Time.unscaledTime;
                        LogDebug($"Reselected button: {objectToSelect.name}");
                    }
                }

                // Update debug visual indicator
                if (enableVisualIndicator && indicatorObject != null)
                {
                    Image image = indicatorObject.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = wasUsingMouse ? Color.white : Color.green;
                    }
                }

                // Wait a short time before checking again
                yield return new WaitForSecondsRealtime(checkInterval);
            }
        }

        bool CheckForMouseInput()
        {
            if (Mouse.current == null) return false;

            bool mousePressed = Mouse.current.leftButton.wasPressedThisFrame ||
                               Mouse.current.rightButton.wasPressedThisFrame ||
                               Mouse.current.middleButton.wasPressedThisFrame;

            bool mouseMoved = Mouse.current.delta.ReadValue().sqrMagnitude > mouseMovementThreshold;

            if (mousePressed || mouseMoved)
            {
                lastMouseInputTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        bool CheckForGamepadInput()
        {
            if (Gamepad.current == null) return false;

            // Check for D-pad input
            bool dpadMoved = Gamepad.current.dpad.ReadValue().sqrMagnitude > gamepadMovementThreshold;

            // Check for stick input
            bool leftStickMoved = Gamepad.current.leftStick.ReadValue().sqrMagnitude > gamepadMovementThreshold;

            // Check for face buttons
            bool buttonPressed = Gamepad.current.buttonSouth.wasPressedThisFrame ||
                                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                                Gamepad.current.buttonEast.wasPressedThisFrame ||
                                Gamepad.current.buttonWest.wasPressedThisFrame;

            if (dpadMoved || leftStickMoved || buttonPressed)
            {
                lastControllerInputTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        void SwitchToMouseInput()
        {
            wasUsingMouse = true;
            isUsingController = false;

            // When using mouse, we don't need to maintain selection
            if (currentEventSystem != null)
            {
                lastValidSelection = currentEventSystem.currentSelectedGameObject;
                // We don't clear the selection here to avoid disrupting click events
            }

            LogDebug("Switched to MOUSE input mode");
        }

        void SwitchToGamepadInput()
        {
            wasUsingMouse = false;
            isUsingController = true;

            // When using controller, we need to ensure something is selected
            if (currentEventSystem != null && currentEventSystem.currentSelectedGameObject == null)
            {
                GameObject objectToSelect = firstSelectedButton;

                if (objectToSelect != null)
                {
                    currentEventSystem.SetSelectedGameObject(objectToSelect);
                    lastValidSelection = objectToSelect;
                    lastSelectionTime = Time.unscaledTime;
                    LogDebug($"Selected button for controller: {objectToSelect.name}");
                }
                else
                {
                    LogDebug("Unable to find valid button to select for controller input");
                }
            }

            LogDebug("Switched to CONTROLLER input mode");
        }

        IEnumerator ResetTransitionFlag(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            isTransitioningDevices = false;
        }

        public void SetFirstSelectedButton(GameObject button)
        {
            if (button == null) return;

            firstSelectedButton = button;
            LogDebug($"First selected button set to: {button.name}");

            if (isUsingController && currentEventSystem != null && currentEventSystem.currentSelectedGameObject == null)
            {
                currentEventSystem.SetSelectedGameObject(firstSelectedButton);
                lastValidSelection = firstSelectedButton;
                lastSelectionTime = Time.unscaledTime;
                LogDebug($"Selected button: {firstSelectedButton.name}");
            }
        }

        // Update the mapping when the UI structure changes
        public void UpdateUIMapping()
        {
            adjacentButtons.Clear();
            MapUINavigation();
            LogDebug("UI navigation mapping updated");
        }

        // Get the currently detected input device
        public bool IsUsingController()
        {
            return isUsingController;
        }

        // Force selection of a specific button (useful for menu transitions)
        public void ForceSelectButton(GameObject button)
        {
            if (button == null || currentEventSystem == null) return;

            currentEventSystem.SetSelectedGameObject(button);
            lastValidSelection = button;
            lastSelectionTime = Time.unscaledTime;
            LogDebug($"Forced selection of button: {button.name}");
        }

        public void RefreshEventSystemReference()
        {
            currentEventSystem = EventSystem.current;
            if (currentEventSystem != null && isUsingController)
            {
                // Force reselection of the last valid selection
                GameObject objectToSelect = lastValidSelection != null ?
                                          lastValidSelection :
                                          firstSelectedButton;

                if (objectToSelect != null)
                {
                    currentEventSystem.SetSelectedGameObject(objectToSelect);
                }
            }
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAAA>[UIInputSwitcher] {message}</color>");
            }
        }

        void Update()
        {
            // Track the current EventSystem
            if (currentEventSystem != EventSystem.current)
            {
                currentEventSystem = EventSystem.current;
                LogDebug($"EventSystem reference updated: {(currentEventSystem != null ? currentEventSystem.name : "null")}");
            }

            // Keep track of the last valid selection
            if (currentEventSystem != null &&
                currentEventSystem.currentSelectedGameObject != null &&
                currentEventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                lastValidSelection = currentEventSystem.currentSelectedGameObject;
            }
        }
    }
}