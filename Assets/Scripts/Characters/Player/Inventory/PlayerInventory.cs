using ProjectColombo.Combat;
using ProjectColombo.Inventory.Collectable;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        //health from healthmanager
        public int currencyAmount = 0;
        public string currentWeapon;

        public Dictionary<CollectableData.CharmType, int> charms = new();
        public Dictionary<CollectableData.MaskType, int> masks = new();

        private void Awake() //doing this will make all possible charms and masks visible in the inventory from the start
        {
            foreach (CollectableData.CharmType charm in System.Enum.GetValues(typeof(CollectableData.CharmType)))
            {
                charms[charm] = 0;
            }

            foreach (CollectableData.MaskType mask in System.Enum.GetValues(typeof(CollectableData.MaskType)))
            {
                masks[mask] = 0;
            }

            //please change to weaponattribures....
            //ChangeWeapon(GetComponentInChildren<WeaponAttributes>().name); 

            ChangeWeapon(GetComponentInChildren<WeaponAttributes>().name); 
        }

        public void ChangeWeapon(string newWeaponName)
        {
            currentWeapon = newWeaponName;
        }

        public void AddCurrency(int amount)
        {
            currencyAmount += amount;
        }

        public void AddCharm(CollectableData.CharmType charm)
        {
            //if (!charms.ContainsKey(charm)) //use if all charms should not be visible from the beginning
            //{
            //    charms[charm] = 0;
            //}

            charms[charm]++;
        }

        public void AddMask(CollectableData.MaskType mask)
        {
            //if (!masks.ContainsKey(mask)) //use if all charms should not be visible from the beginning
            //{
            //    masks[mask] = 0;
            //}

            masks[mask]++;
        }
    }
}