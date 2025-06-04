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

            int multiplier = Mathf.FloorToInt((float)(myHealthManager.MaxHealth - myHealthManager.currentHealth) / forHowManyMissingHealth);
            int value = (int)(multiplier * damageMissingHealth);

            stats.majorDamagePercentage += value;
            stats.minorDamagePercentage += value;
        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthManager = p.GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int multiplier = Mathf.FloorToInt((float)(myHealthManager.MaxHealth - myHealthManager.currentHealth) / forHowManyMissingHealth);
            int value = (int)(multiplier * damageMissingHealth);

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