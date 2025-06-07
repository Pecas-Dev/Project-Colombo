using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement.Stats;
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
            CustomEvents.OnItemPurchase += OnItemPurchase;
            CustomEvents.AbilityUsed(abilitySoundName);
            GameManager.Instance.GetComponent<GlobalStats>().currentShopDiscountPercent = discountOnAbility;
        }

        public override void EndAbility()
        {
            Debug.Log("end ability");
            available = true;
            active = false;
            CustomEvents.OnItemPurchase -= OnItemPurchase;
        }


        private void OnItemPurchase(int obj)
        {
            if (boughtItemsCounter >= numberOfReducedItems)
            {
                GameManager.Instance.GetComponent<GlobalStats>().currentShopDiscountPercent = 0;
            }

            if (boughtItemsCounter >= boughtItemsToReset)
            {
                EndAbility();
            }
        }
    }
}