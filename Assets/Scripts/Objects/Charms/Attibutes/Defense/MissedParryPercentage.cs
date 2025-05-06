using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MissedParryPercentage : BaseAttributes
    {
        public float extraDamageMissedParrySameScalePercentage;
        public float extraDamageMissedParryDifferentScalePercentage;

        public override void Enable()
        {
            CustomEvents.OnParryFailed += OnParryFailed;
        }

        private void OnParryFailed(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthManager, bool sameScale)
        {
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