using UnityEngine;
using TMPro;
using System.Collections.Generic;
using ProjectColombo.Inventory;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using UnityEngine.UI;
using System.Collections;
using ProjectColombo.UI;
using ProjectColombo.GameManagement.Stats;


namespace ProjectColombo.Shop
{
    [System.Serializable]
    public class ItemToSell
    {
        public string name;
        public GameObject item;
        public int price;

        [HideInInspector]
        public int basePrice; // New field
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
        public List<ShopItems> itemButtons = new();
        List<int> noDuplicates = new();
        float discount = 0;
        public Button potionButton;
        //public GameObject exitShopButton;

        [Header("Selection Animation")]
        [SerializeField] bool enableSelectionAnimation = true;

        private void Start()
        {
            potionButton.interactable = true;
            playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString();
            itemButtons = new();

            discount = GameManager.Instance.GetComponent<GlobalStats>().currentShopDiscountPercent;
            Debug.Log("discount on shop start " + discount);

            int positionX = 0;

            for (int i = 0; i < numbersOfItemsToSell; i++)
            {
                Vector3 position = new(positionX, 50, 0);

                // Instantiate the button prefab
                GameObject prefab = Instantiate(buttonPrefab, transform);

                // Add the ShopItems component
                ShopItems shopItem = prefab.GetComponent<ShopItems>();
                itemButtons.Add(shopItem);

                int rand = Random.Range(0, itemsToSell.Count);

                while (noDuplicates.Contains(rand))
                {
                    rand = Random.Range(0, itemsToSell.Count);
                }

                shopItem.SetUp(itemsToSell[rand], position, discount);
                prefab.GetComponent<RectTransform>().localPosition = position;
                noDuplicates.Add(rand);

                if (enableSelectionAnimation)
                {
                    ShopItemSelectionAnimator animator = prefab.GetComponentInChildren<ShopItemSelectionAnimator>();

                    if (shopItem.referenceImage != null)
                    {
                        animator.SetTargetImage(shopItem.referenceImage);
                    }
                }

                positionX += spacing;
            }

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }
            
            StartCoroutine(DelayedInitialColorSetup());
        }
        
        IEnumerator DelayedInitialColorSetup()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.3f);
            
            foreach (ShopItems b in itemButtons)
            {
                if (b != null)
                {
                    b.CheckActive();
                }
            }
        }

        public IEnumerator SetFirstSelected()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.1f);

            foreach (ShopItems b in itemButtons)
            {
                if (b != null)
                {
                    b.CheckActive();
                }
            }
        }

        private void Update()
        {
            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString(); 

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }
        }

        //public void SetDiscount(float discount)
        //{
        //    Debug.Log("set discount in shopscreen " + discount);
        //    this.discount = discount;

        //    foreach(ShopItems b in itemButtons)
        //    {
        //        Debug.Log("item discounted");
        //        b.AdjustPriceToDiscount(discount);
        //    }
        //}

        public void BuyItem(ItemToSell item)
        {
            

            playerInventory.currencyAmount -= item.price;
            CustomEvents.ItemPurchased(item.price);

            UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager != null)
            {
                navigationManager.PlayUISound(UISoundType.PurchseShop);
            }

            currentPlayerCurrency.text = playerInventory.currencyAmount.ToString();

            foreach (ShopItems b in itemButtons)
            {
                b.CheckActive();
            }

            ShopNavigationController navController = FindFirstObjectByType<ShopNavigationController>();
            if (navController != null)
            {
                navController.RefreshNavigationAfterPurchase();
            }

            CustomEvents.CharmCollected(item.item);
        }

        public int GetCurrency()
        {
            return playerInventory.currencyAmount;
        }
    }
}