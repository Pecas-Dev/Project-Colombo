using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MassaniellosCoppola : BaseCharm
    {
        public int damageResistancePercentage = 10;
        public int failedParryBonus = 25;

        public override void Equip()
        {
            CustomEvents.OnDamageReceived += DecreaseDamageGeneral;
            CustomEvents.OnParryFailed += DecreaseDamageParry;
        }

        private void DecreaseDamageGeneral(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int extraDamage = (int)(damage / 100f * damageResistancePercentage);
            healthmanager.TakeDamage(-extraDamage);
        }

        private void DecreaseDamageParry(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager, bool sameScale)
        {
            int extraDamage = (int)(damage / 100f * failedParryBonus);
            healthmanager.TakeDamage(-extraDamage);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageReceived -= DecreaseDamageGeneral;
            CustomEvents.OnParryFailed -= DecreaseDamageParry;
        }
    }
}