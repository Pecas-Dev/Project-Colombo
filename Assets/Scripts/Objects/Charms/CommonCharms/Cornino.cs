using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class Cornino : BaseCharm
    {
        public int luckIncrease = 8;
        PlayerInventory myPlayerInventory;

        public override void Equip()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            myPlayerInventory.currentLuck += luckIncrease;
            Debug.Log("increased luck by: " + luckIncrease + ", to: " + myPlayerInventory.currentLuck);
        }

        public override void Remove()
        {
            myPlayerInventory.currentLuck -= luckIncrease;
            Debug.Log("decreased luck by: " + luckIncrease + ", to: " + myPlayerInventory.currentLuck);
        }
    }
}