using ProjectColombo.Objects.Charms;
using System.Collections;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class TrinacriaAbility : BaseAbility
    {
        public float invincibleDuration;
        public GameObject attribs;
        BaseAttributes[] myAttributes;

        public override void UseAbility()
        {
            myAttributes = attribs.GetComponents<BaseAttributes>();

            StartCoroutine(FirstStep());
        }

        public override void EndAbility()
        {
            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Disable();
            }
        }

        IEnumerator FirstStep()
        {
            myPlayerStateMachine.myHealthManager.ignoreDamage = true;

            yield return new WaitForSeconds(invincibleDuration);

            myPlayerStateMachine.myHealthManager.ignoreDamage = false;

            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Enable();
            }
        }


    }
}


//Activable Ability: Heart of Trinacria [160 seconds Cooldown]
//For the next 2 seconds, ignore all incoming damage.
//After the effect vanishes, for the next 4 seconds:
//-10% Damage dealt
//-6% Movement speed
//Receive +8% damage from all sources

