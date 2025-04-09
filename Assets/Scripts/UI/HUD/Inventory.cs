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
            currentWeaponText.text = inventory.currentWeapon;

            StringBuilder charmAmount = new();

            foreach (var type in inventory.charms)
            {
                charmAmount.AppendLine(type.Key + ": " + type.Value);
            }

            charmAmountText.text = charmAmount.ToString();

            StringBuilder maskAmount = new();

            foreach (var type in inventory.masks)
            {
                maskAmount.AppendLine(type.Key + ": " + type.Value);
            }

            maskAmountText.text = maskAmount.ToString();

            if (gameInput.UseItemPressed)  //map to input this just for testing
            {
                ToggleVisability();
            }
        }

        private void LateUpdate()
        {
            if (gameInput.UseItemPressed)
            {
                gameInput.ResetUseItemPressed();
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