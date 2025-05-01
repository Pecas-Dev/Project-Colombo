using UnityEngine;
using TMPro;
using System.Collections.Generic;
using ProjectColombo.Inventory;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Objects;


namespace ProjectColombo.Shop
{
    [System.Serializable]
    public class ItemToSell
    {
        public string name;
        public GameObject item;
        public int price;
    }

    public class ShopScreen : MonoBehaviour
    {
        public TMP_Text currentPlayerCurrency;
        PlayerInventory playerInventory;
        public int spacing;
        public int numbersOfItemsToSell;
        public List<ItemToSell> itemsToSell;
        public GameObject buttonPrefab;
        public GameObject pickUpPrefab;
        List<ShopItems> itemButtons;
        List<int> noDuplicates = new();
        GameObject player;
        float discount = 0;

        private void Start()
        {
            playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            player = GameObject.Find("Player");
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString();
            itemButtons = new();

            int positionX = 0 - (int)(spacing * (numbersOfItemsToSell-1) / 2f);

            for (int i = 0; i < numbersOfItemsToSell; i++)
            {
                Vector3 position = new(positionX, 50, 0);

                // Instantiate the button prefab
                GameObject prefab = Instantiate(buttonPrefab, transform);

                // Add the ShopItems component
                ShopItems shopItem = prefab.GetComponent<ShopItems>();
                itemButtons.Add(shopItem);

                int rand = Random.Range(0, itemsToSell.Count);

                while(noDuplicates.Contains(rand))
                {
                    rand = Random.Range(0, itemsToSell.Count);
                }

                shopItem.SetUp(itemsToSell[rand], position, discount);
                prefab.GetComponent<RectTransform>().localPosition = position;
                noDuplicates.Add(rand);

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

        public void SetDiscount(float discount)
        {
            this.discount = discount;

            foreach(ShopItems b in itemButtons)
            {
                b.AdjustPriceToDiscount(discount);
            }
        }

        public void BuyItem(ItemToSell item)
        {
            playerInventory.currencyAmount -= item.price;
            CustomEvents.ItemPurchased(item.price);

            GameObject instance = Instantiate(pickUpPrefab, player.transform.position, player.transform.rotation);

            instance.GetComponent<PickUp>().SetCharm(item.item);

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