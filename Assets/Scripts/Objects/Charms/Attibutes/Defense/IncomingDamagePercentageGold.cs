using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using Unity.VisualScripting;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class IncomingDamagePercentageGold : BaseAttributes
    {
        public float damgePercentage;
        public int perHowManyGold;

        PlayerInventory myPlayerInventory;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

            int multiplyer = Mathf.FloorToInt(myPlayerInventory.currencyAmount / perHowManyGold);

            stats.incomingMajorDamagePercentage += damgePercentage * multiplyer;
            stats.incomingMinorDamagePercentage += damgePercentage * multiplyer;
        }

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnDamageReceived += OnDamageReceived;
        }

        private void OnDamageReceived(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int multiplyer = Mathf.FloorToInt(myPlayerInventory.currencyAmount / perHowManyGold);
            int value = (int)(multiplyer * damgePercentage / 100f);
            Debug.Log("receiving extra damage: " + value + " for held gold: " + myPlayerInventory.currencyAmount);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceived;
        }
    }
}