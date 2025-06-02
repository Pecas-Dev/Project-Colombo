using DG.Tweening;
using ProjectColombo.Objects.Charms;
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
        public TMP_Text nameText;
        public Button shopButton;
        bool isActive;
        Animator myAnimator;

        [Header("Sold Out Display")]
        [SerializeField] TMP_Text soldOutText;
        [SerializeField] bool autoFindSoldOutText = true;
        [SerializeField] Color unavailableColor;


        ShopItemSelectionAnimator selectionAnimator;

        void Awake()
        {
            if (autoFindSoldOutText && soldOutText == null)
            {
                FindSoldOutText();
            }
        }

        public void SetUp(ItemToSell itemStruct, Vector3 position, float discount)
        {
            myAnimator = GetComponent<Animator>();
            GetComponent<RectTransform>().position = position;
            item = itemStruct;

            float calculatedDiscount = (100f - discount) / 100f;
            item.price = (int)(item.price * calculatedDiscount);

            BaseCharm charmDetails = item.item.GetComponent<BaseCharm>();

            referenceImage.sprite = charmDetails.charmPicture;
            itemPrice.text = item.price.ToString();
            nameText.text = item.name; //adjust to basecharm, baseitem

            // Set up the button click listener
            shopButton.onClick.RemoveAllListeners();  // Remove any existing listeners
            shopButton.onClick.AddListener(() => BuyItem(item)); // Add a new listener that passes the item to BuyItem

            selectionAnimator = GetComponent<ShopItemSelectionAnimator>();

            if (selectionAnimator != null && referenceImage != null)
            {
                selectionAnimator.SetTargetImage(referenceImage);
            }
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

        public void AdjustPriceToDiscount(float discount)
        {
            float calculatedDiscount = (100f - discount) / 100f;
            item.price = (int)(item.price * calculatedDiscount);
            itemPrice.text = item.price.ToString();
        }

        void FindSoldOutText()
        {
            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>();

            foreach (TMP_Text text in textComponents)
            {
                if (text.name.ToLower().Contains("sold") ||
                    text.name.ToLower().Contains("out") ||
                    text.name.ToLower().Contains("unavailable"))
                {
                    soldOutText = text;
                    Debug.Log($"Found sold out text: {soldOutText.name}");
                    break;
                }
            }

            if (soldOutText == null && textComponents.Length > 0)
            {
                foreach (TMP_Text text in textComponents)
                {
                    if (text != itemPrice && text != nameText)
                    {
                        soldOutText = text;
                        Debug.Log($"Using text component as sold out display: {soldOutText.name}");
                        break;
                    }
                }
            }
        }

        public void SetSoldOutText(TMP_Text text)
        {
            soldOutText = text;
        }

        public void CheckActive()
        {
            ShopScreen currentScreen = GetComponentInParent<ShopScreen>();
            if (currentScreen == null) return;

            bool wasActive = isActive;
            isActive = item.price <= GetComponentInParent<ShopScreen>().GetCurrency();

            if (soldOutText != null)
            {
                soldOutText.gameObject.SetActive(!isActive);
            }

            if (shopButton != null)
            {
                shopButton.interactable = true;
            }

            if (!isActive)
            {
                referenceImage.color = unavailableColor;
                Debug.Log($"Item {item.name} set to unavailable color");
            }
            else if (wasActive != isActive)
            {
                if (selectionAnimator != null)
                {
                    selectionAnimator.RefreshColorState();
                    Debug.Log($"Item {item.name} became available - refreshing selection state");
                }
                else
                {
                    referenceImage.color = Color.white;
                    Debug.Log($"Item {item.name} set to white (no animator)");
                }
            }
        }
    }
}