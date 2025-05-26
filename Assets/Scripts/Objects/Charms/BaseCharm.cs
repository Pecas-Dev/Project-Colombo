
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

        BaseAttributes[] myAttributes;


        public void Equip()
        {
            if (!firstTimeEquiped)
            {
                firstTimeEquiped = true;
                CustomEvents.CharmFirstTimeEquipped();
            }


            myAttributes = GetComponents<BaseAttributes>();

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
            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Disable();
            }
        }
    }
}
