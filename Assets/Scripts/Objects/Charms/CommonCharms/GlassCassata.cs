using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class GlassCassata : BaseCharm
    {
        public float resistancePercentage = 5;

        public override void Equip()
        {
            CustomEvents.OnDamageReceived += AddResistance;
        }

        private void AddResistance(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int calculatedResistance = (int)(damage / 100f * resistancePercentage);
            Debug.Log("decreased damage from: " + damage + ", to: " + calculatedResistance);
            healthmanager.TakeDamage(-calculatedResistance);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageReceived -= AddResistance;
        }
    }
}