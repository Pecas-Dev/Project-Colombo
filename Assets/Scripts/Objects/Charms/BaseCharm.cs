
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

        private void Start()
        {
            Equip();
        }

        private void OnDestroy()
        {
            Remove();
        }

        public abstract void Equip();
        public abstract void Remove();
    }
}