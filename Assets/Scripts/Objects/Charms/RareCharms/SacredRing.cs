using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class SacredRing : BaseCharm
    {
        public float evadeChance = 20;
        public int addedStamina = 1;
        public float doubleGoldChance = 25;
        Stamina myStamina;
        public override void Equip()
        {
            CustomEvents.OnDamageReceived += Evade;
            CustomEvents.OnCoinsCollected += DoubleGold;

            myStamina = GameObject.Find("Player").GetComponent<Stamina>();
            myStamina.maxStamina += addedStamina;
        }

        private void Evade(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int rand = Random.Range(0, 101);

            if (rand < evadeChance)
            {
                healthmanager.TakeDamage(-damage);
            }
        }

        private void DoubleGold(int amount)
        {
            int rand = Random.Range(0, 101);

            if (rand < doubleGoldChance)
            {
                GameObject.Find("Player").GetComponent<PlayerInventory>().currencyAmount += amount;
            }
        }
       

        public override void Remove()
        {
            CustomEvents.OnDamageReceived -= Evade;
            CustomEvents.OnCoinsCollected -= DoubleGold;

            myStamina.maxStamina -= addedStamina;
        }
    }
}