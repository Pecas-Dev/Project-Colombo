using ProjectColombo.Objects.Masks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectColombo.UI
{ 
    public class MaskCanvas : MonoBehaviour
    {
        public TMP_Text maskNameText;
        public TMP_Text maskDescriptionText;
        public TMP_Text echoDescriptionText;

        GameObject lastSelected;

        private void Start()
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
            UpdateTexts();
        }

        private void Update()
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != lastSelected)
            {
                lastSelected = current;
                UpdateTexts();
            }
        }

        void UpdateTexts()
        {
            BaseMask mask = lastSelected.GetComponent<MaskButton>().maskPrefab.GetComponent<BaseMask>();

            maskNameText.text = mask.maskName;
            //could have lore maskLoreText.text = mask.maskLore;
            maskDescriptionText.text = mask.maskDescription;
            echoDescriptionText.text = mask.echoDescription;
        }
    }
}