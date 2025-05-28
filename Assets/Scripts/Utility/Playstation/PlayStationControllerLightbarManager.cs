using UnityEngine;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;
using UnityEngine.InputSystem.DualShock;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.InputSystem.Controllers
{
    public class PlayStationControllerLightbarManager : MonoBehaviour
    {
        [Header("Lightbar Colors")]
        [SerializeField] Color defaultColor = Color.blue;
        [SerializeField] Color minorAttackColor = new Color(0.5f, 0f, 1f, 1f); // Purple
        [SerializeField] Color majorAttackColor = Color.yellow;

        [Header("Parry Colors")]
        [SerializeField] Color minorParrySuccessColor = new Color(0f, 1f, 0.5f, 1f); // Green-cyan for minor parry
        [SerializeField] Color majorParrySuccessColor = new Color(1f, 0.5f, 0f, 1f); // Orange for major parry

        [Header("Lightbar Settings")]
        [SerializeField] float colorDuration = 0.5f;
        [SerializeField] float parryColorDuration = 0.8f; // Slightly longer for parry feedback
        [SerializeField] bool debugMode = false;

        [Header("Deep Debugging")]
        [SerializeField] bool enableDeepPS4Debugging = true;
        [SerializeField] bool logEveryColorAttempt = false;
        [SerializeField] bool resetPS4StateAfterParry = false;

        // State tracking variables
        int totalParryAttempts = 0;
        int successfulParryColorSets = 0;
        bool hasEverWorkedOnce = false;
        float lastSuccessfulParryTime = -1f;

        [Header("Controller Priority Settings")]
        [SerializeField] bool disablePS5Support = false;
        [SerializeField] bool forcePS4Only = false;
        [Tooltip("Forces system to only use PS4 controller, completely ignoring PS5")]

        [Header("Advanced Debugging")]
        [SerializeField] bool enableEventComparison = true;
        [SerializeField] bool delayPS4ParryColor = false;
        [SerializeField] float ps4ParryDelay = 0.1f;

        [Header("PS4 Troubleshooting")]
        [SerializeField] bool useAlternatePS4Colors = false;
        [SerializeField] bool forceMaxBrightnessPS4 = false;
        [SerializeField] float ps4ColorIntensityMultiplier = 1.0f;

        [Header("Debug Controls")]
        [SerializeField] bool enableKeyboardDebugTriggers = true;

        [Header("Real-Time Debug Testing")]
        [SerializeField] bool enableRealtimeColorTesting = false;
        [SerializeField] Color debugTestColor = Color.white;

        GameInputSO gameInputSO;
        DualShockGamepad dualShockController;
        DualSenseGamepadHID dualSenseController;

        bool isLightbarActive = false;
        float colorTimer = 0f;
        Color currentActiveColor;
        Color previousDebugColor;

        void Awake()
        {
            DetectPlayStationController();
        }

        void OnEnable()
        {
            CustomEvents.OnMinorAttackPerformed += HandleMinorAttackPerformed;
            CustomEvents.OnMajorAttackPerformed += HandleMajorAttackPerformed;
            CustomEvents.OnLightbarColorChangeRequested += HandleColorChangeRequest;
            CustomEvents.OnSuccessfullParry += HandleSuccessfulParry;
        }

        void OnDisable()
        {
            CustomEvents.OnMinorAttackPerformed -= HandleMinorAttackPerformed;
            CustomEvents.OnMajorAttackPerformed -= HandleMajorAttackPerformed;
            CustomEvents.OnLightbarColorChangeRequested -= HandleColorChangeRequest;
            CustomEvents.OnSuccessfullParry -= HandleSuccessfulParry;
        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                gameInputSO = GameManager.Instance.gameInput;
            }

            SetLightbarColor(defaultColor);
            previousDebugColor = debugTestColor;
            LogDebug("PlayStation Controller Lightbar Manager initialized with DualSense support");
        }

        void Update()
        {
            // Refresh controller detection periodically to catch hot-swaps
            if (Time.frameCount % 60 == 0) // Every 60 frames (about once per second)
            {
                RefreshControllerDetection();
            }

            if (isLightbarActive)
            {
                colorTimer -= Time.deltaTime;

                if (colorTimer <= 0f)
                {
                    ReturnToDefaultColor();
                }
            }

            HandleDirectInputDetection();
            HandleRealtimeColorTesting();
            HandleKeyboardDebugTriggers();
        }

        void HandleDirectInputDetection()
        {
            if (gameInputSO == null)
            {
                return;
            }

            if (gameInputSO.GetInputPressed(PlayerInputAction.MinorAttack))
            {
                SetAttackColor(minorAttackColor);
                LogDebug("Minor attack input detected - lightbar set to purple");
            }
            else if (gameInputSO.GetInputPressed(PlayerInputAction.MajorAttack))
            {
                SetAttackColor(majorAttackColor);
                LogDebug("Major attack input detected - lightbar set to yellow");
            }
        }

        void HandleKeyboardDebugTriggers()
        {
            if (!enableKeyboardDebugTriggers || !debugMode) return;

            // Key 1 - Trigger Minor Parry Success
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TriggerDebugMinorParry();
            }
            // Key 2 - Trigger Major Parry Success  
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TriggerDebugMajorParry();
            }
        }

        void TriggerDebugMinorParry()
        {
            if (enableEventComparison)
            {
                LogDebug("=== DEBUG PARRY EVENT vs REAL COMPARISON ===");
                LogDebug($"Event Source: KEYBOARD DEBUG");
                LogDebug($"Time: {Time.time}");
                LogDebug($"Frame: {Time.frameCount}");
                LogDebug($"Current Lightbar Active: {isLightbarActive}");
                LogDebug($"Color Timer: {colorTimer}");
            }

            LogDebug("=== DEBUG: Triggering Minor Parry Event (Key 1) ===");
            CustomEvents.SuccessfullParry(GameGlobals.MusicScale.MINOR, true);
        }

        void TriggerDebugMajorParry()
        {
            if (enableEventComparison)
            {
                LogDebug("=== DEBUG PARRY EVENT vs REAL COMPARISON ===");
                LogDebug($"Event Source: KEYBOARD DEBUG");
                LogDebug($"Time: {Time.time}");
                LogDebug($"Frame: {Time.frameCount}");
                LogDebug($"Current Lightbar Active: {isLightbarActive}");
                LogDebug($"Color Timer: {colorTimer}");
            }

            LogDebug("=== DEBUG: Triggering Major Parry Event (Key 2) ===");
            CustomEvents.SuccessfullParry(GameGlobals.MusicScale.MAJOR, true);
        }

        void HandleRealtimeColorTesting()
        {
            if (!enableRealtimeColorTesting) return;

            if (debugTestColor != previousDebugColor)
            {
                SetLightbarColor(debugTestColor);
                previousDebugColor = debugTestColor;
                LogDebug($"Real-time debug color changed to: {debugTestColor}");
            }
        }

        void DetectPlayStationController()
        {
            // First check if we have a DualSense controller specifically
            var currentController = DualShockGamepad.current;

            if (currentController is DualSenseGamepadHID dualSense)
            {
                dualSenseController = dualSense;
                dualShockController = null;
                LogDebug("DualSense (PS5) controller detected");
            }
            else if (currentController != null)
            {
                dualShockController = currentController;
                dualSenseController = null;
                LogDebug("DualShock 4 controller detected");
            }
            else
            {
                dualShockController = null;
                dualSenseController = null;
                LogDebug("No PlayStation controller detected - lightbar functionality will be inactive");
            }
        }

        void HandleMinorAttackPerformed(GameGlobals.MusicScale scale)
        {
            SetAttackColor(minorAttackColor);
            LogDebug("Minor attack event received - lightbar set to purple");
        }

        void HandleMajorAttackPerformed(GameGlobals.MusicScale scale)
        {
            SetAttackColor(majorAttackColor);
            LogDebug("Major attack event received - lightbar set to yellow");
        }

        void HandleSuccessfulParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            Color parryColor = (scale == GameGlobals.MusicScale.MINOR) ? minorParrySuccessColor : majorParrySuccessColor;

            // Force parry color with extra PS4 debugging
            LogDebug($"=== PARRY EVENT TRIGGERED ===");
            LogDebug($"Scale: {scale}, Same Scale: {sameScale}");
            LogDebug($"Parry Color: {parryColor}");
            LogDebug($"DualShock Controller: {(dualShockController != null ? "Connected" : "Not Connected")}");
            LogDebug($"DualSense Controller: {(dualSenseController != null ? "Connected" : "Not Connected")}");

            SetParryColor(parryColor);

            string scaleText = (scale == GameGlobals.MusicScale.MINOR) ? "minor" : "major";
            string sameScaleText = sameScale ? " (same scale)" : " (different scale)";
            LogDebug($"Successful {scaleText} parry detected{sameScaleText} - lightbar set to {(scale == GameGlobals.MusicScale.MINOR ? "green-cyan" : "orange")}");
        }

        void HandleColorChangeRequest(Color requestedColor)
        {
            SetLightbarColor(requestedColor);
            LogDebug($"Color change requested: {requestedColor}");
        }

        void SetAttackColor(Color attackColor)
        {
            if (!enableRealtimeColorTesting)
            {
                SetLightbarColor(attackColor);
                currentActiveColor = attackColor;
                colorTimer = colorDuration;
                isLightbarActive = true;
            }
        }

        void SetParryColor(Color parryColor)
        {
            if (!enableRealtimeColorTesting)
            {
                SetLightbarColor(parryColor);
                currentActiveColor = parryColor;
                colorTimer = parryColorDuration; // Use longer duration for parry feedback
                isLightbarActive = true;
            }
        }

        void ReturnToDefaultColor()
        {
            if (!enableRealtimeColorTesting)
            {
                SetLightbarColor(defaultColor);
                isLightbarActive = false;
                colorTimer = 0f;
                LogDebug("Lightbar returned to default color");
            }
        }

        void SetLightbarColor(Color color)
        {
            bool colorSet = false;

            if (dualSenseController != null)
            {
                try
                {
                    dualSenseController.SetLightBarColor(color);
                    colorSet = true;
                    LogDebug($"DualSense lightbar color set to: {color}");
                }
                catch (System.Exception e)
                {
                    LogDebug($"Failed to set DualSense color: {e.Message}");
                }
            }
            else if (dualShockController != null)
            {
                try
                {
                    // Force the color setting for PS4 DualShock
                    dualShockController.SetLightBarColor(color);
                    colorSet = true;
                    LogDebug($"DualShock 4 lightbar color set to: {color}");

                    // Additional PS4 specific debugging
                    LogDebug($"PS4 Controller Info - Name: {dualShockController.name}, Description: {dualShockController.description}");
                }
                catch (System.Exception e)
                {
                    LogDebug($"Failed to set DualShock 4 color: {e.Message}");
                }
            }

            if (!colorSet)
            {
                LogDebug($"WARNING: No controller available to set color {color}");
            }
        }

        public void SetDefaultColor(Color newDefaultColor)
        {
            defaultColor = newDefaultColor;

            if (!isLightbarActive && !enableRealtimeColorTesting)
            {
                SetLightbarColor(defaultColor);
            }

            LogDebug($"Default color changed to: {newDefaultColor}");
        }

        public void SetMinorAttackColor(Color newMinorAttackColor)
        {
            minorAttackColor = newMinorAttackColor;
            LogDebug($"Minor attack color changed to: {newMinorAttackColor}");
        }

        public void SetMajorAttackColor(Color newMajorAttackColor)
        {
            majorAttackColor = newMajorAttackColor;
            LogDebug($"Major attack color changed to: {newMajorAttackColor}");
        }

        public void SetMinorParryColor(Color newMinorParryColor)
        {
            minorParrySuccessColor = newMinorParryColor;
            LogDebug($"Minor parry success color changed to: {newMinorParryColor}");
        }

        public void SetMajorParryColor(Color newMajorParryColor)
        {
            majorParrySuccessColor = newMajorParryColor;
            LogDebug($"Major parry success color changed to: {newMajorParryColor}");
        }

        public void SetColorDuration(float newDuration)
        {
            colorDuration = newDuration;
            LogDebug($"Color duration changed to: {newDuration} seconds");
        }

        public void SetParryColorDuration(float newParryDuration)
        {
            parryColorDuration = newParryDuration;
            LogDebug($"Parry color duration changed to: {newParryDuration} seconds");
        }

        public void RefreshControllerDetection()
        {
            LogDebug("=== REFRESHING CONTROLLER DETECTION ===");
            DetectPlayStationController();

            // Try to set the default color to test connectivity
            if (dualShockController != null || dualSenseController != null)
            {
                LogDebug("Controller detected - testing with default color");
                SetLightbarColor(defaultColor);
            }
        }

        public void ForceColor(Color color)
        {
            SetLightbarColor(color);
            LogDebug($"Forced lightbar color to: {color}");
        }

        [ContextMenu("Test Minor Attack Color")]
        void TestMinorAttackColor()
        {
            SetAttackColor(minorAttackColor);
        }

        [ContextMenu("Test Major Attack Color")]
        void TestMajorAttackColor()
        {
            SetAttackColor(majorAttackColor);
        }

        [ContextMenu("Debug: Trigger Minor Parry (Key 1)")]
        void DebugTriggerMinorParry()
        {
            TriggerDebugMinorParry();
        }

        [ContextMenu("Debug: Trigger Major Parry (Key 2)")]
        void DebugTriggerMajorParry()
        {
            TriggerDebugMajorParry();
        }

        [ContextMenu("Force PS4-Only Mode")]
        void ForcePS4OnlyMode()
        {
            LogDebug("=== FORCING PS4-ONLY MODE ===");
            forcePS4Only = true;
            disablePS5Support = true;

            // Force refresh with new settings
            RefreshControllerDetection();

            // Test with obvious color
            if (dualShockController != null)
            {
                LogDebug("Testing PS4-only mode with bright red...");
                SetLightbarColor(Color.red);
            }
        }

        [ContextMenu("Enable Both Controllers")]
        void EnableBothControllers()
        {
            LogDebug("=== ENABLING BOTH CONTROLLERS ===");
            forcePS4Only = false;
            disablePS5Support = false;

            RefreshControllerDetection();
        }

        [ContextMenu("PS4 State Investigation")]
        void PS4StateInvestigation()
        {
            if (dualShockController != null)
            {
                LogDebug("=== PS4 STATE INVESTIGATION ===");
                LogDebug($"Controller Name: {dualShockController.name}");
                LogDebug($"Controller Description: {dualShockController.description}");
                LogDebug($"Controller Enabled: {dualShockController.enabled}");
                LogDebug($"Controller Added: {dualShockController.added}");
                LogDebug($"Controller Device ID: {dualShockController.deviceId}");
                LogDebug($"Is Lightbar Active: {isLightbarActive}");
                LogDebug($"Color Timer: {colorTimer}");
                LogDebug($"Current Active Color: {currentActiveColor}");
                LogDebug($"Enable Realtime Testing: {enableRealtimeColorTesting}");

                // Test immediate color change
                LogDebug("Testing immediate color change...");
                dualShockController.SetLightBarColor(Color.red);

                StartCoroutine(PS4StateTestSequence());
            }
            else
            {
                LogDebug("No PS4 controller detected for state investigation");
            }
        }

        System.Collections.IEnumerator PS4StateTestSequence()
        {
            yield return new WaitForSeconds(1f);

            LogDebug("Testing color during lightbar active state...");
            isLightbarActive = true;
            colorTimer = 5f;
            dualShockController.SetLightBarColor(Color.green);

            yield return new WaitForSeconds(1f);

            LogDebug("Testing color during lightbar inactive state...");
            isLightbarActive = false;
            colorTimer = 0f;
            dualShockController.SetLightBarColor(Color.blue);

            yield return new WaitForSeconds(1f);

            LogDebug("PS4 state investigation completed");
            dualShockController.SetLightBarColor(defaultColor);
        }

        [ContextMenu("PS4 Extreme Color Test")]
        void PS4ExtremeColorTest()
        {
            if (dualShockController != null)
            {
                LogDebug("=== PS4 EXTREME COLOR TEST ===");
                StartCoroutine(PS4ExtremeTestSequence());
            }
            else
            {
                LogDebug("No PS4 controller for extreme test");
            }
        }

        System.Collections.IEnumerator PS4ExtremeTestSequence()
        {
            // Test with very obvious, bright colors
            Color[] extremeColors = {
                Color.white,           // Maximum brightness
                Color.red,             // Pure red
                Color.green,           // Pure green  
                Color.blue,            // Pure blue
                Color.magenta,         // Bright magenta
                Color.cyan,            // Bright cyan
                new Color(1f, 0f, 1f, 1f),  // Purple max
                new Color(1f, 1f, 0f, 1f),  // Yellow max
                minorParrySuccessColor,      // Original minor
                majorParrySuccessColor       // Original major
            };

            foreach (Color color in extremeColors)
            {
                LogDebug($"PS4 EXTREME TEST - Setting to: {color}");

                // Multiple rapid-fire attempts
                for (int i = 0; i < 5; i++)
                {
                    dualShockController.SetLightBarColor(color);
                }

                yield return new WaitForSeconds(1.5f);
            }

            // Return to default
            dualShockController.SetLightBarColor(defaultColor);
            LogDebug("PS4 extreme test completed");
        }

        [ContextMenu("Force PS4 Detection & Test")]
        void ForcePS4DetectionAndTest()
        {
            LogDebug("=== FORCE PS4 DETECTION & TEST ===");

            // Force refresh detection
            RefreshControllerDetection();

            // Wait a frame then test
            StartCoroutine(DelayedPS4Test());
        }

        System.Collections.IEnumerator DelayedPS4Test()
        {
            yield return null; // Wait one frame

            if (dualShockController != null)
            {
                LogDebug("PS4 Controller found - testing colors...");

                // Test sequence of colors
                Color[] testColors = { Color.red, Color.green, Color.blue, defaultColor };

                foreach (Color color in testColors)
                {
                    LogDebug($"Setting PS4 to: {color}");
                    dualShockController.SetLightBarColor(color);
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                LogDebug("No PS4 controller detected in delayed test");
            }
        }

        [ContextMenu("Force PS4 Parry Test")]
        void ForcePS4ParryTest()
        {
            if (dualShockController != null)
            {
                LogDebug("=== FORCE PS4 PARRY TEST ===");
                Color testColor = minorParrySuccessColor;
                LogDebug($"Testing PS4 with color: {testColor}");

                try
                {
                    dualShockController.SetLightBarColor(testColor);
                    LogDebug("PS4 parry color test SUCCESS!");
                }
                catch (System.Exception e)
                {
                    LogDebug($"PS4 parry color test FAILED: {e.Message}");
                }
            }
            else
            {
                LogDebug("No PS4 controller detected for testing");
            }
        }

        [ContextMenu("Force Color Refresh PS4")]
        void ForcePS4ColorRefresh()
        {
            if (dualShockController != null)
            {
                LogDebug("=== FORCE PS4 COLOR REFRESH ===");

                // Try setting multiple colors in sequence
                Color[] testColors = { Color.red, Color.green, Color.blue, minorParrySuccessColor, majorParrySuccessColor };

                StartCoroutine(PS4ColorTestSequence(testColors));
            }
        }

        System.Collections.IEnumerator PS4ColorTestSequence(Color[] colors)
        {
            foreach (Color color in colors)
            {
                LogDebug($"Testing PS4 color: {color}");
                dualShockController.SetLightBarColor(color);
                yield return new WaitForSeconds(1f);
            }

            // Return to default
            dualShockController.SetLightBarColor(defaultColor);
            LogDebug("PS4 color test sequence completed");
        }

        [ContextMenu("Reset to Default Color")]
        void TestDefaultColor()
        {
            ReturnToDefaultColor();
        }

        [ContextMenu("Refresh Controller Detection")]
        void TestRefreshController()
        {
            RefreshControllerDetection();
        }

        void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"<color=#FF69B4>[PlayStation Lightbar] {message}</color>");
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying && enableRealtimeColorTesting)
            {
                if (debugTestColor != previousDebugColor)
                {
                    SetLightbarColor(debugTestColor);
                    previousDebugColor = debugTestColor;
                }
            }
        }
    }
}