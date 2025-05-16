using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


namespace ProjectColombo.UI
{
    public class UIDebugger : MonoBehaviour
    {
        /*public static UIDebugger Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;
        [SerializeField] bool enableOnScreenDisplay = true;
        [SerializeField] int maxLogEntries = 20;
        [SerializeField] bool trackInputStates = true;
        [SerializeField] bool trackEventSystem = true;
        [SerializeField] bool trackCanvasStates = true;
        [SerializeField] bool trackInputActions = true;
        [SerializeField] bool trackTimeScale = true;
        [SerializeField] bool trackSceneChanges = true;

        [Header("Display Settings")]
        [SerializeField] KeyCode toggleDisplayKey = KeyCode.F1;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color warningColor = Color.yellow;
        [SerializeField] Color errorColor = Color.red;
        [SerializeField] Color actionColor = Color.cyan;
        [SerializeField] Color stateColor = Color.green;

        List<LogEntry> logEntries = new List<LogEntry>();
        List<string> filteredLogs = new List<string>();
        bool isDisplayActive = false;
        Rect windowRect = new Rect(10, 10, 400, 400);
        string currentCategory = "All";
        Vector2 scrollPosition = Vector2.zero;
        string[] categoryOptions = { "All", "Input", "EventSystem", "Menu", "UI", "Canvas", "Scene", "Time" };

        GameObject lastSelectedGameObject;
        EventSystem currentEventSystem;
        PlayerInput playerInput;
        string lastActiveActionMap = "Unknown";
        float lastTimeScale = 1.0f;
        bool wasGamePaused = false;

        class LogEntry
        {
            public string Message { get; set; }
            public string Category { get; set; }
            public LogType Type { get; set; }
            public float Timestamp { get; set; }
        }

        public enum LogType
        {
            Normal,
            Warning,
            Error,
            Action,
            State
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.logMessageReceived += HandleLog;
        }

        void Start()
        {
            Initialize();
            AddLog("UIDebugger initialized", "UI", LogType.Normal);
            if (enableOnScreenDisplay)
            {
                AddLog("On-screen display active - Press " + toggleDisplayKey + " to toggle", "UI", LogType.Action);
            }
        }

        void Initialize()
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
            currentEventSystem = EventSystem.current;

            if (currentEventSystem == null)
            {
                currentEventSystem = FindFirstObjectByType<EventSystem>();
                AddLog("No active EventSystem found, searched for any available: " + (currentEventSystem != null), "EventSystem", LogType.Warning);
            }
            else
            {
                AddLog("EventSystem found: " + currentEventSystem.name, "EventSystem", LogType.Normal);
            }

            if (playerInput != null)
            {
                AddLog("PlayerInput found: " + playerInput.name + ", current action map: " + playerInput.currentActionMap.name, "Input", LogType.Normal);
                lastActiveActionMap = playerInput.currentActionMap.name;
            }
            else
            {
                AddLog("No PlayerInput component found", "Input", LogType.Warning);
            }

            lastTimeScale = Time.timeScale;
            wasGamePaused = lastTimeScale < 0.01f;

            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AddLog($"Found {canvases.Length} canvases in scene", "Canvas", LogType.Normal);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.activeInHierarchy)
                {
                    AddLog($"Active canvas: {GetGameObjectPath(canvas.gameObject)}", "Canvas", LogType.Normal);
                }
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AddLog($"Scene loaded: {scene.name}, mode: {mode}", "Scene", LogType.State);
            if (trackSceneChanges)
            {
                StartCoroutine(AnalyzeSceneNextFrame(scene));
            }
        }

        IEnumerator AnalyzeSceneNextFrame(Scene scene)
        {
            yield return null;
            yield return null;

            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AddLog($"Scene {scene.name} contains {canvases.Length} canvases", "Scene", LogType.Normal);

            int activeCanvases = 0;
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.activeInHierarchy)
                {
                    activeCanvases++;
                }
            }

            AddLog($"{activeCanvases} active canvases in scene {scene.name}", "Canvas", LogType.Normal);

            if (EventSystem.current != currentEventSystem)
            {
                currentEventSystem = EventSystem.current;
                AddLog($"EventSystem changed to: {(currentEventSystem != null ? currentEventSystem.name : "None")}", "EventSystem", LogType.Warning);
            }

            PlayerInput newPlayerInput = FindFirstObjectByType<PlayerInput>();

            if (newPlayerInput != playerInput)
            {
                playerInput = newPlayerInput;
                AddLog($"PlayerInput changed to: {(playerInput != null ? playerInput.name : "None")}", "Input", LogType.Warning);
            }

            ProjectColombo.UI.MenuController[] menuControllers = FindObjectsByType<ProjectColombo.UI.MenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AddLog($"Scene {scene.name} contains {menuControllers.Length} menu controllers", "Menu", LogType.Normal);

            foreach (MenuController controller in menuControllers)
            {
                AddLog($"Menu controller: {controller.GetType().Name} at {GetGameObjectPath(controller.gameObject)}, active: {controller.gameObject.activeInHierarchy}", "Menu", LogType.Normal);
            }

            if (UIManager.Instance != null)
            {
                AddLog($"UIManager.Instance exists, current menu: {(UIManager.Instance.GetCurrentMenu() != null ? UIManager.Instance.GetCurrentMenu().GetType().Name : "None")}", "UI", LogType.Normal);
            }
            else
            {
                AddLog("No UIManager.Instance available", "UI", LogType.Warning);
            }

            var gameManager = FindFirstObjectByType<ProjectColombo.GameManagement.GameManager>();
            if (gameManager != null)
            {
                AddLog($"GameManager found, gameIsPaused: {gameManager.gameIsPaused}", "UI", LogType.Normal);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleDisplayKey))
            {
                isDisplayActive = !isDisplayActive;
                AddLog("Debug display " + (isDisplayActive ? "enabled" : "disabled"), "UI", LogType.Action);
            }

            if (trackEventSystem && EventSystem.current != null)
            {
                GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
                if (selectedObject != lastSelectedGameObject)
                {
                    if (selectedObject != null)
                    {
                        AddLog("EventSystem selection changed to: " + GetGameObjectPath(selectedObject), "EventSystem", LogType.Normal);
                    }
                    else
                    {
                        AddLog("EventSystem selection cleared (null)", "EventSystem", LogType.Warning);
                    }
                    lastSelectedGameObject = selectedObject;
                }
            }

            if (trackInputActions && playerInput != null && playerInput.currentActionMap != null && playerInput.currentActionMap.name != lastActiveActionMap)
            {
                AddLog("Input action map changed from: " + lastActiveActionMap + " to: " + playerInput.currentActionMap.name, "Input", LogType.State);
                lastActiveActionMap = playerInput.currentActionMap.name;
            }

            if (trackTimeScale && Math.Abs(Time.timeScale - lastTimeScale) > 0.01f)
            {
                AddLog("Time scale changed from " + lastTimeScale + " to " + Time.timeScale, "Time", LogType.State);
                lastTimeScale = Time.timeScale;

                bool isPaused = Time.timeScale < 0.01f;
                if (isPaused != wasGamePaused)
                {
                    AddLog("Game " + (isPaused ? "PAUSED" : "RESUMED"), "Time", LogType.Action);
                    wasGamePaused = isPaused;
                }
            }

            if (trackInputStates)
            {
                if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
                {
                    if (Gamepad.current.startButton.wasPressedThisFrame)
                    {
                        AddLog("Gamepad START button pressed", "Input", LogType.Action);
                    }
                    if (Gamepad.current.selectButton.wasPressedThisFrame)
                    {
                        AddLog("Gamepad SELECT button pressed", "Input", LogType.Action);
                    }
                }

                if (Keyboard.current != null)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        AddLog("Keyboard ESCAPE pressed", "Input", LogType.Action);
                    }
                    if (Keyboard.current.enterKey.wasPressedThisFrame)
                    {
                        AddLog("Keyboard ENTER pressed", "Input", LogType.Action);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!enableOnScreenDisplay || !isDisplayActive)
                return;

            windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, "UI Debug Console");
        }

        private void DrawDebugWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            int selectedIndex = Array.IndexOf(categoryOptions, currentCategory);
            selectedIndex = GUILayout.SelectionGrid(selectedIndex, categoryOptions, 4);
            if (selectedIndex >= 0 && selectedIndex < categoryOptions.Length)
            {
                currentCategory = categoryOptions[selectedIndex];
                UpdateFilteredLogs();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                ClearLogs();
            }

            trackInputStates = GUILayout.Toggle(trackInputStates, "Input", GUILayout.Width(60));
            trackEventSystem = GUILayout.Toggle(trackEventSystem, "Events", GUILayout.Width(60));
            trackCanvasStates = GUILayout.Toggle(trackCanvasStates, "Canvas", GUILayout.Width(60));
            trackInputActions = GUILayout.Toggle(trackInputActions, "Actions", GUILayout.Width(60));
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            for (int i = filteredLogs.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(filteredLogs[i]);
            }

            GUILayout.EndScrollView();

            GUILayout.Box($"Scene: {SceneManager.GetActiveScene().name} | Time: {Time.timeScale:F2} | " +
                          $"Selected: {(EventSystem.current?.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "None")} | " +
                          $"ActionMap: {lastActiveActionMap}");

            GUI.DragWindow();
        }

        void UpdateFilteredLogs()
        {
            filteredLogs.Clear();
            foreach (LogEntry entry in logEntries)
            {
                if (currentCategory == "All" || entry.Category == currentCategory)
                {
                    string logText = $"[{entry.Timestamp:F1}] [{entry.Category}] {entry.Message}";
                    switch (entry.Type)
                    {
                        case LogType.Warning:
                            logText = $"<color=#{ColorUtility.ToHtmlStringRGB(warningColor)}>{logText}</color>";
                            break;
                        case LogType.Error:
                            logText = $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>{logText}</color>";
                            break;
                        case LogType.Action:
                            logText = $"<color=#{ColorUtility.ToHtmlStringRGB(actionColor)}>{logText}</color>";
                            break;
                        case LogType.State:
                            logText = $"<color=#{ColorUtility.ToHtmlStringRGB(stateColor)}>{logText}</color>";
                            break;
                    }
                    filteredLogs.Add(logText);
                }
            }
        }

        void ClearLogs()
        {
            logEntries.Clear();
            filteredLogs.Clear();
            AddLog("Logs cleared", "UI", LogType.Normal);
        }

        public void AddLog(string message, string category, LogType type)
        {
            LogEntry entry = new LogEntry
            {
                Message = message,
                Category = category,
                Type = type,
                Timestamp = Time.unscaledTime
            };

            logEntries.Add(entry);
            if (logEntries.Count > maxLogEntries)
            {
                logEntries.RemoveAt(0);
            }

            if (enableDebugLogs)
            {
                string colorCode = "";
                switch (type)
                {
                    case LogType.Normal:
                        colorCode = "#FFFFFF";
                        break;
                    case LogType.Warning:
                        colorCode = "#FFFF00";
                        break;
                    case LogType.Error:
                        colorCode = "#FF0000";
                        break;
                    case LogType.Action:
                        colorCode = "#00FFFF";
                        break;
                    case LogType.State:
                        colorCode = "#00FF00";
                        break;
                }

                UnityEngine.Debug.Log($"<color={colorCode}>[{category}] {message}</color>");
            }

            UpdateFilteredLogs();
        }

        void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
            {
                AddLog("Unity Error: " + logString, "UI", LogType.Error);
            }
            else if (type == UnityEngine.LogType.Warning)
            {
                if (logString.Contains("UI") || logString.Contains("EventSystem") || logString.Contains("Canvas"))
                {
                    AddLog("Unity Warning: " + logString, "UI", LogType.Warning);
                }
            }
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";

            StringBuilder sb = new StringBuilder();
            Transform current = obj.transform;

            while (current != null)
            {
                if (sb.Length > 0)
                {
                    sb.Insert(0, "/");
                }
                sb.Insert(0, current.name);
                current = current.parent;
            }

            return sb.ToString();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.logMessageReceived -= HandleLog;
        }*/
    }
}