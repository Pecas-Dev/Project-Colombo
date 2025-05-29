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
        [SerializeField] Color minorAttackColor = new Color(0.5f, 0f, 1f, 1f); 
        [SerializeField] Color majorAttackColor = Color.yellow;

        [SerializeField] Color minorParryColor = new Color(0.5f, 0f, 1f, 1f); 
        [SerializeField] Color majorParryColor = Color.yellow;

        [Header("Lightbar Settings")]
        [SerializeField] float colorDuration = 0.5f;
        [SerializeField] bool debugMode = false;

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
            CustomEvents.OnMinorParryPerformed += HandleMinorParryPerformed;
            CustomEvents.OnMajorParryPerformed += HandleMajorParryPerformed;
            CustomEvents.OnLightbarColorChangeRequested += HandleColorChangeRequest;
        }

        void OnDisable()
        {
            CustomEvents.OnMinorAttackPerformed -= HandleMinorAttackPerformed;
            CustomEvents.OnMajorAttackPerformed -= HandleMajorAttackPerformed;
            CustomEvents.OnMinorParryPerformed -= HandleMinorParryPerformed;
            CustomEvents.OnMajorParryPerformed -= HandleMajorParryPerformed;
            CustomEvents.OnLightbarColorChangeRequested -= HandleColorChangeRequest;
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
            if (Time.frameCount % 60 == 0) 
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
            else if (gameInputSO.GetInputPressed(PlayerInputAction.MinorParry))
            {
                SetAttackColor(minorParryColor);
                LogDebug("Minor parry input detected - lightbar set to purple");
            }
            else if (gameInputSO.GetInputPressed(PlayerInputAction.MajorParry))
            {
                SetAttackColor(majorParryColor);
                LogDebug("Major parry input detected - lightbar set to yellow");
            }
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
        void HandleMinorParryPerformed(GameGlobals.MusicScale scale)
        {
            SetAttackColor(minorParryColor);
            LogDebug("Minor parry event received - lightbar set to purple");
        }

        void HandleMajorParryPerformed(GameGlobals.MusicScale scale)
        {
            SetAttackColor(majorParryColor);
            LogDebug("Major parry event received - lightbar set to yellow");
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
                    dualShockController.SetLightBarColor(color);
                    colorSet = true;
                    LogDebug($"DualShock 4 lightbar color set to: {color}");

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
            minorParryColor = newMinorParryColor;
            LogDebug($"Minor parry color changed to: {newMinorParryColor}");
        }

        public void SetMajorParryColor(Color newMajorParryColor)
        {
            majorParryColor = newMajorParryColor;
            LogDebug($"Major parry color changed to: {newMajorParryColor}");
        }


        public void SetColorDuration(float newDuration)
        {
            colorDuration = newDuration;
            LogDebug($"Color duration changed to: {newDuration} seconds");
        }


        public void RefreshControllerDetection()
        {
            LogDebug("=== REFRESHING CONTROLLER DETECTION ===");
            DetectPlayStationController();

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

        [ContextMenu("Test Minor Parry Color")]
        void TestMinorParryColor()
        {
            SetAttackColor(minorParryColor);
        }

        [ContextMenu("Test Major Parry Color")]
        void TestMajorParryColor()
        {
            SetAttackColor(majorParryColor);
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