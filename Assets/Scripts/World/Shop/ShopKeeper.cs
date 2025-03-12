using ProjectColombo.GameInputSystem;
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
        [SerializeField] private GameInputSO gameInput;
        public GameObject firstSelect;

        private void Start()
        {
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
                Debug.Log("player entered shop");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerInRange = false;
                shopIndicator.SetActive(false);
                other.GetComponent<PlayerStateMachine>().closeShop = null;
                Debug.Log("player exited shop");
            }
        }

        public void OpenShop()
        {
            shopScreen.SetActive(true);
            shopIndicator.SetActive(false);
            gameInput.EnableUIMode();
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().EnterShopState(this);
            EventSystem.current.SetSelectedGameObject(firstSelect);
        }

        public void CloseShop()
        {
            shopScreen.SetActive(false);
            shopIndicator.SetActive(true);
            gameInput.DisableUIMode();
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().ExitShopState();
        }
    }
}