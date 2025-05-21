using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForPickup : BaseAttributes
    {
        public int minAmountOfGold;
        public int maxAmountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
            CustomEvents.OnMaskCollected += OnMaskCollected;
            CustomEvents.OnCharmCollected += OnCharmCollected;
        }

        private void OnCharmCollected(GameObject obj)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            AddCoins();
        }

        private void OnMaskCollected(GameObject obj)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            AddCoins();
        }

        private void OnCoinsCollected(int arg1)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            AddCoins();
        }

        void AddCoins()
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int rand = Random.Range(minAmountOfGold, maxAmountOfGold + 1);
            Debug.Log("extra gold for pick up: " + rand);
            CustomEvents.CoinsCollected(rand);
            myPlayerInventory.currencyAmount += rand;
        }

        public override void Disable()
        {
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
            CustomEvents.OnMaskCollected -= OnMaskCollected;
            CustomEvents.OnCharmCollected -= OnCharmCollected;
        }
    }
}