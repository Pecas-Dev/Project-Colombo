using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace ProjectColombo.UI.Pausescreen
{

    public class PauseMenuSettingsController : MenuController
    {
        [Header("Settings Menu Elements")]
        [SerializeField] Button[] buttons;
        [SerializeField] TextMeshProUGUI[] buttonTexts;
        [SerializeField] Image[] clefImages;

        [Header("Debug")]
        [SerializeField] bool enableDebugLogs = false;

        int currentIndex = -1;

        Coroutine[] sizeCoroutines;

        bool initialised;

        public override void Initialize()
        {
            base.Initialize();

            if (buttons == null || buttons.Length == 0)
            {
                return;
            }

            sizeCoroutines = new Coroutine[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                int idx = i;

                if (!buttons[i])
                {
                    continue;
                }

                buttons[i].onClick.AddListener(() => SelectButton(idx));

                EventTrigger trigger = buttons[i].GetComponent<EventTrigger>() ?? buttons[i].gameObject.AddComponent<EventTrigger>();

                AddTrigger(trigger, EventTriggerType.Select, (d) => SelectButton(idx));
                AddTrigger(trigger, EventTriggerType.PointerEnter, (d) => SelectButton(idx));

                if (idx < buttonTexts.Length && buttonTexts[idx])
                {
                    SetTextToDefaultSize(buttonTexts[idx]);
                }
            }

            if (clefImages != null)
            {
                foreach (var images in clefImages)
                {
                    if (images)
                    {
                        images.gameObject.SetActive(false);
                    }
                }
            }



            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (buttons[0])
            {
                SelectButton(0);
            }

            initialised = true;
        }

        public override void Show()
        {
            base.Show();
            EnsureSelection();
        }

        void OnEnable()
        {
            if (!initialised)
            {
                return;
            }

            EnsureSelection();
        }

        void EnsureSelection()
        {
            if (buttons == null || buttons.Length == 0 || !buttons[0])
            {
                return;
            }

            if (EventSystem.current)
            {
                EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            }

            if (uiInputSwitcher)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
            }
        }

        void AddTrigger(EventTrigger _trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            _trigger.triggers.Add(entry);
        }

        void SelectButton(int idx)
        {
            if (currentIndex == idx)
            {
                return;
            }

            if (currentIndex >= 0)
            {
                if (currentIndex < clefImages?.Length && clefImages[currentIndex])
                {
                    clefImages[currentIndex].gameObject.SetActive(false);
                }

                if (currentIndex < buttonTexts?.Length && buttonTexts[currentIndex])
                {
                    if (sizeCoroutines[currentIndex] != null)
                    {
                        StopCoroutine(sizeCoroutines[currentIndex]);
                    }

                    sizeCoroutines[currentIndex] = StartCoroutine(AnimateTextSize(buttonTexts[currentIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));
                }
            }

            currentIndex = idx;

            if (idx < clefImages?.Length && clefImages[idx])
            {
                clefImages[idx].gameObject.SetActive(true);
            }

            if (idx < buttonTexts?.Length && buttonTexts[idx])
            {
                if (sizeCoroutines[idx] != null)
                {
                    StopCoroutine(sizeCoroutines[idx]);
                }

                sizeCoroutines[idx] = StartCoroutine(AnimateTextSize(buttonTexts[idx], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));
            }

            if (EventSystem.current && !EventSystem.current.alreadySelecting && EventSystem.current.currentSelectedGameObject != buttons[idx].gameObject)
            {
                EventSystem.current.SetSelectedGameObject(buttons[idx].gameObject);
            }

            if (uiInputSwitcher)
            {
                uiInputSwitcher.SetFirstSelectedButton(buttons[idx].gameObject);
            }

            Log($"Selected settings button {idx}");
        }

        void Log(string msg)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[PauseMenuSettings] {msg}</color>");
            }
        }
    }
}
