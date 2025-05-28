using ProjectColombo.GameManagement.Events;
using UnityEngine;
using ProjectColombo.Combat;

namespace ProjectColombo.Objects.Charms
{

    public class DamagePercentageMaxHealth : BaseAttributes
    {
        public float damagePercentageMaxHealth;
        public int forHowManyMaxHealth;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();

            int multiplyer = Mathf.FloorToInt(myHealthManager.MaxHealth / forHowManyMaxHealth);

            stats.majorDamagePercentage += multiplyer * damagePercentageMaxHealth;
            stats.minorDamagePercentage += multiplyer * damagePercentageMaxHealth;
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

            int multiplyer = Mathf.FloorToInt(myHealthManager.MaxHealth / forHowManyMaxHealth);
            int value = (int)(multiplyer * damagePercentageMaxHealth / 100f);

            Debug.Log("dealt " + value + " less damage for maxHealth: " + myHealthManager.MaxHealth);

            CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}