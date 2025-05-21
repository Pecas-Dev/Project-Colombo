using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectColombo.Objects.Charms;

namespace ProjectColombo.UI
{
    public class CharmButton : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [ReadOnlyInspector] public GameObject charmObject; 
        public Image imageSlot;

        [Header("Pause Menu Integration")]
        [SerializeField] public bool isPauseMenuButton = false;
        [SerializeField] bool keepOriginalImage = true;

         Pausescreen.PauseMenuInventoryManager inventoryManager;
         Sprite originalSprite; 

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

            if (imageSlot != null)
            {
                if (charm != null)
                {
                    BaseCharm charmComponent = charm.GetComponent<BaseCharm>();
                    if (charmComponent != null && charmComponent.charmPicture != null)
                    {
                        imageSlot.sprite = charmComponent.charmPicture;
                        imageSlot.enabled = true;
                    }
                    else if (keepOriginalImage && originalSprite != null)
                    {
                        imageSlot.sprite = originalSprite;
                        imageSlot.enabled = true;
                    }
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

        public void AddCharm()
        {
            CharmSelectScreen selectScreen = GetComponentInParent<CharmSelectScreen>();
            if (selectScreen != null && charmObject != null)
            {
                selectScreen.AddCharm(charmObject);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (isPauseMenuButton && inventoryManager != null)
            {
                inventoryManager.UpdateCharmInfo(charmObject);
                Debug.Log($"CharmButton selected: {name}, has charm: {(charmObject != null ? "yes" : "no")}");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
        }
    }
}