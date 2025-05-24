using ProjectColombo.GameManagement;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.Objects.Decorations
{
    public class Chest : MonoBehaviour
    {
        public GameObject chestCanvas;
        public Animator chestAnimator;
        public VisualEffect chestVFX;
        bool playerClose;

        private void Update()
        {
            if (playerClose)
            {
                if (GameManager.Instance.gameInput.GetInputPressed(GameInputSystem.PlayerInputAction.Interact))
                {
                    OpenChest();
                }
            }
        }

        public void OpenChest()
        {
            GetComponent<Collider>().enabled = false;
            chestCanvas.SetActive(false);
            chestVFX.Stop();
            chestAnimator.SetTrigger("OpenChest");
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.gameInput.EnableInput(GameInputSystem.PlayerInputAction.Interact);
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