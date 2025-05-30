using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class LuckPoints : BaseAttributes
    {
        public int extraLuck;

        PlayerInventory myPlayerInventory;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.luckPoints += extraLuck;
        }

        public override void Enable()
        {
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            Debug.Log("extra Luck added: " + extraLuck);
            myPlayerInventory.AddLuckPoints(extraLuck);
        }

        public override void Disable()
        {
            Debug.Log("extra Luck removed: " + extraLuck);
            if (myPlayerInventory == null)
            {
                Debug.Log("no inventory in luck");
                return;
            }

            myPlayerInventory.AddLuckPoints(-extraLuck);
        }
    }
}