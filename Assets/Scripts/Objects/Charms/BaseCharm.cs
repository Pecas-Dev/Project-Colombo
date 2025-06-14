
using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public enum RARITY { COMMON = 0, RARE, LEGENDARY }
    public class BaseCharm : MonoBehaviour
    {
        public string charmName;
        public RARITY charmRarity;
        public Sprite charmPicture;
        [TextArea] public string charmDescription;
        [TextArea] public string charmLore;

        public GameObject abilityObject;
        bool firstTimeEquiped = false;

        public bool equipped = false;
        AttributesStatSheet myStats;
        BaseAttributes[] myAttributes;

        private void Start()
        {
            myAttributes = GetComponents<BaseAttributes>();
            myStats = GetComponent<AttributesStatSheet>();
            myStats.ResetStats();

            foreach (var attrib in myAttributes)
            {
                attrib.UpdateStatSheed(myStats);
            }
        }

        public AttributesStatSheet GetStats()
        {
            myStats.ResetStats();
            if (!equipped) return myStats;

            foreach (var attrib in myAttributes)
            {
                attrib.UpdateStatSheed(myStats);
            }

            return myStats;
        }

        public void Equip()
        {
            if (equipped) return;
            equipped = true;

            if (!firstTimeEquiped)
            {
                firstTimeEquiped = true;
                CustomEvents.CharmFirstTimeEquipped();
            }



            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Enable();
            }
        }

        public GameObject GetAbility()
        {
            return abilityObject;
        }

        public void Remove()
        {
            if (!equipped) return;
            equipped = false;

            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Disable();
            }
        }
    }
}
