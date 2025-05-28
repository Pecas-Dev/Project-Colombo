using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{

    public class DamagePercentageLuck : BaseAttributes
    {
        public float damagePercentageLuck;
        public int forHowManyLuck;

        PlayerInventory myPlayerInventroy;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myPlayerInventroy = GameManager.Instance.GetComponent<PlayerInventory>();

            int multiplyer = Mathf.FloorToInt(myPlayerInventroy.currentLuck / forHowManyLuck);

            stats.majorDamagePercentage += multiplyer * damagePercentageLuck;
            stats.minorDamagePercentage += multiplyer * damagePercentageLuck;
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

            int multiplyer = Mathf.FloorToInt(myPlayerInventroy.currentLuck / forHowManyLuck);
            int value = (int)(multiplyer * damagePercentageLuck / 100f);

            Debug.Log("dealt " + value + " extra damage for luck: " + myPlayerInventroy.currentLuck);
            CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}