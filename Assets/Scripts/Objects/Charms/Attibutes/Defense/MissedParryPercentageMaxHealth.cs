using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class MissedParryPercentageMaxHealth : BaseAttributes
    {
        public float damgePercentageSameScale;
        public float damgePercentageOppScale;
        public int perHowManyMaxHealth;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();

            int multiplyer = Mathf.FloorToInt(myHealthManager.MaxHealth / perHowManyMaxHealth);

            stats.extraDamageMissedParrySameScalePercentage += multiplyer * damgePercentageSameScale;
            stats.extraDamageMissedParryDifferentScalePercentage += multiplyer * damgePercentageOppScale;
        }

        public override void Enable()
        {
            CustomEvents.OnParryFailed += OnParryFailed;
        }

        private void OnParryFailed(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, bool sameScale)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            if (sameScale)
            {
                int multiplyer = Mathf.FloorToInt(healthmanager.MaxHealth / perHowManyMaxHealth);
                int value = (int)(multiplyer * damgePercentageSameScale / 100f);
                Debug.Log("extra same scale failed parry damage: " + value + " for max health: " + healthmanager.MaxHealth);
                healthmanager.TakeDamage(value);
            }
            else
            {
                int multiplyer = Mathf.FloorToInt(healthmanager.MaxHealth / perHowManyMaxHealth);
                int value = (int)(multiplyer * damgePercentageOppScale / 100f);
                Debug.Log("extra opposite scale failed parry damage: " + value + " for max health: " + healthmanager.MaxHealth);
                healthmanager.TakeDamage(value);
            }

        }

        public override void Disable()
        {
            CustomEvents.OnParryFailed -= OnParryFailed;
        }
    }
}