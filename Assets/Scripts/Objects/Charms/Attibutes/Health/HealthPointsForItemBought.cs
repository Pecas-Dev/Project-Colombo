using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class HealthPointsForItemBought : BaseAttributes
    {
        public float healthPointsForItemPricePercentage;

        HealthManager myHealthManager;

        public override void Enable()
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
            CustomEvents.OnItemPurchase += OnItemPurchase;
        }

        private void OnItemPurchase(int price)
        {
            int value = (int)(price * healthPointsForItemPricePercentage / 100f);
            Debug.Log("health for item price: " + value);
            myHealthManager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnItemPurchase -= OnItemPurchase;
        }
    }
}