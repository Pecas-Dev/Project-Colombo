using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class DamagePercentage : BaseAttributes
    {
        public float majorDamagePercentage;
        public float minorDamagePercentage;

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale scale, bool sameScale, Combat.HealthManager healthmanager, int comboLength)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                int value = (int)(amount * majorDamagePercentage / 100f);
                Debug.Log("increased major damage delt by: " + value);
                CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
                healthmanager.TakeDamage(value);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                int value = (int)(amount * minorDamagePercentage / 100f);
                Debug.Log("increased minor damage delt by: " + value);
                CustomEvents.DamageDelt(value, scale, sameScale, healthmanager, comboLength);
                healthmanager.TakeDamage(value);
            }
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}