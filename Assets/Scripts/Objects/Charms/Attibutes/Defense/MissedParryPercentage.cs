using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MissedParryPercentage : BaseAttributes
    {
        public float extraDamageMissedParrySameScalePercentage;
        public float extraDamageMissedParryDifferentScalePercentage;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.extraDamageMissedParrySameScalePercentage += extraDamageMissedParryDifferentScalePercentage;
            stats.extraDamageMissedParryDifferentScalePercentage += extraDamageMissedParryDifferentScalePercentage;
        }

        public override void Enable()
        {
            CustomEvents.OnParryFailed += OnParryFailed;
        }

        private void OnParryFailed(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthManager, bool sameScale)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            if (sameScale)
            {
                int value = (int)(amount * extraDamageMissedParrySameScalePercentage / 100f);
                Debug.Log("extra damage on same scale parry missed: " + value);
                healthManager.TakeDamage(value);
            }
            else
            {
                int value = (int)(amount * extraDamageMissedParryDifferentScalePercentage / 100f);
                Debug.Log("extra damage on different scale parry missed: " + value);
                healthManager.TakeDamage(value);
            }
        }

        public override void Disable()
        {
            CustomEvents.OnParryFailed -= OnParryFailed;
        }
    }
}