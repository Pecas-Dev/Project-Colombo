using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class EvadeChance : BaseAttributes
    {
        public float evadeChancePercentage;

        public override void Enable()
        {
            CustomEvents.OnDamageReceived += OnDamageReceived;
        }

        private void OnDamageReceived(int amount, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int rand = Random.Range(0, 101);

            if (rand < evadeChancePercentage)
            {
                healthmanager.Heal(amount);
                Debug.Log("evaded damage");
            }
            else
            {
                Debug.Log("not evaded damage");
            }
        }

        public override void Disable()
        {
            CustomEvents.OnDamageReceived -= OnDamageReceived;
        }
    }
}