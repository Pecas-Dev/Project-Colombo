using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class IncomingDamagePercentage : BaseAttributes
    {
        public float majorDamagePercentage;
        public float minorDamagePercentage;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.majorDamagePercentage += majorDamagePercentage;
            stats.minorDamagePercentage += minorDamagePercentage;
        }

        public override void Enable()
        {
            CustomEvents.OnDamageReceived += OnDamageReceived;
        }

        private void OnDamageReceived(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                int value = (int)(amount * majorDamagePercentage / 100f);
                Debug.Log("increased major damage received by: " + value);
                healthmanager.TakeDamage(value);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                int value = (int)(amount * minorDamagePercentage / 100f);
                Debug.Log("increased minor damage received by: " + value);
                healthmanager.TakeDamage(value);
            }
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceived;
        }
    }
}