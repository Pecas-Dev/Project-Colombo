using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class MissedParryPercentageMaxHealth : BaseAttributes
    {
        public float damgePercentage;
        public int perHowManyMaxHealth;

        public override void Enable()
        {
            CustomEvents.OnParryFailed += OnParryFailed;
        }

        private void OnParryFailed(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, bool sameScale)
        {
            int multiplyer = Mathf.FloorToInt(healthmanager.MaxHealth / perHowManyMaxHealth);
            int value = (int)(multiplyer * damgePercentage / 100f);
            Debug.Log("extra failed parry damage: " + value + " for max health: " + healthmanager.MaxHealth);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnParryFailed -= OnParryFailed;
        }
    }
}