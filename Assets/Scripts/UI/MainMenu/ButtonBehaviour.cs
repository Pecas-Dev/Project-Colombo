using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class ButtonBehaviour : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image[] clefImages;

    [Header("Buttons")]
    [SerializeField] Button[] buttons;

    [Header("Text Components")]
    [SerializeField] TextMeshProUGUI[] buttonTexts;

    [Header("Text Size Animation")]
    [SerializeField] float selectedMinFontSize = 75f;
    [SerializeField] float selectedMaxFontSize = 105f;
    [SerializeField] float defaultMinFontSize = 70f;
    [SerializeField] float defaultMaxFontSize = 100f;
    [SerializeField] float growAnimationDuration = 0.3f;
    [SerializeField] float shrinkAnimationDuration = 0.3f;


    int currentSelectedIndex = -1;


    Coroutine[] sizeAnimationCoroutines;


    void Start()
    {
        if (buttons.Length != clefImages.Length)
        {
            Debug.LogWarning("Number of buttons does not match number of images.");
        }

        if (buttons.Length != buttonTexts.Length)
        {
            Debug.LogWarning("Number of buttons does not match number of text components.");
        }

        sizeAnimationCoroutines = new Coroutine[buttonTexts.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                int index = i;

                buttons[i].onClick.AddListener(() => SelectButton(index));

                EventTrigger eventTrigger = buttons[i].gameObject.GetComponent<EventTrigger>();

                if (eventTrigger == null)
                {
                    eventTrigger = buttons[i].gameObject.AddComponent<EventTrigger>();
                }

                AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, (data) => { SelectButton(index); });
                AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) => { SelectButton(index); });
            }
        }

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            if (buttonTexts[i] != null)
            {
                SetTextToDefaultSize(buttonTexts[i]);
            }
        }

        DeactivateAllImages();

        if (buttons.Length > 0 && buttons[0] != null)
        {
            SelectButton(0);
        }
    }

    private void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
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

            if (currentSelectedIndex < buttonTexts.Length && buttonTexts[currentSelectedIndex] != null)
            {
                if (sizeAnimationCoroutines[currentSelectedIndex] != null)
                {
                    StopCoroutine(sizeAnimationCoroutines[currentSelectedIndex]);
                }

                sizeAnimationCoroutines[currentSelectedIndex] = StartCoroutine(AnimateTextSize(buttonTexts[currentSelectedIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, shrinkAnimationDuration));
            }
        }

        currentSelectedIndex = index;

        if (index < clefImages.Length && clefImages[index] != null)
        {
            clefImages[index].gameObject.SetActive(true);
        }

        if (index < buttonTexts.Length && buttonTexts[index] != null)
        {
            if (sizeAnimationCoroutines[index] != null)
            {
                StopCoroutine(sizeAnimationCoroutines[index]);
            }

            sizeAnimationCoroutines[index] = StartCoroutine(AnimateTextSize(buttonTexts[index], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, growAnimationDuration));
        }
    }

    IEnumerator AnimateTextSize(TextMeshProUGUI text, float startMin, float startMax, float endMin, float endMax, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            float smoothT = t * t * (3f - 2f * t);

            float currentMin = Mathf.Lerp(startMin, endMin, smoothT);
            float currentMax = Mathf.Lerp(startMax, endMax, smoothT);

            text.enableAutoSizing = true;
            text.fontSizeMin = currentMin;
            text.fontSizeMax = currentMax;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        text.fontSizeMin = endMin;
        text.fontSizeMax = endMax;
    }

    void SetTextToDefaultSize(TextMeshProUGUI text)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = defaultMinFontSize;
        text.fontSizeMax = defaultMaxFontSize;
    }

    void DeactivateAllImages()
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
        if (index >= 0 && index < buttons.Length)
        {
            SelectButton(index);

            if (buttons[index] != null)
            {
                EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
            }
        }
    }
}