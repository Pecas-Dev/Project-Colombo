using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class DamagePercentageMissingHealth : BaseAttributes
    {
        public float damagePercentageMissingHealth;
        public int forHowManyMissingHealth;

        HealthManager myHealthManager;

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            int multiplyer = Mathf.FloorToInt((myHealthManager.MaxHealth - myHealthManager.currentHealth) / forHowManyMissingHealth);
            int value = (int)(multiplyer * damagePercentageMissingHealth / 100f);

            Debug.Log("dealt " + value + " extra damage for missing health: " + (myHealthManager.MaxHealth - myHealthManager.currentHealth));
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}