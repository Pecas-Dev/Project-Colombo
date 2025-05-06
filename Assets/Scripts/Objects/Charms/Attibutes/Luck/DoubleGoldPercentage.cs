using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class DoubleGoldPercentage : BaseAttributes
    {
        public float doubleGoldOnPickUpPercentage;
        PlayerInventory myPlayerInventory;
        public override void Enable()
        {
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
        }

        private void OnCoinsCollected(int amount)
        {
            int rand = Random.Range(0, 101);

            if (rand < doubleGoldOnPickUpPercentage)
            {
                myPlayerInventory.currencyAmount += amount;
                Debug.Log("doubled coins pickup");
            }
            else
            {
                Debug.Log("coins not doubled");
            }
        }

        public override void Disable()
        {
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
        }
    }
}