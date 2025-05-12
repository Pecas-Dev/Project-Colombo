using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class ShopPotion : MonoBehaviour
{
    public bool isActive;
    public int price;

    public void BuyPotion()
    {
        PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

        GetComponent<Button>().interactable = false;
        playerInventory.numberOfPotions++;
        playerInventory.currencyAmount -= price;
        CustomEvents.ItemPurchased(price);
    }
}
