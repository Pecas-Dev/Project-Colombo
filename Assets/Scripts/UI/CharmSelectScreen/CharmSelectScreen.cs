using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ProjectColombo.UI
{
    public class CharmSelectScreen : MonoBehaviour
    {
        public GameObject newCharm;
        public List<GameObject> charmButtons;
        public GameObject legendaryCharmButton;

        public Image newCharmImage;
        public TMP_Text newCharmNameText;
        public TMP_Text newCharmDescriptionText;

        public Image selectedCharmImage;
        public TMP_Text selectedCharmNameText;
        public TMP_Text selectedCharmDescriptionText;

        bool active = true;
        GameObject lastSelected;

        private void Start()
        {
            active = true;
        }

        private void Update()
        {
            if (active)
            {
                GameObject selection = EventSystem.current.currentSelectedGameObject;
                if (selection != lastSelected)
                {
                    CharmButton button = selection.GetComponent<CharmButton>();
                    if (button == null) return;

                    BaseCharm charm = button.charmObject.GetComponent<BaseCharm>();
                    if (charm == null) return;

                    selectedCharmImage.sprite = charm.charmPicture;
                    selectedCharmNameText.text = charm.charmName;
                    //could add lore: selectedCharmLoreText.text = charm.charmLore;
                    selectedCharmDescriptionText.text = charm.charmDescription;
                }
            }
        }

        void UpdateFromInventory()
        {
            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();

            int i = 0;
            for (; i < inventory.charms.Count; i++)
            {
                charmButtons[i].GetComponent<CharmButton>().UpdateInfo(inventory.charms[i]);
            }

            while (i < charmButtons.Count)
            {
                charmButtons[i].GetComponent<Button>().enabled = false;
                i++;
            }

            if (inventory.legendaryCharms.Count != 0)
            {
                    legendaryCharmButton.GetComponent<CharmButton>().UpdateInfo(inventory.legendaryCharms[0]);
            }
        }

        public void ActivateScreen(GameObject newCharmObj)
        {
            active = true;
            lastSelected = null;
            UpdateFromInventory();
            EventSystem.current.SetSelectedGameObject(charmButtons[0].gameObject);

            newCharm = newCharmObj;
            BaseCharm newCharmInfo = newCharm.GetComponent<BaseCharm>();
            newCharmImage.sprite = newCharmInfo.charmPicture;
            newCharmNameText.text = newCharmInfo.charmName;
            //could add lore: newCharmLoreText.text = newCharmInfo.charmLore;
            newCharmDescriptionText.text = newCharmInfo.charmDescription;
        }

        public void DeactivateScreen()
        {
            active = false;
            gameObject.SetActive(false);
            newCharm = null;
            GameManager.Instance.ResumeGame();
        }

        public void AddCharm(GameObject charmToRemove)
        {
            active = false;
            GameManager.Instance.GetComponent<PlayerInventory>().ReplaceCharm(charmToRemove, newCharm);
            gameObject.SetActive(false);
            newCharm = null;
            GameManager.Instance.ResumeGame();
        }
    }
}