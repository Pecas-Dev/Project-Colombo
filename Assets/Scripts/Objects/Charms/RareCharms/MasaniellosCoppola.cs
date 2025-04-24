using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class MassaniellosCoppola : BaseCharm
    {
        public float damageResistancePercentage = 10;
        public float failedParryBonus = 25;

        public override void Equip()
        {
            CustomEvents.OnDamageReceived += DecreaseDamageGeneral;
            CustomEvents.OnParryFailed += DecreaseDamageParry;
        }

        private void DecreaseDamageGeneral(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int extraDamage = (int)(damage / 100f * damageResistancePercentage);
            Debug.Log("decreased damage from: " + damage + ", by: " + extraDamage);
            healthmanager.TakeDamage(-extraDamage);
        }

        private void DecreaseDamageParry(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager, bool sameScale)
        {
            int extraDamage = (int)(damage / 100f * failedParryBonus);
            Debug.Log("decrease failed parry damage from: " + damage + ", by: " + extraDamage);
            healthmanager.TakeDamage(-extraDamage);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageReceived -= DecreaseDamageGeneral;
            CustomEvents.OnParryFailed -= DecreaseDamageParry;
        }
    }
}