using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class OldFriscalettu : BaseCharm
    {
        public float damageIncreaseMajor = 12;
        public float damageIncreaseMinor = 4;
        public float damageResistance = 4; 

        public override void Equip()
        {
            CustomEvents.OnDamageDelt += IncreaseDamage;
            CustomEvents.OnDamageReceived += AddResistance;
            
        }

        private void AddResistance(int damage, GameGlobals.MusicScale scale, HealthManager healthManager)
        {
            int resistedDamage = (int)(damage / 100f * damageResistance);
            Debug.Log("decreased damage from: " + damage + ", by: " + resistedDamage);
            healthManager.TakeDamage(-resistedDamage);
        }

        private void IncreaseDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthManager)
        {
            int additionalDamage = 0;
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                additionalDamage += (int)(damage * damageIncreaseMajor / 100f);
                Debug.Log("increase major damage from: " + damage + ", by: " + additionalDamage);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                additionalDamage += (int)(damage * damageIncreaseMinor / 100f);
                Debug.Log("increased minor damage from: " + damage + ", by: " + additionalDamage);
            }

            healthManager.TakeDamage(additionalDamage);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= IncreaseDamage;
            CustomEvents.OnDamageReceived -= AddResistance;
        }
    }
}