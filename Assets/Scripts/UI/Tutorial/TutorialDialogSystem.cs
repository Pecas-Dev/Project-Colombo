using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.Tutorial;
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
        TutorialManager myTutorialManager;

        bool isEnabled = false;
        int index;

        private void Start()
        {
            isEnabled = false;
            myTutorialManager = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();
            dialogCanvas.SetActive(false);
        }

        private void Update()
        {
            if (!isEnabled) return;

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
            isEnabled = true;

            GameObject player = GameObject.Find("Player");
            GameManager.Instance.gameInput.ResetMovementInput();
            player.GetComponent<PlayerStateMachine>().SwitchState(new PlayerMovementState(player.GetComponent<PlayerStateMachine>()));

            myTutorialManager.allowedInputs = new GameInputSystem.InputActionType[]
            {
                GameInputSystem.InputActionType.Interact
            };

            dialogCanvas.SetActive(true);
            HUDCanvas.SetActive(false);
            index = 0;
            textComponent.text = "";
            StartDialog();
        }

        public void DisableDialog()
        {
            isEnabled = false;
            myTutorialManager.allowedInputs = new GameInputSystem.InputActionType[]
            {
                GameInputSystem.InputActionType.Movement,
                GameInputSystem.InputActionType.Roll
            };

            onDialogComplete.Invoke();
            HUDCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}