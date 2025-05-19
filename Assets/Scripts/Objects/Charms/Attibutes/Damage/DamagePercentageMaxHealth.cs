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

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            int multiplyer = Mathf.FloorToInt(myHealthManager.MaxHealth / forHowManyMaxHealth);
            int value = (int)(multiplyer * damagePercentageMaxHealth / 100f);

            Debug.Log("dealt " + value + " extra damage for maxHealth: " + myHealthManager.MaxHealth);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}