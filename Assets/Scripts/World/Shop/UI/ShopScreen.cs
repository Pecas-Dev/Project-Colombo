using UnityEngine;
using TMPro;
using System.Collections.Generic;
using ProjectColombo.Inventory;


namespace ProjectColombo.Shop
{
    [System.Serializable]
    public class ItemToSell
    {
        public string name;
        public GameObject item;
        public Sprite sprite;
        public int price;
    }

    public class ShopScreen : MonoBehaviour
    {
        public TMP_Text currentPlayerCurrency;
        PlayerInventory playerInventory;
        public int spacing;
        public List<ItemToSell> itemsToSell;
        public GameObject buttonPrefab;
        List<ShopItems> itemButtons;

        private void Start()
        {
            playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString();
            itemButtons = new();

            int positionX = 0 - spacing * (itemsToSell.Count-1) / 2;

            foreach (ItemToSell i in itemsToSell)
            {
                Vector3 position = new(positionX, 0, 0);

                // Instantiate the button prefab
                GameObject prefab = Instantiate(buttonPrefab, transform);

                // Add the ShopItems component
                ShopItems shopItem = prefab.GetComponent<ShopItems>();
                itemButtons.Add(shopItem);

                // Set up the button (position, item info, etc.)
                shopItem.SetUp(i, position);

                // Optionally, you can adjust the position of the button relative to its parent
                prefab.GetComponent<RectTransform>().localPosition = position;

                positionX += spacing;
            }

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }
        }

        private void Update()
        {
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString(); //update text

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }
        }

        public void BuyItem(ItemToSell item)
        {
            playerInventory.currencyAmount -= item.price;
            Instantiate(item.item, playerInventory.transform.position, Quaternion.identity); //spawn bought item
            //Debug.Log("instantiate " + item.item.name + " at " + playerInventory.transform.position);
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString(); //update text

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }
        }

        public int GetCurrency()
        {
            return playerInventory.currencyAmount;
        }
    }
}