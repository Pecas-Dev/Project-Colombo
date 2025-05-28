using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class DamagePercentageGold : BaseAttributes
    {
        public float damagePercentageGold;
        public int forHowManyGold;

        PlayerInventory myPlayerInventroy;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myPlayerInventroy = GameManager.Instance.GetComponent<PlayerInventory>();

            int multiplyer = Mathf.FloorToInt((float)(myPlayerInventroy.currencyAmount / forHowManyGold));
            int value = (int)(multiplyer * damagePercentageGold / 100f);

            stats.majorDamagePercentage += value;
            stats.minorDamagePercentage += value;
        }

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myPlayerInventroy = GameManager.Instance.GetComponent<PlayerInventory>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int multiplyer = Mathf.FloorToInt((float)(myPlayerInventroy.currencyAmount / forHowManyGold));
            int value = (int)(multiplyer * damagePercentageGold / 100f);

            Debug.Log("dealt " + value + " extra damage for carried gold: " + myPlayerInventroy.currencyAmount);
            CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}