using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class BlockPercentage : BaseAttributes
    {
        public float extraBlockDamagePercentage;

        public override void Enable()
        {
            CustomEvents.OnDamageBlocked += OnDamageBlocked;
        }

        private void OnDamageBlocked(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int value = (int)(amount * extraBlockDamagePercentage / 100f);
            Debug.Log("added block damage of: " + value);
            healthmanager.TakeDamage(value);
        }

        public override void Disable()
        {
            CustomEvents.OnDamageBlocked -= OnDamageBlocked;
        }
    }
}