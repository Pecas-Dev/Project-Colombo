using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuUI : MonoBehaviour
    {
        public List<GameObject> charmButtons;
        public GameObject legendaryCharmButton;
        public TMP_Text charmNameText;
        public TMP_Text charmDescriptionText;

        GameObject lastSelected;

        private void Start()
        {
            UpdateCharms();
        }

        private void Update()
        {
            GameObject selection = EventSystem.current.currentSelectedGameObject;

            if (selection == null) return;

            if (selection != lastSelected)
            {
                CharmButton charm = selection.GetComponent<CharmButton>();

                if (charm == null)
                {
                    charmNameText.text = "";
                    charmDescriptionText.text = "";
                    return;
                }

                GameObject charmobj = charm.charmObject;
                if (charmobj == null) return;

                BaseCharm charminfo = charm.charmObject.GetComponent<BaseCharm>();
                charmNameText.text = charminfo.charmName;
                //could add lore: charmLoreText.text = charminfo.charmLore;
                charmDescriptionText.text = charminfo.charmDescription;
            }
        }


        public void UpdateCharms()
        {
            foreach (GameObject b in charmButtons)
            {
                b.GetComponent<Button>().interactable = false;
            }

            legendaryCharmButton.GetComponent<Button>().interactable = false;

            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();

            int slot = 0;
            foreach (GameObject charm in inventory.charms)
            {
                charmButtons[slot].GetComponent<Button>().interactable = true;
                charmButtons[slot].GetComponent<CharmButton>().UpdateInfo(charm);
                slot++;
            }

            for (int i = slot; i < charmButtons.Count; i++)
            {
                charmButtons[i].GetComponent<CharmButton>().UpdateInfo(null);
            }

            if (inventory.legendaryCharms.Count != 0)
            {
                legendaryCharmButton.GetComponent<Button>().interactable = true;
                legendaryCharmButton.GetComponent<CharmButton>().UpdateInfo(inventory.legendaryCharms[0]);
            }
            else
            {
                legendaryCharmButton.GetComponent<CharmButton>().UpdateInfo(null);
            }
        }
    }
}