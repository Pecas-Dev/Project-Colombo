using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace ProjectColombo.UI
{
    public class DialogSystem : MonoBehaviour
    {
        public GameObject dialogCanvas;
        public TextMeshProUGUI textComponent;
        public GameObject HUDCanvas;
        public string[] lines;
        public float textSpeed;
        public AudioResource[] voiceLines;
        public AudioSource audioPlayer;
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
            textComponent.text = "";

            if (index < voiceLines.Length)
            {
                audioPlayer.resource = voiceLines[index];
                audioPlayer.Play();
            }

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
                audioPlayer.Stop();
                index++;

                if (index < voiceLines.Length)
                {
                    audioPlayer.resource = voiceLines[index];
                    audioPlayer.Play();
                }

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
            if (isEnabled) return;
            isEnabled = true;

            StartCoroutine(StopMoving());
            GameManager.Instance.gameInput.SwitchToUI();
            GameManager.Instance.gameInput.LockAllInputsViaTutorial();

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
            audioPlayer.Stop();
            GameManager.Instance.gameInput.SwitchToGameplay();
            GameManager.Instance.gameInput.UnlockAllInputsViaTutorial();

            onDialogComplete.Invoke();
            HUDCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
