using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.Objects.Decorations
{
    public class Chest : MonoBehaviour
    {
        public GameObject chestCanvas;
        public Animator chestAnimator;
        bool playerClose;

        private void Update()
        {
            if (playerClose)
            {
                if (GameManager.Instance.gameInput.InteractPressed)
                {
                    OpenChest();
                }
            }
        }

        public void OpenChest()
        {
            GetComponent<Collider>().enabled = false;
            chestCanvas.SetActive(false);
            chestAnimator.SetTrigger("OpenChest");
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.gameInput.EnableInput(GameInputSystem.InputActionType.UseItem);
                chestCanvas.SetActive(true);
                playerClose = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                chestCanvas.SetActive(false);
                playerClose = false;
            }
        }
    }
}