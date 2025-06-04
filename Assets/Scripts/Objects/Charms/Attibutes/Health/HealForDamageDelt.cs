using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class HealForDamageDelt : BaseAttributes
    {
        public float healDamagePercentage;

        HealthManager myHealthmanager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            CustomEvents.OnDamageDelt += OnDamageDelt;
            myHealthmanager = p.GetComponent<HealthManager>();
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

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