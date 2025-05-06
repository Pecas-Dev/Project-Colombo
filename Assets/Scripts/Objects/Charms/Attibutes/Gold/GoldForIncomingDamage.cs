using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForIncomingDamage : BaseAttributes
    {
        public int minAmountOfGold;
        public int maxAmountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnDamageReceived += OnDamageReceive;
        }

        private void OnDamageReceive(int arg1, GameGlobals.MusicScale arg2, Combat.HealthManager arg3)
        {
            int rand = Random.Range(minAmountOfGold, maxAmountOfGold + 1);
            Debug.Log("extra gold for getting hit: " + rand);
            myPlayerInventory.currencyAmount += rand;
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceive;
        }
    }
}