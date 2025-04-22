using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ImageButtonBehaviour : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image[] clefImages;

    [Header("Buttons")]
    [SerializeField] Button[] buttons;


    int currentSelectedIndex = -1;


    void Start()
    {
        if (buttons.Length != clefImages.Length)
        {
            Debug.LogWarning("Number of buttons does not match number of images.");
        }

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

    private void SelectButton(int index)
    {
        if (currentSelectedIndex == index)
        {
            return;
        }

        if (currentSelectedIndex >= 0 && currentSelectedIndex < clefImages.Length && clefImages[currentSelectedIndex] != null)
        {
            clefImages[currentSelectedIndex].gameObject.SetActive(false);
        }

        currentSelectedIndex = index;

        if (index < clefImages.Length && clefImages[index] != null)
        {
            clefImages[index].gameObject.SetActive(true);
        }
    }

    private void DeactivateAllImages()
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
