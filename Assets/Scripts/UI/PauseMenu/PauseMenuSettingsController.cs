using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectColombo.GameManagement;

namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuSettingsController : MenuController
    {
        [Header("Settings UI Elements")]
        [SerializeField] Image[] clefImages;
        [SerializeField] Button[] settingsButtons;
        [SerializeField] TextMeshProUGUI[] settingsButtonTexts;
        [SerializeField] GameObject settingsPanel;


        int currentSelectedIndex = -1;

        Coroutine[] sizeAnimationCoroutines;

        public override void Initialize()
        {
            base.Initialize();

            if (settingsButtons.Length != clefImages.Length)
            {
                Debug.LogWarning("Number of settings buttons does not match number of clef images.");
            }

            if (settingsButtons.Length != settingsButtonTexts.Length)
            {
                Debug.LogWarning("Number of settings buttons does not match number of text components.");
            }

            sizeAnimationCoroutines = new Coroutine[settingsButtonTexts.Length];

            for (int i = 0; i < settingsButtons.Length; i++)
            {
                if (settingsButtons[i] != null)
                {
                    int index = i;

                    settingsButtons[i].onClick.AddListener(() => SelectButton(index));

                    EventTrigger eventTrigger = settingsButtons[i].gameObject.GetComponent<EventTrigger>();

                    if (eventTrigger == null)
                    {
                        eventTrigger = settingsButtons[i].gameObject.AddComponent<EventTrigger>();
                    }

                    AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, (data) => { SelectButton(index); });
                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => { SelectButton(index); });
                }
            }

            for (int i = 0; i < settingsButtonTexts.Length; i++)
            {
                if (settingsButtonTexts[i] != null)
                {
                    SetTextToDefaultSize(settingsButtonTexts[i]);
                }
            }

            DeactivateAllClefImages();

            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (settingsButtons.Length > 0 && settingsButtons[0] != null)
            {
                SelectButton(0);

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(settingsButtons[0].gameObject);
                }
            }
        }

        public override void Show()
        {
            base.Show();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }

            if (currentSelectedIndex >= 0 && currentSelectedIndex < settingsButtons.Length)
            {
                EventSystem.current.SetSelectedGameObject(settingsButtons[currentSelectedIndex].gameObject);

                if (uiInputSwitcher == null)
                {
                    uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();
                }

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(settingsButtons[currentSelectedIndex].gameObject);
                }
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((data) => { action((BaseEventData)data); });
            trigger.triggers.Add(entry);
        }

        void SelectButton(int index)
        {
            if (currentSelectedIndex == index)
            {
                return;
            }

            if (currentSelectedIndex >= 0)
            {
                if (currentSelectedIndex < clefImages.Length && clefImages[currentSelectedIndex] != null)
                {
                    clefImages[currentSelectedIndex].gameObject.SetActive(false);
                }

                if (currentSelectedIndex < settingsButtonTexts.Length && settingsButtonTexts[currentSelectedIndex] != null)
                {
                    if (sizeAnimationCoroutines[currentSelectedIndex] != null)
                    {
                        StopCoroutine(sizeAnimationCoroutines[currentSelectedIndex]);
                    }

                    sizeAnimationCoroutines[currentSelectedIndex] = StartCoroutine(AnimateTextSize(settingsButtonTexts[currentSelectedIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));
                }
            }

            currentSelectedIndex = index;

            if (index < clefImages.Length && clefImages[index] != null)
            {
                clefImages[index].gameObject.SetActive(true);
            }

            if (index < settingsButtonTexts.Length && settingsButtonTexts[index] != null)
            {
                if (sizeAnimationCoroutines[index] != null)
                {
                    StopCoroutine(sizeAnimationCoroutines[index]);
                }

                sizeAnimationCoroutines[index] = StartCoroutine(AnimateTextSize(settingsButtonTexts[index], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));
            }

            if (settingsButtons[index] != null)
            {
                RectTransform rectTransform = settingsButtons[index].GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    PlayButtonClickAnimation(rectTransform);
                }
            }
        }

        void DeactivateAllClefImages()
        {
            foreach (Image image in clefImages)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(false);
                }
            }
        }

        public void SelectButtonByIndex(int index)
        {
            if (index >= 0 && index < settingsButtons.Length)
            {
                SelectButton(index);

                if (settingsButtons[index] != null)
                {
                    EventSystem.current.SetSelectedGameObject(settingsButtons[index].gameObject);
                }
            }
        }

        public override void HandleInput()
        {
            base.HandleInput();

            if (gameInputSO != null && gameInputSO.playerInputActions != null)
            {
                Vector2 navInput = gameInputSO.playerInputActions.UI.Navigate.ReadValue<Vector2>();

                if (navInput.y < -0.5f && lastNavDirection.y >= -0.5f)
                {
                    NavigateDown();
                }
                else if (navInput.y > 0.5f && lastNavDirection.y <= 0.5f)
                {
                    NavigateUp();
                }

                lastNavDirection = navInput;
            }
        }

        Vector2 lastNavDirection = Vector2.zero;

        void NavigateUp()
        {
            if (currentSelectedIndex > 0)
            {
                SelectButtonByIndex(currentSelectedIndex - 1);
            }
            else
            {
                SelectButtonByIndex(settingsButtons.Length - 1);
            }
        }

        void NavigateDown()
        {
            if (currentSelectedIndex < settingsButtons.Length - 1)
            {
                SelectButtonByIndex(currentSelectedIndex + 1);
            }
            else
            {
                SelectButtonByIndex(0);
            }
        }
    }
}