using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class HealForDamageDelt : BaseAttributes
    {
        public float healDamagePercentage;

        HealthManager myHealthmanager;

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthmanager = GameObject.Find("Player").GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, int comboLength)
        {
            int value = (int)(amount * healDamagePercentage / 100f);
            Debug.Log("healed: " + value + "of this damage: " + amount);
            myHealthmanager.Heal(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}