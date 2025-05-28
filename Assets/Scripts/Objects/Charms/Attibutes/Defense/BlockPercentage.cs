using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class BlockPercentage : BaseAttributes
    {
        public float extraBlockDamagePercentage;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.blockPercentage += extraBlockDamagePercentage;
        }

        public override void Enable()
        {
            CustomEvents.OnDamageBlocked += OnDamageBlocked;
        }

        private void OnDamageBlocked(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

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