using UnityEngine;

namespace ProjectColombo.Shop
{
    public class ShopButtons : MonoBehaviour
    {
        public void ExitShop()
        {
            GetComponentInParent<ShopKeeper>().CloseShop();
        }

        public void BuyItem(ItemToSell item)
        {
            GetComponentInParent<ShopScreen>().BuyItem(item);
        }
    }
}