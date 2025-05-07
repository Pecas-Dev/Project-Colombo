using ProjectColombo.Objects.Charms;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class WineChaliceAbility : BaseAbility
    {
        public GameObject attribs;
        BaseAttributes[] myAttributes;

        public override void UseAbility()
        {
            myAttributes = attribs.GetComponents<BaseAttributes>();

            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Enable();
            }
        }

        public override void EndAbility()
        {
            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Enable();
            }
        }
    }
}


//Activable Ability: Drunken Warrior[120 Seconds Cooldown]
//For the next 5 seconds:
//-10 % Attack speed
//- 8 % Movement speed
//- 15 % Stamina regeneration speed
//+18% Damage dealt
//You heal by 6% of damage dealt
