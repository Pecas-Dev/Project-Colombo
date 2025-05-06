
using System.Collections.Generic;
using ProjectColombo.Objects.Charms;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class BaseMask : MonoBehaviour
    {
        public string maskName;
        public Sprite maskPicture;
        [TextArea] public string maskDescription;
        [TextArea] public string echoDescription;
        [TextArea] public string maskLore;

        [ReadOnlyInspector] public bool echoUnlocked = false;
        [ReadOnlyInspector] public bool abilityAvailable = false;
        [HideInInspector] public float currentAbilityCooldown = 0;
        [HideInInspector] public float timer = 0;

        public List<BaseAttributes> attribBeforeEcho;
        public List<BaseAttributes> attribAfterEcho;

        public void Equip()
        {
            if (!echoUnlocked)
            {
                foreach (BaseAttributes attrib in attribBeforeEcho)
                {
                    attrib.Enable();
                }
            }
            else
            {
                foreach (BaseAttributes attrib in attribAfterEcho)
                {
                    attrib.Enable();
                }
            }
        }

        public void UseAbility()
        {

        }

        public void UnlockEcho()
        {
            foreach (BaseAttributes attrib in attribBeforeEcho)
            {
                attrib.Disable();
            }

            foreach (BaseAttributes attrib in attribAfterEcho)
            {
                attrib.Enable();
            }
        }

        public void Remove()
        {
            foreach (BaseAttributes attrib in attribBeforeEcho)
            {
                attrib.Disable();
            }

            foreach (BaseAttributes attrib in attribAfterEcho)
            {
                attrib.Disable();
            }
        }
    }
}