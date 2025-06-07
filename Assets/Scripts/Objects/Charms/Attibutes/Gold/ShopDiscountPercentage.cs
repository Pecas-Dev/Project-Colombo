using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.Shop;


namespace ProjectColombo.Objects.Charms
{
    public class ShopDiscountPercentage : BaseAttributes
    {
        public int discountPercentage;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            GameManager.Instance.GetComponent<GlobalStats>().currentShopDiscountPercent = discountPercentage;
        }

        public override void Disable()
        {
            GameManager.Instance.GetComponent<GlobalStats>().currentShopDiscountPercent = 0;
        }
    }
}