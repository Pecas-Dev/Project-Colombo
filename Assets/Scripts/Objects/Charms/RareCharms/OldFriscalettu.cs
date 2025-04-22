using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class OldFriscalettu : BaseCharm
    {
        public int damageIncreaseMajor = 12;
        public int damageIncreaseMinor = 4;
        public int damageResistance = 4; 

        public override void Equip()
        {
            CustomEvents.OnDamageDelt += IncreaseDamage;
            CustomEvents.OnDamageReceived += AddResistance;
            
        }

        private void AddResistance(int damage, GameGlobals.MusicScale scale, HealthManager healthManager)
        {
            int resistedDamage = (int)(damage / 100f * damageResistance);
            healthManager.TakeDamage(-resistedDamage);
        }

        private void IncreaseDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthManager)
        {
            int additionalDamage = damage;
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                additionalDamage += (int)(damage * damageIncreaseMajor / 100f);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                additionalDamage += (int)(damage * damageIncreaseMinor / 100f);
            }

            healthManager.TakeDamage(damage);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= IncreaseDamage;
        }
    }
}