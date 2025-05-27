using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;
using UnityEngine.SceneManagement;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuSettingsTabController : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button returnToHubButton;
        [SerializeField] Button exitButton;

        [Header("Visual Elements")]
        [SerializeField] Image[] buttonClefImages;
        [SerializeField] TextMeshProUGUI[] buttonTexts;

        [Header("Text Animation")]
        [SerializeField] float selectedMinFontSize = 75f;
        [SerializeField] float selectedMaxFontSize = 105f;
        [SerializeField] float defaultMinFontSize = 70f;
        [SerializeField] float defaultMaxFontSize = 100f;
        [SerializeField] float animationDuration = 0.3f;

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        int currentSelectedIndex = -1;

        bool isInitialized = false;

        Button[] allButtons;

        IEnumerator[] textAnimationCoroutines;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void OnEnable()
        {
            StartCoroutine(DelayedInitialSelection());

            if (allButtons != null && allButtons.Length > 0)
            {
                SelectButton(0);
            }
        }

        void Update()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                for (int i = 0; i < allButtons.Length; i++)
                {
                    if (EventSystem.current.currentSelectedGameObject == allButtons[i].gameObject && i != currentSelectedIndex)
                    {
                        SelectButton(i);
                        break;
                    }
                }
            }
        }

        public void Initialize()
        {
            LogDebug("Initializing settings tab controller");

            allButtons = new Button[] { resumeButton, optionsButton, returnToHubButton, exitButton };

            textAnimationCoroutines = new System.Collections.IEnumerator[allButtons.Length];

            if (buttonClefImages != null)
            {
                foreach (var clef in buttonClefImages)
                {
                    if (clef != null)
                    {
                        clef.gameObject.SetActive(false);
                    }
                }
            }

            if (buttonTexts != null)
            {
                foreach (var text in buttonTexts)
                {
                    if (text != null)
                    {
                        text.enableAutoSizing = true;
                        text.fontSizeMin = defaultMinFontSize;
                        text.fontSizeMax = defaultMaxFontSize;
                    }
                }
            }

            SetupButtonActions();

            for (int i = 0; i < allButtons.Length; i++)
            {
                if (allButtons[i] == null) continue;

                int buttonIndex = i;

                EventTrigger eventTrigger = allButtons[i].GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = allButtons[i].gameObject.AddComponent<EventTrigger>();
                }

                eventTrigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Select
                };
                selectEntry.callback.AddListener((data) => SelectButton(buttonIndex));
                eventTrigger.triggers.Add(selectEntry);

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnterEntry.callback.AddListener((data) => SelectButton(buttonIndex));
                eventTrigger.triggers.Add(pointerEnterEntry);
            }

            isInitialized = true;
        }

        void SetupButtonActions()
        {
            // Resume button
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(() =>
                {
                    LogDebug("Resume button clicked");
                    GameManager.Instance.ResumeGame();
                });
            }

            // Options button
            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveAllListeners();
                optionsButton.onClick.AddListener(() =>
                {
                    LogDebug("Options button clicked");
                    // TODO: Implement options menu functionality
                });
            }

            // Return to Hub button - FIXED VERSION
            if (returnToHubButton != null)
            {
                returnToHubButton.onClick.RemoveAllListeners();
                returnToHubButton.onClick.AddListener(() =>
                {
                    LogDebug("Return to Hub button clicked");
                    StartCoroutine(ReturnToHubSequence());
                });
            }

            // Exit button
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() =>
                {
                    LogDebug("Exit button clicked");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }
        }

        IEnumerator ReturnToHubSequence()
        {
            LogDebug("Starting return to hub sequence");

            GameManager gameManager = GameManager.Instance;

            gameManager.PlayCloseTransition();
            LogDebug("Playing close transition");

            yield return new WaitForSecondsRealtime(2.5f);

            SceneManager.LoadScene("00_MainMenu");

        }

        public void SelectButton(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= allButtons.Length || allButtons[buttonIndex] == null)
            {
                LogDebug($"Invalid button index: {buttonIndex}");
                return;
            }

            if (buttonIndex == currentSelectedIndex)
            {
                return;
            }

            LogDebug($"Selecting button {buttonIndex}");

            if (currentSelectedIndex >= 0 && currentSelectedIndex < allButtons.Length)
            {
                if (buttonClefImages != null && currentSelectedIndex < buttonClefImages.Length && buttonClefImages[currentSelectedIndex] != null)
                {
                    buttonClefImages[currentSelectedIndex].gameObject.SetActive(false);
                }

                if (buttonTexts != null && currentSelectedIndex < buttonTexts.Length && buttonTexts[currentSelectedIndex] != null)
                {
                    if (textAnimationCoroutines[currentSelectedIndex] != null)
                    {
                        StopCoroutine(textAnimationCoroutines[currentSelectedIndex]);
                    }

                    textAnimationCoroutines[currentSelectedIndex] = AnimateTextSize(buttonTexts[currentSelectedIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration);

                    StartCoroutine(textAnimationCoroutines[currentSelectedIndex]);
                }
            }

            currentSelectedIndex = buttonIndex;

            if (currentSelectedIndex >= 0 && currentSelectedIndex < allButtons.Length)
            {
                if (buttonClefImages != null && currentSelectedIndex < buttonClefImages.Length && buttonClefImages[currentSelectedIndex] != null)
                {
                    buttonClefImages[currentSelectedIndex].gameObject.SetActive(true);
                }

                if (buttonTexts != null && currentSelectedIndex < buttonTexts.Length && buttonTexts[currentSelectedIndex] != null)
                {
                    if (textAnimationCoroutines[currentSelectedIndex] != null)
                    {
                        StopCoroutine(textAnimationCoroutines[currentSelectedIndex]);
                    }

                    textAnimationCoroutines[currentSelectedIndex] = AnimateTextSize(buttonTexts[currentSelectedIndex], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration);

                    StartCoroutine(textAnimationCoroutines[currentSelectedIndex]);
                }
            }

            EventSystem.current.SetSelectedGameObject(allButtons[buttonIndex].gameObject);
        }

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();

            if (currentSelectedIndex < 0)
            {
                SelectButton(0);
            }
            else if (currentSelectedIndex >= 0)
            {
                SelectButton(currentSelectedIndex);
            }
        }
        IEnumerator AnimateTextSize(TextMeshProUGUI text, float startMin, float startMax, float endMin, float endMax, float duration)
        {
            if (text == null) yield break;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float smoothT = t * t * (3f - 2f * t);

                float currentMin = Mathf.Lerp(startMin, endMin, smoothT);
                float currentMax = Mathf.Lerp(startMax, endMax, smoothT);

                text.fontSizeMin = currentMin;
                text.fontSizeMax = currentMax;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            text.fontSizeMin = endMin;
            text.fontSizeMax = endMax;
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FFAA00>[SettingsTabController] {message}</color>");
            }
        }
    }
}