using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using TMPro;
using UnityEngine;
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

        bool isEnabled = false;
        int index;

        private void Start()
        {
            isEnabled = false;
            dialogCanvas.SetActive(false);
        }

        private void Update()
        {
            if (!isEnabled) return;

            if (GameManager.Instance.gameInput.inputActions.UI.Submit.WasPressedThisFrame())
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
            isEnabled = true;

            StartCoroutine(StopMoving());
            GameManager.Instance.gameInput.SwitchToUI();

            dialogCanvas.SetActive(true);
            HUDCanvas.SetActive(false);
            index = 0;
            textComponent.text = "";
            StartDialog();
        }

        IEnumerator StopMoving()
        {
            yield return new WaitForEndOfFrame();

            GameObject player = GameObject.Find("Player");
            player.GetComponent<PlayerStateMachine>().SwitchState(new PlayerMovementState(player.GetComponent<PlayerStateMachine>()));
        }

        public void DisableDialog()
        {
            isEnabled = false;
            GameManager.Instance.gameInput.SwitchToGameplay();

            onDialogComplete.Invoke();
            HUDCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}