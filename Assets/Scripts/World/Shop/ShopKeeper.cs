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
        public GameObject firstSelect;

        private void Start()
        {
            gameInput = GameManager.Instance.gameInput;
            shopScreen.SetActive(false);
            shopIndicator.SetActive(false);
        }

        private void Update()
        {
            if (playerInRange && gameInput.InteractPressed)
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
                //Debug.Log("player exited shop");
            }
        }

        public void OpenShop()
        {
            shopScreen.SetActive(true);
            shopScreen.GetComponentInChildren<ShopScreen>().SetDiscount(0f);
            shopIndicator.SetActive(false);
            gameInput.EnableUIMode();
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().EnterShopState(this.gameObject);
            EventSystem.current.SetSelectedGameObject(firstSelect);
            CustomEvents.ShopOpen(this);
        }

        public void CloseShop()
        {
            shopScreen.SetActive(false);
            shopIndicator.SetActive(true);
            gameInput.DisableUIMode();
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().ExitShopState();
            shopScreen.GetComponentInChildren<ShopScreen>().SetDiscount(0f);
            CustomEvents.ShopClose();
        }
    }
}