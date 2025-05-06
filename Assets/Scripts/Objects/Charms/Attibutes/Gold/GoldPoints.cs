using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class GoldPoints : BaseAttributes
    {
        public int amountOfGold;

        PlayerInventory myPlayerInventory;

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            myPlayerInventory.currencyAmount += amountOfGold;
        }

        public override void Disable()
        {
            
        }
    }
}