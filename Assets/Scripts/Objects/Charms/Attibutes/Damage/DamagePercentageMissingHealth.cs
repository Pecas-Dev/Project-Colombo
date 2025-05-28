using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class DamagePercentageMissingHealth : BaseAttributes
    {
        public float damageMissingHealth;
        public int forHowManyMissingHealth;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();

            int multiplyer = Mathf.FloorToInt((myHealthManager.MaxHealth - myHealthManager.currentHealth) / forHowManyMissingHealth);

            stats.majorDamagePercentage += multiplyer * damageMissingHealth;
            stats.minorDamagePercentage += multiplyer * damageMissingHealth;
        }

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int multiplyer = Mathf.FloorToInt((myHealthManager.MaxHealth - myHealthManager.currentHealth) / forHowManyMissingHealth);
            int value = (int)(multiplyer * damageMissingHealth);

            Debug.Log("dealt " + value + " extra damage for missing health: " + (myHealthManager.MaxHealth - myHealthManager.currentHealth));

            CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}