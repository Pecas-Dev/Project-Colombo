using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class BrokenScacciapensieri : BaseCharm
    {
        public float damageIncreaseMajor = 4;
        public float damageIncreaseMinor = 12;
        public float moveSpeedIncrease = 6;
        float speedIncrease;

        EntityAttributes myEntityAttributes;

        public override void Equip()
        {
            myEntityAttributes = GameObject.Find("Player").GetComponent<EntityAttributes>();
            speedIncrease = myEntityAttributes.moveSpeed / 100f * moveSpeedIncrease;
            Debug.Log("increased speed from: " + myEntityAttributes.moveSpeed + ", by: " + speedIncrease);
            myEntityAttributes.moveSpeed += speedIncrease;

            CustomEvents.OnDamageDelt += IncreaseDamage;
            
        }

        private void IncreaseDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthManager)
        {
            int additionalDamage = 0;
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                additionalDamage += (int)(damage * damageIncreaseMajor / 100f);
                Debug.Log("increased major damage from: " + damage + ", by: " + additionalDamage);
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
            myEntityAttributes.moveSpeed -= speedIncrease;
            CustomEvents.OnDamageDelt -= IncreaseDamage;
        }
    }
}