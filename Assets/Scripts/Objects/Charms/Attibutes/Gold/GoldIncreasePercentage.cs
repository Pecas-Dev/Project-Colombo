using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldIncreasePercentage : BaseAttributes
    {
        public int extraGoldPercentage;

        PlayerInventory myPlayerInventory;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnCoinsCollected(int amount)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int extra = Mathf.RoundToInt(amount * extraGoldPercentage / 100f);
            CustomEvents.CoinsCollected(extra);
            myPlayerInventory.currencyAmount += extra;
        }

        public override void Disable()
        {
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
        }
    }
}