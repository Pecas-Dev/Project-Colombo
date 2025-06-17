using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectColombo.Objects.Charms;

namespace ProjectColombo.UI
{
    public class CharmButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [ReadOnlyInspector] public GameObject charmObject;
        public Image imageSlot;

        [Header("Pause Menu Integration")]
        [SerializeField] public bool isPauseMenuButton = false;
        [SerializeField] bool keepOriginalImage = true;

        Pausescreen.PauseMenuInventoryManager inventoryManager;
        Sprite originalSprite;
        [HideInInspector] public BaseCharm charmComponent;

        void Awake()
        {
            if (isPauseMenuButton)
            {
                inventoryManager = FindFirstObjectByType<Pausescreen.PauseMenuInventoryManager>();
            }

            if (imageSlot != null)
            {
                originalSprite = imageSlot.sprite;
            }
        }

        public void UpdateInfo(GameObject charm)
        {
            charmObject = charm;
            charmComponent = charm != null ? charm.GetComponent<BaseCharm>() : null;

            if (imageSlot != null)
            {
                if (charm != null && charmComponent != null && charmComponent.charmPicture != null)
                {
                    imageSlot.sprite = charmComponent.charmPicture;
                    imageSlot.enabled = true;
                }
                else if (keepOriginalImage && originalSprite != null)
                {
                    imageSlot.sprite = originalSprite;
                    imageSlot.enabled = true;
                }
                else
                {
                    imageSlot.sprite = null;
                    imageSlot.enabled = false;
                }
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            UpdateDisplayInfo();
        }

        public void OnDeselect(BaseEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UpdateDisplayInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        void UpdateDisplayInfo()
        {
            if (!isPauseMenuButton || inventoryManager == null)
            {
                return;
            }

            if (charmObject != null && charmComponent != null)
            {
                inventoryManager.UpdateCharmInfo(charmObject);
                Debug.Log($"Selected charm: {charmComponent.charmName} (Rarity: {charmComponent.charmRarity})");
            }
            else
            {
                inventoryManager.ShowEmptyCharmInfo();
            }
        }

        public void UpdateInfoWithEmptySprite(GameObject charm, Sprite emptySprite)
        {
            charmObject = charm;
            charmComponent = charm != null ? charm.GetComponent<BaseCharm>() : null;

            if (imageSlot != null)
            {
                if (charm != null && charmComponent != null && charmComponent.charmPicture != null)
                {
                    imageSlot.sprite = charmComponent.charmPicture;
                    imageSlot.enabled = true;
                }
                else if (emptySprite != null)
                {
                    imageSlot.sprite = emptySprite;
                    imageSlot.enabled = true;
                }
                else if (keepOriginalImage && originalSprite != null)
                {
                    imageSlot.sprite = originalSprite;
                    imageSlot.enabled = true;
                }
                else
                {
                    imageSlot.sprite = null;
                    imageSlot.enabled = false;
                }
            }
        }
    }
}