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
            Debug.Log("increased stamina from: " + myStamina.maxStamina + ", by: " + addedStamina);
            myStamina.maxStamina += addedStamina;
        }

        private void Evade(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int rand = Random.Range(0, 101);
            Debug.Log("try to evade");

            if (rand < evadeChance)
            {
                healthmanager.TakeDamage(-damage);
                Debug.Log("evaded");
            }
        }

        private void DoubleGold(int amount)
        {
            int rand = Random.Range(0, 101);
            Debug.Log("try to double gold " + amount);

            if (rand < doubleGoldChance)
            {
                GameObject.Find("Player").GetComponent<PlayerInventory>().currencyAmount += amount;
                Debug.Log("doubled gold");
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