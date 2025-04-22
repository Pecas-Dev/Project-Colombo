using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class BrokenScacciapensieri : BaseCharm
    {
        public int damageIncreaseMajor = 4;
        public int damageIncreaseMinor = 12;
        public int moveSpeedIncrease = 6;
        float speedIncrease;

        public override void Equip()
        {
            speedIncrease = GetComponentInParent<EntityAttributes>().moveSpeed / 100f * moveSpeedIncrease;
            GetComponentInParent<EntityAttributes>().moveSpeed += speedIncrease;

            CustomEvents.OnDamageDelt += IncreaseDamage;
            
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
            GetComponentInParent<EntityAttributes>().moveSpeed -= speedIncrease;
            CustomEvents.OnDamageDelt -= IncreaseDamage;
        }
    }
}