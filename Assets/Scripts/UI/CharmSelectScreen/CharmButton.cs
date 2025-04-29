using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectColombo.UI
{
    public class CharmButton : MonoBehaviour
    {
        [HideInInspector] public GameObject charmObject; //set from charmselectscreen
        public Image imageSlot;

        public void UpdateInfo(GameObject charm)
        {
            charmObject = charm;
            imageSlot.sprite = charmObject.GetComponent<BaseCharm>().charmPicture;

        }

        public void AddCharm()
        {
            GetComponentInParent<CharmSelectScreen>().AddCharm(charmObject);
        }
    }
}