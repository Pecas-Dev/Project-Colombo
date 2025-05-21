using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class IncomingDamagePercentageMaxHealth : BaseAttributes
    {
        public float damgePercentage;
        public int perHowManyMaxHealth;

        public override void Enable()
        {
            CustomEvents.OnDamageReceived += OnDamageReceived;
        }

        private void OnDamageReceived(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            int multiplyer = Mathf.FloorToInt(healthmanager.MaxHealth / perHowManyMaxHealth);
            int value = (int)(multiplyer * damgePercentage / 100f);
            Debug.Log("extra damage: " + value + " for max health: " + healthmanager.MaxHealth);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceived;
        }
    }
}