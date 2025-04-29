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
        public List<GameObject> CharmButtons;

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
                    BaseCharm charm = selection.GetComponent<CharmButton>().charmObject.GetComponent<BaseCharm>();
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

            int slot = 0;
            foreach (GameObject charm in inventory.charms)
            {
                CharmButtons[slot].GetComponent<CharmButton>().UpdateInfo(charm);
                slot++;
            }
        }

        public void ActivateScreen(GameObject newCharmObj)
        {
            UpdateFromInventory();
            EventSystem.current.SetSelectedGameObject(CharmButtons[0].gameObject);

            newCharm = newCharmObj;
            BaseCharm newCharmInfo = newCharm.GetComponent<BaseCharm>();
            newCharmImage.sprite = newCharmInfo.charmPicture;
            newCharmNameText.text = newCharmInfo.charmName;
            //could add lore: newCharmLoreText.text = newCharmInfo.charmLore;
            newCharmDescriptionText.text = newCharmInfo.charmDescription;
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