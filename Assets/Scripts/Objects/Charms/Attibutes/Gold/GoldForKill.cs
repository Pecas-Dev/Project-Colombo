using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class GoldForKill : BaseAttributes
    {
        public int minAmountOfGold;
        public int maxAmountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            CustomEvents.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale arg2)
        {
            int rand = Random.Range(minAmountOfGold, maxAmountOfGold + 1);
            Debug.Log("extra gold for kill: " + rand);
            myPlayerInventory.currencyAmount += rand;
        }

        public override void Disable()
        {
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
        }
    }
}