using ProjectColombo.GameManagement.Events;
using ProjectColombo.Shop;


namespace ProjectColombo.Objects.Charms
{
    public class ShopDiscountPercentage : BaseAttributes
    {
        public int discountPercentage;

        public override void Enable()
        {
            CustomEvents.OnShopOpen += OnShopOpen;
        }

        private void OnShopOpen(Shop.ShopKeeper shop)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            shop.GetComponentInChildren<ShopScreen>().SetDiscount(discountPercentage);
        }

        public override void Disable()
        {
            CustomEvents.OnShopOpen -= OnShopOpen;
        }
    }
}