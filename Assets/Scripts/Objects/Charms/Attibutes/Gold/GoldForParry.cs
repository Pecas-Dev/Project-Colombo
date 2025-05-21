using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForParry : BaseAttributes
    {
        public int minAmountOfGold;
        public int maxAmountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale arg2, bool arg3)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int rand = Random.Range(minAmountOfGold, maxAmountOfGold + 1);
            Debug.Log("extra gold for parry: " + rand);
            CustomEvents.CoinsCollected(rand);
            myPlayerInventory.currencyAmount += rand;
        }

        public override void Disable()
        {
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
        }
    }
}