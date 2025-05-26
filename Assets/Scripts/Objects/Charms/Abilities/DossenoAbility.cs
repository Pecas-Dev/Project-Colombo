using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Shop;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class DossenoAbility : BaseAbility
    {
        [Header("Dosseno")]
        public int numberOfReducedItems = 2;
        public float discountOnAbility = 15;
        public int boughtItemsToReset = 4;
        int boughtItemsCounter;

        public override void UseAbility()
        {
            CustomEvents.OnShopOpen += OnShopOpen;
            CustomEvents.OnItemPurchase += OnItemPurchase;
            CustomEvents.AbilityUsed(abilitySoundName);
        }

        public override void EndAbility()
        {
            Debug.Log("end ability");
            available = true;
            active = false;
            CustomEvents.OnShopOpen -= OnShopOpen;
            CustomEvents.OnItemPurchase -= OnItemPurchase;
        }

        private void OnShopOpen(ShopKeeper shop)
        {
            if (boughtItemsCounter <= numberOfReducedItems)
            {
                shop.GetComponentInChildren<ShopScreen>().SetDiscount(discountOnAbility);
                Debug.Log("set discount");
            }
        }
        private void OnItemPurchase(int obj)
        {
            boughtItemsCounter++;
            Debug.Log("item purchased: " + boughtItemsCounter);

            if (boughtItemsCounter >= boughtItemsToReset)
            {
                EndAbility();
            }
        }
    }
}