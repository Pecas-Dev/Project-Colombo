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
        [SerializeField] float checkInterval = 0.05f;

        [Header("Device Detection")]
        [SerializeField] float mouseMovementThreshold = 0.5f;
        [SerializeField] float gamepadMovementThreshold = 0.2f;
        [SerializeField] float deviceSwitchDelay = 0.15f;
        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = false;
        [SerializeField] bool enableVisualIndicator = false;

        bool wasUsingMouse = false;
        bool isUsingController = false;
        bool isTransitioningDevices = false;

        float lastMouseInputTime = 0f;
        float lastControllerInputTime = 0f;
        float lastSelectionTime = 0f;
        float lastDeviceSwitchTime = 0f;

        Dictionary<GameObject, List<GameObject>> adjacentButtons = new Dictionary<GameObject, List<GameObject>>();

        PlayerInput playerInput;
        EventSystem currentEventSystem;
        GameObject lastValidSelection;

        GameObject indicatorObject;

        void Awake()
        {
            if (enableVisualIndicator)
            {
                CreateVisualIndicator();
            }
        }

        void Start()
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
            currentEventSystem = EventSystem.current;

            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            if (firstSelectedButton != null && currentEventSystem != null)
            {
                currentEventSystem.SetSelectedGameObject(firstSelectedButton);
                lastValidSelection = firstSelectedButton;
                LogDebug($"Initially selected: {firstSelectedButton.name}");
            }

            MapUINavigation();

            StartCoroutine(MonitorInputDeviceChanges());
        }

        void CreateVisualIndicator()
        {
            indicatorObject = new GameObject("InputDeviceIndicator");
            indicatorObject.transform.SetParent(transform);

            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("DebugCanvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }
            }

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
            Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (selectables.Length > 0)
            {
                LogDebug($"Mapping UI navigation for {selectables.Length} selectable elements");

                foreach (Selectable selectable in selectables)
                {
                    if (selectable.gameObject.activeInHierarchy)
                    {
                        List<GameObject> adjacent = new List<GameObject>();

                        Navigation nav = selectable.navigation;

                        if (nav.mode == Navigation.Mode.Explicit)
                        {
                            if (nav.selectOnUp != null)
                            {
                                adjacent.Add(nav.selectOnUp.gameObject);
                            }
                            if (nav.selectOnDown != null)
                            {
                                adjacent.Add(nav.selectOnDown.gameObject);
                            }
                            if (nav.selectOnLeft != null)
                            {
                                adjacent.Add(nav.selectOnLeft.gameObject);
                            }
                            if (nav.selectOnRight != null)
                            {
                                adjacent.Add(nav.selectOnRight.gameObject);
                            }
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
                bool mouseInput = CheckForMouseInput();

                bool gamepadInput = CheckForGamepadInput();

                bool shouldSwitchToMouse = mouseInput && !wasUsingMouse && Time.unscaledTime - lastDeviceSwitchTime > deviceSwitchDelay;

                bool shouldSwitchToGamepad = gamepadInput && wasUsingMouse && Time.unscaledTime - lastDeviceSwitchTime > deviceSwitchDelay;

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

                if (currentEventSystem != null &&
                    isUsingController &&
                    currentEventSystem.currentSelectedGameObject == null &&
                    Time.unscaledTime - lastSelectionTime > reselectDelay)
                {
                    GameObject objectToSelect = lastValidSelection != null ? lastValidSelection : firstSelectedButton;

                    if (objectToSelect != null)
                    {
                        currentEventSystem.SetSelectedGameObject(objectToSelect);
                        lastSelectionTime = Time.unscaledTime;
                        LogDebug($"Reselected button: {objectToSelect.name}");
                    }
                }

                if (enableVisualIndicator && indicatorObject != null)
                {
                    Image image = indicatorObject.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = wasUsingMouse ? Color.white : Color.green;
                    }
                }

                yield return new WaitForSecondsRealtime(checkInterval);
            }
        }

        bool CheckForMouseInput()
        {
            if (Mouse.current == null)
            {
                return false;
            }

            bool mousePressed = Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame;
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
            if (Gamepad.current == null)
            {
                return false;
            }

            bool dpadMoved = Gamepad.current.dpad.ReadValue().sqrMagnitude > gamepadMovementThreshold;
            bool leftStickMoved = Gamepad.current.leftStick.ReadValue().sqrMagnitude > gamepadMovementThreshold;
            bool buttonPressed = Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame;

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

            if (currentEventSystem != null)
            {
                lastValidSelection = currentEventSystem.currentSelectedGameObject;
            }

            LogDebug("Switched to MOUSE input mode");
        }

        void SwitchToGamepadInput()
        {
            wasUsingMouse = false;
            isUsingController = true;

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

        public void UpdateUIMapping()
        {
            adjacentButtons.Clear();
            MapUINavigation();
            LogDebug("UI navigation mapping updated");
        }

        public bool IsUsingController()
        {
            return isUsingController;
        }

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
                GameObject objectToSelect = lastValidSelection != null ? lastValidSelection : firstSelectedButton;

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
            if (currentEventSystem != EventSystem.current)
            {
                currentEventSystem = EventSystem.current;
                LogDebug($"EventSystem reference updated: {(currentEventSystem != null ? currentEventSystem.name : "null")}");
            }

            if (currentEventSystem != null && currentEventSystem.currentSelectedGameObject != null && currentEventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                lastValidSelection = currentEventSystem.currentSelectedGameObject;
            }
        }
    }
}