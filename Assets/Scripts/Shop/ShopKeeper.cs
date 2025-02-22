using ProjectColombo.GameInputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectColombo.Shop
{
    public class ShopKeeper : MonoBehaviour
    {
        public GameObject shopScreen;
        bool playerInRange;
        [SerializeField] private GameInputSO gameInput;
        public GameObject firstSelect;

        private void Start()
        {
            shopScreen.SetActive(false);
        }

        private void Update()
        {
            if (playerInRange && gameInput.InteractPressed)
            {
                OpenShop();
                gameInput.ResetInteractPressed(); // Reset interaction to prevent multiple triggers
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        public void OpenShop()
        {
            gameInput.EnableUIMode();
            shopScreen.SetActive(true);
            EventSystem.current.SetSelectedGameObject(firstSelect);
        }

        public void CloseShop()
        {
            gameInput.DisableUIMode();
            shopScreen.SetActive(false);
        }
    }
}