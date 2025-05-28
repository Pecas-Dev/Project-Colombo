using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using System.Xml.Serialization;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class BaseMask : MonoBehaviour
    {
        public string maskName;
        public Sprite maskPicture;
        [TextArea] public string maskDescription;
        [TextArea] public string maskDescriptionAfterEcho;
        [TextArea] public string echoDescription;
        [TextArea] public string maskLore;

        [ReadOnlyInspector] public bool echoUnlocked = false;

        public GameObject attribBeforeEcho;
        public GameObject attribAfterEcho;
        BaseAttributes[] attribBeforeEchoList;
        BaseAttributes[] attribAfterEchoList;
        AttributesStatSheet myStats;

        public BaseEchoMissions echoMission;
        public GameObject abilityObject;

        private void Start()
        {
            attribBeforeEchoList = attribBeforeEcho.GetComponents<BaseAttributes>();
            attribAfterEchoList = attribAfterEcho.GetComponents<BaseAttributes>(); 
            myStats = GetComponent<AttributesStatSheet>();
            myStats.ResetStats();

            if (!echoUnlocked)
            {
                foreach (var attrib in attribBeforeEchoList)
                {
                    attrib.UpdateStatSheed(myStats);
                }
            }
            else
            {
                foreach (var attrib in attribAfterEchoList)
                {
                    attrib.UpdateStatSheed(myStats);
                }
            }
        }

        public AttributesStatSheet GetStats()
        {
            myStats.ResetStats();

            if (!echoUnlocked)
            {
                foreach (var attrib in attribBeforeEchoList)
                {
                    attrib.UpdateStatSheed(myStats);
                }
            }
            else
            {
                foreach (var attrib in attribAfterEchoList)
                {
                    attrib.UpdateStatSheed(myStats);
                }
            }

            return myStats;
        }

        public void Equip()
        {


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
            if (echoUnlocked == true) return;

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

        public void ResetEchoMission()
        {
            Debug.Log($"[BaseMask] {maskName}: ResetEchoMission called. Current echoUnlocked: {echoUnlocked}");

            if (echoMission != null)
            {
                Debug.Log($"[BaseMask] {maskName}: Disabling echo mission");
                echoMission.Disable();

                Debug.Log($"[BaseMask] {maskName}: Resetting mission progress");
                echoMission.ResetProgress();

                Debug.Log($"[BaseMask] {maskName}: Re-enabling echo mission");
                echoMission.Enable();
            }
            else
            {
                Debug.LogError($"[BaseMask] {maskName}: echoMission is null!");
            }
        }
    }
}