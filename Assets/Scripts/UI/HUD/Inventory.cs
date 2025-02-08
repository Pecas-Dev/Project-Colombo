using ProjectColombo.Inventory;
using UnityEngine;
using TMPro;
using System.Text;

namespace ProjectColombo.UI.HUD
{
    public class Currency : MonoBehaviour
    {
        PlayerInventory inventory;
        public TMP_Text currencyAmountText;
        public TMP_Text currentWeaponText;
        public TMP_Text charmAmountText;
        public TMP_Text maskAmountText;


        private void Start()
        {
            inventory = GetComponentInParent<PlayerInventory>();
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

            if (Input.GetKeyUp(KeyCode.I))  //map to input this just for testing
            {
                ToggleVisability();
            }
        }

        public void ToggleVisability()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(!child.gameObject.activeInHierarchy);
            }
        }
    }
}