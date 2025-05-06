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
        [ReadOnlyInspector] public bool abilityAvailable = false;
        [HideInInspector] public float currentAbilityCooldown = 0;
        [HideInInspector] public float timer = 0;

        public GameObject attribBeforeEcho;
        public GameObject attribAfterEcho;
        BaseAttributes[] attribBeforeEchoList;
        BaseAttributes[] attribAfterEchoList;

        public BaseEchoMissions echoMission;

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

        public void UseAbility()
        {

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
            foreach (BaseAttributes attrib in attribBeforeEchoList)
            {
                attrib.Disable();
            }

            foreach (BaseAttributes attrib in attribAfterEchoList)
            {
                attrib.Disable();
            }

            echoMission.Disable();
        }
    }
}