using ProjectColombo.GameManagement;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
        public AudioResource[] voiceLines;
        public AudioSource audioPlayer;
        public UnityEvent onDialogStart;
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
            GameManager.Instance.gameInput.LockAllInputsViaTutorial();
            GameManager.Instance.gameInput.SwitchToUI();

            onDialogStart.Invoke();
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
            GameManager.Instance.gameInput.UnlockInputViaTutorial(GameInputSystem.PlayerInputAction.Movement);
            GameManager.Instance.gameInput.UnlockInputViaTutorial(GameInputSystem.PlayerInputAction.Pause);
            //GameManager.Instance.gameInput.UnlockInputViaTutorial(GameInputSystem.PlayerInputAction.Roll);


            onDialogComplete.Invoke();
            HUDCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}