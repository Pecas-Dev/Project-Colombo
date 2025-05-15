using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TESTMaskScreenSelection : MonoBehaviour
{
    [SerializeField] private Button defaultButton;

    private void OnEnable()
    {
        SelectDefaultButton();
    }

    private void Start()
    {
        SelectDefaultButton();
    }

    public void SelectDefaultButton()
    {
        if (defaultButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            StartCoroutine(SelectButtonNextFrame());
        }
        else
        {
            if (defaultButton == null)
                Debug.LogWarning("Default button reference is missing!");

            if (EventSystem.current == null)
                Debug.LogWarning("No EventSystem found in the scene!");
        }
    }

    private System.Collections.IEnumerator SelectButtonNextFrame()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
    }

    public void SetDefaultButton(Button newDefaultButton)
    {
        defaultButton = newDefaultButton;
        SelectDefaultButton();
    }
}
