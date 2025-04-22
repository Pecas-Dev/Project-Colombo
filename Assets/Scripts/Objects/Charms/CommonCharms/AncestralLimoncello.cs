using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class AncestralLimoncello : BaseCharm
    {
        public int damageIncreasePercentage = 6;

        public override void Equip()
        {
            CustomEvents.OnDamageDelt += IncreaseDamage;
        }

        private void IncreaseDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int extraDamage = (int)(damage / 100f * damageIncreasePercentage);
            healthmanager.TakeDamage(extraDamage);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= IncreaseDamage;
        }
    }
}