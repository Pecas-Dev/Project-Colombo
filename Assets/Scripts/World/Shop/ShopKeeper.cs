using ProjectColombo.GameInputSystem;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;
using UnityEngine.EventSystems;


namespace ProjectColombo.Shop
{
    public class ShopKeeper : MonoBehaviour
    {
        public GameObject shopScreen;
        public GameObject shopIndicator;
        bool playerInRange;
        private GameInputSO gameInput;

        private void Start()
        {
            gameInput = GameManager.Instance.gameInput;
            shopScreen.SetActive(false);
            shopIndicator.SetActive(false);
        }

        private void Update()
        {
            if (playerInRange && gameInput.GetInputPressed(PlayerInputAction.Interact))
            {
                OpenShop();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = true;
                shopIndicator.SetActive(true);
                other.GetComponent<PlayerStateMachine>().closeShop = this;
                //Debug.Log("player entered shop");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = false;
                shopIndicator.SetActive(false);
                other.GetComponent<PlayerStateMachine>().closeShop = null;
            }
        }

        public void OpenShop()
        {
            GameManager.Instance.PauseGame(false);
            shopScreen.SetActive(true);
            shopScreen.GetComponentInChildren<ShopScreen>().SetDiscount(0f);
            shopIndicator.SetActive(false);
            StartCoroutine(shopScreen.GetComponent<ShopScreen>().SetFirstSelected());
            CustomEvents.ShopOpen(this);
        }

        public void CloseShop()
        {
            GameManager.Instance.ResumeGame();
            shopScreen.SetActive(false);
            shopIndicator.SetActive(true);
            shopScreen.GetComponentInChildren<ShopScreen>().SetDiscount(0f);
            CustomEvents.ShopClose();
        }


        public void CloseShopScreen()
        {
            shopScreen.SetActive(false);
        }

        public void OpenShopScreen()
        {
            GameManager.Instance.PauseGame(false);
            shopScreen.SetActive(true);
            StartCoroutine(shopScreen.GetComponent<ShopScreen>().SetFirstSelected());
        }
    }
}