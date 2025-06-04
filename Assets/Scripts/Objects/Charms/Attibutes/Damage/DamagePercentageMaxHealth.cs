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
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myHealthManager = p.GetComponent<HealthManager>();


            int multiplyer = Mathf.FloorToInt((float)(myHealthManager.MaxHealth / forHowManyMaxHealth));
            int value = (int)(multiplyer * damagePercentageMaxHealth / 100f);

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

            int multiplyer = Mathf.FloorToInt((float)(myHealthManager.MaxHealth / forHowManyMaxHealth));
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