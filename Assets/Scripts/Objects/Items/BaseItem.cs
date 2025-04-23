
using UnityEngine;

namespace ProjectColombo.Objects.Items
{
    public enum RARITY { COMMON = 0, RARE, LEGENDARY }
    public abstract class BaseItem : MonoBehaviour
    {
        public string itemName;
        public RARITY itemRarity;
        public Texture2D itemPicture;
        [TextArea] public string itemDescription;
        [TextArea] public string itemLore;

        public abstract void Activate();
    }
}