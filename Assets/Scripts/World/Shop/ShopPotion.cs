using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.Shop;
using ProjectColombo.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPotion : MonoBehaviour
{
    [Header("Sold Out Display")]
    [SerializeField] TMP_Text soldOutText;
    [SerializeField] bool autoFindSoldOutText = true;
    [SerializeField] Color unavailableColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Visual References")]
    [SerializeField] Image potionImage;
    [SerializeField] bool autoFindPotionImage = true;

    ShopItemSelectionAnimator selectionAnimator;

    bool isBought = false;
    public bool isActive;
    public int price;

    void Start()
    {
        if (autoFindSoldOutText && soldOutText == null)
        {
            FindSoldOutText();
        }

        if (autoFindPotionImage && potionImage == null)
        {
            FindPotionImage();
        }

        selectionAnimator = GetComponent<ShopItemSelectionAnimator>();

        if (selectionAnimator != null && potionImage != null)
        {
            selectionAnimator.SetTargetImage(potionImage);
        }

        CheckAvailability();
    }

    void Update()
    {
        CheckAvailability();
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
    }

    void FindPotionImage()
    {
        Image[] images = GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            if (image.name.ToLower().Contains("potion") ||
                image.name.ToLower().Contains("icon") ||
                image.name.ToLower().Contains("item"))
            {
                potionImage = image;
                Debug.Log($"Found potion image: {potionImage.name}");
                break;
            }
        }

        if (potionImage == null && images.Length > 0)
        {
            Button buttonComponent = GetComponent<Button>();
            foreach (Image image in images)
            {
                if (buttonComponent == null || image.gameObject != buttonComponent.gameObject)
                {
                    potionImage = image;
                    Debug.Log($"Using image as potion display: {potionImage.name}");
                    break;
                }
            }
        }
    }

    void CheckAvailability()
    {
        PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
        if (playerInventory == null) return;

        bool wasAffordable = (potionImage != null && potionImage.color == Color.white);
        bool canAfford = playerInventory.currencyAmount >= price && !isBought;

        if (soldOutText != null)
        {
            soldOutText.gameObject.SetActive(!canAfford);
        }

        Button buttonComponent = GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true;
        }

        if (potionImage != null)
        {
            if (!canAfford)
            {
                potionImage.color = unavailableColor;

                if (selectionAnimator != null)
                {
                    selectionAnimator.SetItemUnavailable(true);
                }
            }
            else if (!wasAffordable && canAfford)
            {
                if (selectionAnimator != null)
                {
                    selectionAnimator.SetItemUnavailable(false);
                }
                else
                {
                    potionImage.color = Color.white;
                }
            }
            else if (canAfford && selectionAnimator != null)
            {
                selectionAnimator.SetItemUnavailable(false);
            }
        }
    }

    public void SetSoldOutText(TMP_Text text)
    {
        soldOutText = text;
    }

    public void SetPotionImage(Image image)
    {
        potionImage = image;
    }

    public void BuyPotion()
    {
        PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
        if (playerInventory.currencyAmount < price)
        {
            Debug.Log("Not enough money for potion");
            return;
        }

        if (isBought) return;

        isBought = true;

        playerInventory.numberOfPotions++;
        playerInventory.currencyAmount -= price;
        CustomEvents.ItemPurchased(price);

        UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
        if (navigationManager != null)
        {
            navigationManager.PlayUISound(UISoundType.PurchseShop);
        }
    }
}
