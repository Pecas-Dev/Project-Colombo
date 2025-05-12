using ProjectColombo.GameManagement.Events;
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

        public GameObject attribBeforeEcho;
        public GameObject attribAfterEcho;
        BaseAttributes[] attribBeforeEchoList;
        BaseAttributes[] attribAfterEchoList;

        public BaseEchoMissions echoMission;
        public GameObject abilityObject;

        public void Equip()
        {
            attribBeforeEchoList = attribBeforeEcho.GetComponents<BaseAttributes>();
            attribAfterEchoList = attribAfterEcho.GetComponents<BaseAttributes>();

            if (!echoUnlocked)
            {
                foreach (BaseAttributes attrib in attribBeforeEchoList)
                {
                    attrib.Enable();
                }
            }
            else
            {
                foreach (BaseAttributes attrib in attribAfterEchoList)
                {
                    attrib.Enable();
                }
            }

            if (!echoUnlocked)
            {
                echoMission.Enable();
            }
        }

        public GameObject GetAbility()
        {
            return abilityObject;
        }

        public void UnlockEcho()
        {
            echoUnlocked = true;
            echoMission.Disable();
            CustomEvents.EchoUnlocked();

            foreach (BaseAttributes attrib in attribBeforeEchoList)
            {
                attrib.Disable();
            }

            foreach (BaseAttributes attrib in attribAfterEchoList)
            {
                attrib.Enable();
            }
        }

        public void Remove()
        {
            if (!echoUnlocked)
            {
                foreach (BaseAttributes attrib in attribBeforeEchoList)
                {
                    attrib.Disable();
                }
            }
            else
            {
                foreach (BaseAttributes attrib in attribAfterEchoList)
                {
                    attrib.Disable();
                }
            }

            echoMission.Disable();
        }
    }
}