using UnityEngine;
using TMPro;
using System.Collections;
using ProjectColombo.GameManagement;
using UnityEngine.Events;

namespace ProjectColombo.UI
{
    public class TutorialDialogSystem : MonoBehaviour
    {
        public GameObject dialogCanvas;
        public TextMeshProUGUI textComponent;
        public GameObject HUDCanvas;
        public string[] lines;
        public float textSpeed;
        public UnityEvent onDialogComplete;

        int index;

        private void Start()
        {
            dialogCanvas.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance.gameInput.InteractPressed)
            {
                if (textComponent.text == lines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    textComponent.text = lines[index];
                }
            }
        }

        void StartDialog()
        {
            StartCoroutine(TypeLine());
        }

        IEnumerator TypeLine()
        {
            foreach (char c in lines[index].ToCharArray())
            {
                textComponent.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        void NextLine()
        {
            if (index < lines.Length - 1)
            {
                index++;
                textComponent.text = "";
                StartCoroutine(TypeLine());
            }
            else
            {
                DisableDialog();
            }
        }

        public void EnableDialog()
        {
            GameManager.Instance.gameInput.DisableAllInputsExcept(GameInputSystem.InputActionType.Interact);
            GameManager.Instance.gameInput.ResetMovementInput();
            dialogCanvas.SetActive(true);
            HUDCanvas.SetActive(false);
            index = 0;
            textComponent.text = "";
            StartDialog();
        }

        public void DisableDialog()
        {
            GameManager.Instance.gameInput.EnableAllInputs();
            onDialogComplete.Invoke();
            HUDCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}