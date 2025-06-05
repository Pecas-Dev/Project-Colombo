using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace ProjectColombo.UI
{
    public class MainMenuReturner : MonoBehaviour
    {
        [Header("Gamepad Hold Settings")]
        [Tooltip("How long to hold the south button (A/X) to return to main menu")]
        [SerializeField] float holdDurationRequired = 5f;

        [Tooltip("Enable gamepad hold functionality")]
        [SerializeField] bool enableGamepadHold = true;

        [Header("Timeout Settings")]
        [Tooltip("Time in seconds before automatically returning to main menu")]
        [SerializeField] float timeoutDuration = 75f;

        [Tooltip("Enable automatic timeout")]
        [SerializeField] bool enableTimeout = true;

        [Header("Main Menu Settings")]
        [Tooltip("Scene index of the main menu (usually 0)")]
        [SerializeField] int mainMenuSceneIndex = 0;

        [Tooltip("Scene name of the main menu (alternative to scene index)")]
        [SerializeField] string mainMenuSceneName = "";

        [Header("UI References")]
        [Tooltip("Glyph image showing the button to press")]
        [SerializeField] Image glyphImage;

        [Tooltip("Radial fill image showing hold progress")]
        [SerializeField] Image radialFillImage;

        [Tooltip("Text component for displaying instructions")]
        [SerializeField] TextMeshProUGUI instructionText;

        [Tooltip("Parent container for all UI elements (optional)")]
        [SerializeField] GameObject uiContainer;

        [Header("UI Animation Settings")]
        [Tooltip("Time before UI elements fade in")]
        [SerializeField] float uiFadeInDelay = 10f;

        [Tooltip("Duration of fade in animation")]
        [SerializeField] float fadeInDuration = 1f;

        [Tooltip("Blink speed for glyph and text")]
        [SerializeField] float blinkSpeed = 1f;

        [Tooltip("Minimum alpha during blink")]
        [SerializeField] float blinkMinAlpha = 0.3f;

        [Tooltip("Maximum alpha during blink")]
        [SerializeField] float blinkMaxAlpha = 1f;

        [Header("Fill Animation Settings")]
        [Tooltip("Speed at which fill decreases when button is released")]
        [SerializeField] float fillDecreaseSpeed = 0.5f;

        [Tooltip("Delay before fill starts decreasing after button release")]
        [SerializeField] float fillDecreaseDelay = 0.2f;

        [Header("Debug Settings")]
        [Tooltip("Enable debug logs")]
        [SerializeField] bool enableDebugLogs = true;

        [Tooltip("Show remaining time in console")]
        [SerializeField] bool showRemainingTime = false;

        [Header("Excluded Scenes")]
        [Tooltip("Scene names where this script should not work")]
        [SerializeField] string[] excludedScenes = { "MainMenu", "00_MainMenu" };

        // Core timing variables
        float currentHoldTime = 0f;
        float currentTimeoutTime = 0f;
        float persistentHoldProgress = 0f;
        bool isHoldingButton = false;
        bool hasTriggeredReturn = false;

        bool uiElementsVisible = false;
        bool isBlinking = false;
        Coroutine fadeInCoroutine;
        Coroutine blinkCoroutine;
        Coroutine fillDecreaseCoroutine;

        float originalGlyphAlpha = 1f;
        float originalTextAlpha = 1f;
        float originalFillAlpha = 1f;

        #region Unity Lifecycle

        void Start()
        {
            if (ShouldBeDisabled())
            {
                LogDebug("MainMenuReturner disabled in excluded scene");
                enabled = false;
                return;
            }

            InitializeUI();

            currentTimeoutTime = 0f;
            currentHoldTime = 0f;
            persistentHoldProgress = 0f;
            hasTriggeredReturn = false;

            LogDebug($"MainMenuReturner initialized - Hold: {holdDurationRequired}s, Timeout: {timeoutDuration}s");
        }

        void Update()
        {
            if (hasTriggeredReturn)
            {
                return;
            }

            UpdateGamepadHold();
            UpdateTimeout();
            UpdateUIVisibility();
        }

        #endregion

        #region UI Initialization

        void InitializeUI()
        {
            if (glyphImage != null)
            {
                originalGlyphAlpha = glyphImage.color.a;
                SetImageAlpha(glyphImage, 0f);
            }

            if (radialFillImage != null)
            {
                originalFillAlpha = radialFillImage.color.a;
                SetImageAlpha(radialFillImage, 0f);
                radialFillImage.fillAmount = 0f;
                radialFillImage.type = Image.Type.Filled;
                radialFillImage.fillMethod = Image.FillMethod.Radial360;
            }

            if (instructionText != null)
            {
                originalTextAlpha = instructionText.color.a;
                SetTextAlpha(instructionText, 0f);
            }

            if (uiContainer != null)
            {
                uiContainer.SetActive(false);
            }

            LogDebug("UI initialized and hidden");
        }

        void UpdateUIVisibility()
        {
            if (!uiElementsVisible && currentTimeoutTime >= uiFadeInDelay)
            {
                ShowUI();
            }
        }

        void ShowUI()
        {
            if (uiElementsVisible)
            {
                return;
            }

            uiElementsVisible = true;

            if (uiContainer != null)
            {
                uiContainer.SetActive(true);
            }

            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
            }

            fadeInCoroutine = StartCoroutine(FadeInUI());
            LogDebug("Starting UI fade in");
        }

        IEnumerator FadeInUI()
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / fadeInDuration;

                if (glyphImage != null)
                {
                    SetImageAlpha(glyphImage, Mathf.Lerp(0f, originalGlyphAlpha, progress));
                }

                if (radialFillImage != null)
                {
                    SetImageAlpha(radialFillImage, Mathf.Lerp(0f, originalFillAlpha, progress));
                }

                if (instructionText != null)
                {
                    SetTextAlpha(instructionText, Mathf.Lerp(0f, originalTextAlpha, progress));
                }

                yield return null;
            }

            if (glyphImage != null)
            {
                SetImageAlpha(glyphImage, originalGlyphAlpha);
            }

            if (radialFillImage != null)
            {
                SetImageAlpha(radialFillImage, originalFillAlpha);
            }

            if (instructionText != null)
            {
                SetTextAlpha(instructionText, originalTextAlpha);
            }

            StartBlinking();
            LogDebug("UI fade in complete, starting blink animation");
        }

        void StartBlinking()
        {
            if (isBlinking)
            {
                return;
            }

            isBlinking = true;

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }

            blinkCoroutine = StartCoroutine(BlinkAnimation());
        }

        IEnumerator BlinkAnimation()
        {
            while (isBlinking && !hasTriggeredReturn)
            {
                float elapsedTime = 0f;
                float blinkDuration = 1f / blinkSpeed;

                while (elapsedTime < blinkDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = elapsedTime / blinkDuration;
                    float alpha = Mathf.Lerp(blinkMaxAlpha, blinkMinAlpha, progress);

                    if (glyphImage != null)
                    {
                        SetImageAlpha(glyphImage, alpha);
                    }

                    if (instructionText != null)
                    {
                        SetTextAlpha(instructionText, alpha);
                    }

                    yield return null;
                }

                elapsedTime = 0f;
                while (elapsedTime < blinkDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = elapsedTime / blinkDuration;
                    float alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, progress);

                    if (glyphImage != null)
                    {
                        SetImageAlpha(glyphImage, alpha);
                    }

                    if (instructionText != null)
                    {
                        SetTextAlpha(instructionText, alpha);
                    }

                    yield return null;
                }
            }
        }

        #endregion

        #region Gamepad Hold Logic

        void UpdateGamepadHold()
        {
            if (!enableGamepadHold)
            {
                return;
            }

            bool southButtonPressed = IsGamepadSouthButtonPressed();

            if (southButtonPressed)
            {
                if (!isHoldingButton)
                {
                    isHoldingButton = true;

                    if (fillDecreaseCoroutine != null)
                    {
                        StopCoroutine(fillDecreaseCoroutine);
                        fillDecreaseCoroutine = null;
                    }
                    LogDebug($"Started holding gamepad south button from progress: {persistentHoldProgress:F2}");
                }

                float progressIncrement = Time.unscaledDeltaTime / holdDurationRequired;
                persistentHoldProgress = Mathf.Min(1f, persistentHoldProgress + progressIncrement);

                UpdateRadialFill();

                if (showRemainingTime && Time.frameCount % 30 == 0)
                {
                    float remainingTime = holdDurationRequired * (1f - persistentHoldProgress);
                    LogDebug($"Hold progress: {persistentHoldProgress:F2} (Remaining: {remainingTime:F1}s)");
                }

                if (persistentHoldProgress >= 1f)
                {
                    LogDebug("Gamepad hold duration reached - returning to main menu");
                    ReturnToMainMenu();
                }
            }
            else
            {
                if (isHoldingButton)
                {
                    isHoldingButton = false;
                    LogDebug($"Released gamepad south button at progress: {persistentHoldProgress:F2}");

                    if (fillDecreaseCoroutine != null)
                    {
                        StopCoroutine(fillDecreaseCoroutine);
                    }
                    fillDecreaseCoroutine = StartCoroutine(DecreaseHoldProgress());
                }
            }
        }

        IEnumerator DecreaseHoldProgress()
        {
            yield return new WaitForSecondsRealtime(fillDecreaseDelay);

            while (persistentHoldProgress > 0f && !isHoldingButton)
            {
                float decreaseAmount = fillDecreaseSpeed * Time.unscaledDeltaTime;
                persistentHoldProgress = Mathf.Max(0f, persistentHoldProgress - decreaseAmount);

                UpdateRadialFill();

                yield return null;
            }

            fillDecreaseCoroutine = null;
        }

        void UpdateRadialFill()
        {
            if (radialFillImage != null && uiElementsVisible)
            {
                radialFillImage.fillAmount = persistentHoldProgress;
            }
        }

        bool IsGamepadSouthButtonPressed()
        {
            Gamepad gamepad = Gamepad.current;

            bool gamepadPressed = gamepad != null && gamepad.aButton.isPressed;

            Keyboard keyboard = Keyboard.current;
            bool keyboardPressed = keyboard != null && (keyboard.rKey.isPressed || keyboard.enterKey.isPressed);

            return gamepadPressed || keyboardPressed;
        }

        #endregion

        #region Timeout Logic

        void UpdateTimeout()
        {
            if (!enableTimeout)
            {
                return;
            }

            currentTimeoutTime += Time.unscaledDeltaTime;

            if (showRemainingTime && Time.frameCount % 300 == 0)
            {
                float remainingTime = timeoutDuration - currentTimeoutTime;
                LogDebug($"Timeout progress: {currentTimeoutTime:F0}s / {timeoutDuration}s (Remaining: {remainingTime:F0}s)");
            }

            if (currentTimeoutTime >= timeoutDuration)
            {
                LogDebug("Timeout duration reached - returning to main menu");
                ReturnToMainMenu();
            }
        }

        #endregion

        #region Utility Methods

        void SetImageAlpha(Image image, float alpha)
        {
            if (image == null) return;

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        void SetTextAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text == null) return;

            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }

        #endregion

        #region Scene Management

        void ReturnToMainMenu()
        {
            if (hasTriggeredReturn)
            {
                return;
            }

            hasTriggeredReturn = true;

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (fillDecreaseCoroutine != null)
            {
                StopCoroutine(fillDecreaseCoroutine);
                fillDecreaseCoroutine = null;
            }

            LogDebug("Returning to main menu...");

            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                LogDebug($"Loading main menu by name: {mainMenuSceneName}");
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                LogDebug($"Loading main menu by index: {mainMenuSceneIndex}");
                SceneManager.LoadScene(mainMenuSceneIndex);
            }
        }

        bool ShouldBeDisabled()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            foreach (string excludedScene in excludedScenes)
            {
                if (currentSceneName.Equals(excludedScene, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Public Methods
        public void ResetTimers()
        {
            currentHoldTime = 0f;
            currentTimeoutTime = 0f;
            persistentHoldProgress = 0f;
            isHoldingButton = false;
            hasTriggeredReturn = false;

            if (radialFillImage != null)
            {
                radialFillImage.fillAmount = 0f;
            }

            LogDebug("All timers and progress reset");
        }

        public void ResetTimeoutTimer()
        {
            currentTimeoutTime = 0f;
            LogDebug("Timeout timer reset");
        }

        public void ResetHoldProgress()
        {
            persistentHoldProgress = 0f;
            isHoldingButton = false;

            if (radialFillImage != null)
            {
                radialFillImage.fillAmount = 0f;
            }

            LogDebug("Hold progress reset");
        }

        public void ForceReturnToMainMenu()
        {
            LogDebug("Force returning to main menu");
            ReturnToMainMenu();
        }

        public float GetHoldProgress()
        {
            return persistentHoldProgress;
        }

        public float GetTimeoutProgress()
        {
            return Mathf.Clamp01(currentTimeoutTime / timeoutDuration);
        }

        public void ForceShowUI()
        {
            ShowUI();
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF9900>[MainMenuReturner] {message}</color>");
            }
        }

        #endregion

        #region Editor Utilities

#if UNITY_EDITOR
        [Header("Editor Testing")]
        [SerializeField] bool testInEditor = false;

        void OnValidate()
        {
            if (holdDurationRequired <= 0f)
            {
                holdDurationRequired = 5f;
            }

            if (timeoutDuration <= 0f)
            {
                timeoutDuration = 75f;
            }

            if (mainMenuSceneIndex < 0)
            {
                mainMenuSceneIndex = 0;
            }

            if (uiFadeInDelay < 0f)
            {
                uiFadeInDelay = 10f;
            }

            if (fadeInDuration <= 0f)
            {
                fadeInDuration = 1f;
            }

            if (blinkSpeed <= 0f)
            {
                blinkSpeed = 1f;
            }
        }

        [ContextMenu("Test Show UI")]
        void TestShowUI()
        {
            if (Application.isPlaying)
            {
                ForceShowUI();
            }
        }

        [ContextMenu("Test Return to Main Menu")]
        void TestReturnToMainMenu()
        {
            if (Application.isPlaying)
            {
                ForceReturnToMainMenu();
            }
        }

        [ContextMenu("Reset All")]
        void TestResetAll()
        {
            if (Application.isPlaying)
            {
                ResetTimers();
            }
        }
#endif

        #endregion
    }
}