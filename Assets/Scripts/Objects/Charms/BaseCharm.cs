
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public enum RARITY { COMMON = 0, RARE, LEGENDARY }
    public abstract class BaseCharm : MonoBehaviour
    {
        public string charmName;
        public RARITY charmRarity;
        public Texture2D charmPicture;
        [TextArea] public string charmDescription;
        [TextArea] public string charmLore;


        public abstract void Equip();
        public abstract void Remove();
    }
}
