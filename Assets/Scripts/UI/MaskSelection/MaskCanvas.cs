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
        public TMP_Text echoNameText;
        public TMP_Text echoAbilityText;

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
            if (lastSelected == null) return;
            MaskButton button = lastSelected.GetComponent<MaskButton>();
            if (button == null) return;

            BaseMask mask = button.maskPrefab.GetComponent<BaseMask>();

            maskNameText.text = mask.maskName;
            //could have lore maskLoreText.text = mask.maskLore;
            maskDescriptionText.text = mask.maskDescription;
            echoNameText.text = mask.echoDescription;
            echoAbilityText.text = mask.abilityObject.GetComponent<BaseAbility>().abilityDescription;
        }
    }
}