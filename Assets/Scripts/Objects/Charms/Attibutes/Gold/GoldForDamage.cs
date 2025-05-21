using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForDamage : BaseAttributes
    {
        public int minAmountOfGold;
        public int maxAmountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnDamageDelt += OnDamageDelt;
        }

        private void OnDamageDelt(int arg1, GameGlobals.MusicScale arg2, bool sameScale, Combat.HealthManager arg3, int comboLength)
        {
            int rand = Random.Range(minAmountOfGold, maxAmountOfGold + 1);
            Debug.Log("extra gold for damage: " + rand);
            CustomEvents.CoinsCollected(rand);
            myPlayerInventory.currencyAmount += rand;
        }

        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}