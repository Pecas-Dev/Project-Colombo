using ProjectColombo.Inventory;
using UnityEngine;
using TMPro;
using System.Text;
using ProjectColombo.GameInputSystem;

namespace ProjectColombo.UI.HUD
{
    public class InventoryHUD : MonoBehaviour
    {
        PlayerInventory inventory;
        public TMP_Text currencyAmountText;
        public TMP_Text currentWeaponText;
        public TMP_Text charmAmountText;
        public TMP_Text maskAmountText;
        [SerializeField] private GameInputSO gameInput;
        [HideInInspector] public bool visible;


        private void Start()
        {
            inventory = GetComponentInParent<PlayerInventory>();
            visible = true;
        }

        private void Update()
        {
            currencyAmountText.text = inventory.currencyAmount.ToString();

            StringBuilder charmAmount = new();



            charmAmountText.text = charmAmount.ToString();

            StringBuilder maskAmount = new();


            maskAmountText.text = maskAmount.ToString();

            if (gameInput.GetInputPressed(PlayerInputAction.Interact))  //map to input this just for testing
            {
                ToggleVisability();
            }
        }

        public void ToggleVisability()
        {
            visible = !visible;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
            }
        }
    }
}