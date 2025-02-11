using UnityEngine;

namespace ProjectColombo.Inventory.Collectable
{
    [CreateAssetMenu(fileName = "CollectableData", menuName = "Scriptable Objects/Collectibles")]

    public class CollectableData : ScriptableObject
    {
        public enum CollectibleType { Currency, Charm, Mask };
        public enum CharmType { Health, Attack};
        public enum MaskType { Default, Rare };

        //apply to all
        public CollectibleType type;
        public Color itemColor;
        //public int dropChance;
        public bool isPickable = true;

        //currency
        public int minAmount;
        public int maxAmount;

        //charms
        public CharmType charmType;
        public int valueModifierInPercent;

        //health
        public MaskType maskType;
        
    }
}
