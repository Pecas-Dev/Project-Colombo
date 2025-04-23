using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Masks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuUI : MonoBehaviour
    {
        public ToggleGroup charmToggleGroup;
        public ToggleGroup maskToggleGroup;
        public ToggleGroup itemToggleGroup;

        public GameObject CharmSlot;
        public GameObject MaskSlot;
        public GameObject ItemSlot;


        public void MakeSelection()
        {
            // Remove existing children
            foreach (Transform child in CharmSlot.transform)
            {
                BaseCharm childcharm = child.GetComponent<BaseCharm>();
                childcharm.Remove();
                GameObject.Destroy(child.gameObject);
            }

            // Remove existing children
            foreach (Transform child in MaskSlot.transform)
            {
                child.GetComponent<BaseMask>().Remove();
                GameObject.Destroy(child.gameObject);

            }

            // Remove existing children
            foreach (Transform child in ItemSlot.transform)
            {
                GameObject.Destroy(child.gameObject);

            }


            //ProcessSelection(charmToggleGroup, CharmSlot);
            Toggle charmtoggle = charmToggleGroup.ActiveToggles().FirstOrDefault();

            if (charmtoggle != null)
            {
                GameObject charm = charmtoggle.GetComponent<PauseToggle>().selection;
            
                if (charm != null)
                {

                    // Instantiate a copy of the selected toggle into the slot
                    GameObject newToggle = GameObject.Instantiate(charm.gameObject, CharmSlot.transform);
                    newToggle.transform.localPosition = Vector3.zero;
                    newToggle.transform.localRotation = Quaternion.identity;
                    newToggle.transform.localScale = Vector3.one;
                    BaseCharm charmscript = newToggle.GetComponent<BaseCharm>();
                    charmscript.Equip();
                }
            }


            //ProcessSelection(maskToggleGroup, MaskSlot);
            Toggle masktoggle = maskToggleGroup.ActiveToggles().FirstOrDefault();
            if (masktoggle != null)
            {
                GameObject mask = masktoggle.GetComponent<PauseToggle>().selection;

                if (mask != null)
                {


                    // Instantiate a copy of the selected toggle into the slot
                    GameObject newToggle = GameObject.Instantiate(mask.gameObject, MaskSlot.transform);
                    newToggle.transform.localPosition = Vector3.zero;
                    newToggle.transform.localRotation = Quaternion.identity;
                    newToggle.transform.localScale = Vector3.one;
                    newToggle.GetComponent<BaseMask>().Equip();
                }
            }

            //ProcessSelection(itemToggleGroup, ItemSlot);
            Toggle itemtoggle = itemToggleGroup.ActiveToggles().FirstOrDefault();
            if (itemtoggle != null)
            {
                GameObject item = itemtoggle.GetComponent<PauseToggle>().selection;

                if (item != null)
                {


                    // Instantiate a copy of the selected toggle into the slot
                    GameObject newToggle = GameObject.Instantiate(item.gameObject, ItemSlot.transform);
                    newToggle.transform.localPosition = Vector3.zero;
                    newToggle.transform.localRotation = Quaternion.identity;
                    newToggle.transform.localScale = Vector3.one;
                }
            }
        }
    }
}