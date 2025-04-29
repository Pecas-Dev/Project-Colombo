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
            Debug.Log("increased luck from: " + myPlayerInventory.currentLuck + ", by: " + luckIncrease);
            myPlayerInventory.currentLuck += luckIncrease;
        }

        public override void Remove()
        {
            myPlayerInventory.currentLuck -= luckIncrease;
        }
    }
}