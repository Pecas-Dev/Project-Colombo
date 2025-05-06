using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForIncomingDamage : BaseAttributes
    {
        public float damageForGoldPercentage;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnDamageReceived += OnDamageReceive;
        }

        private void OnDamageReceive(int arg1, GameGlobals.MusicScale arg2, Combat.HealthManager arg3)
        {
            int value = (int)(myPlayerInventory.currencyAmount * damageForGoldPercentage / 100f);
            Debug.Log("extra gold for getting hit: " + value);
            myPlayerInventory.currencyAmount += value;
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceive;
        }
    }
}