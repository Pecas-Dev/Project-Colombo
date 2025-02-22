using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectColombo.Shop
{
    public class ShopItems : MonoBehaviour
    {
        public ItemToSell item;
        public Image referenceImage;
        public TMP_Text itemPrice;
        public TMP_Text name;
        public Button shopButton;
        bool isActive;
        Animator myAnimator;

        private void Update()
        {
        }

        public void SetUp(ItemToSell itemStruct, Vector3 position)
        {
            myAnimator = GetComponent<Animator>();
            GetComponent<RectTransform>().position = position;
            item = itemStruct;

            referenceImage.sprite = item.sprite;
            itemPrice.text = item.price.ToString();
            name.text = item.name;

            // Set up the button click listener
            shopButton.onClick.RemoveAllListeners();  // Remove any existing listeners
            shopButton.onClick.AddListener(() => BuyItem(item)); // Add a new listener that passes the item to BuyItem
        }

        private void BuyItem(ItemToSell item)
        {
            myAnimator.ResetTrigger("Success");
            myAnimator.ResetTrigger("Fail");

            if (isActive)
            {
                GetComponentInParent<ShopScreen>().BuyItem(item);
                myAnimator.SetTrigger("Success");
                Debug.Log("bought for " + item.price);
            }
            else
            {
                myAnimator.SetTrigger("Fail");
                Debug.Log("not enough money");
            }
        }

        public void CheckActive()
        {
            isActive = item.price <= GetComponentInParent<ShopScreen>().GetCurrency();

            referenceImage.color = isActive ? Color.white : Color.red;
        }


    }
}