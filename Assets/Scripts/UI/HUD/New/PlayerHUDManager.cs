using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Masks;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.UI
{
    public class PlayerHUDManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the TextMeshPro component displaying currency amount")]
        [SerializeField] TextMeshProUGUI currencyText;

        [Tooltip("Reference to the Image component displaying the equipped mask")]
        [SerializeField] Image maskImage;


        Sprite currentMaskSprite = null;


        PlayerInventory playerInventory;


        int currentCurrencyAmount = -1;


        void Awake()
        {
            if (currencyText == null)
            {
                Debug.LogError("Currency Text reference is missing in PlayerHUDManager!");
            }

            if (maskImage == null)
            {
                Debug.LogError("Mask Image reference is missing in PlayerHUDManager!");
            }
        }

        void Start()
        {
            CustomEvents.OnCoinsCollected += HandleCoinsCollected;
            CustomEvents.OnMaskCollected += HandleMaskCollected;
            CustomEvents.OnLevelChange += HandleLevelChanged;

            FindPlayerReferences();

            UpdateUI();
        }

        void OnDestroy()
        {
            CustomEvents.OnCoinsCollected -= HandleCoinsCollected;
            CustomEvents.OnMaskCollected -= HandleMaskCollected;
            CustomEvents.OnLevelChange -= HandleLevelChanged;
        }

        void FindPlayerReferences()
        {
            if (GameManager.Instance != null)
            {
                playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

                if (playerInventory == null)
                {
                    Debug.LogWarning("PlayerInventory not found on GameManager, checking for direct access...");
                }
            }
            else
            {
                Debug.LogWarning("GameManager instance not found, attempting to find PlayerInventory directly...");
            }

            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();

                if (playerInventory == null)
                {
                    Debug.LogError("PlayerInventory not found in scene! HUD updates will not work correctly.");
                }
            }
        }

        void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                UpdateUI();
            }
        }

        void HandleCoinsCollected(int amount)
        {
            Debug.Log($"Coins collected: {amount}");
            UpdateCurrencyDisplay();
        }

        void HandleMaskCollected(GameObject mask)
        {
            Debug.Log($"Mask collected: {mask.name}");
            Invoke("UpdateMaskDisplay", 0.1f);
        }

        void HandleLevelChanged()
        {
            Debug.Log("Level changed, updating UI");
            Invoke("FindPlayerReferences", 0.5f);
            Invoke("UpdateUI", 1.0f);
        }


        void UpdateUI()
        {
            UpdateCurrencyDisplay();
            UpdateMaskDisplay();
        }


        void UpdateCurrencyDisplay()
        {
            if (currencyText == null) return;

            int currentAmount = 0;

            if (playerInventory != null)
            {
                currentAmount = playerInventory.currencyAmount;
            }
            else if (GameManager.Instance != null)
            {
                var inventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    currentAmount = inventory.currencyAmount;
                }
            }

            if (currentAmount != currentCurrencyAmount)
            {
                currentCurrencyAmount = currentAmount;
                currencyText.text = currentAmount.ToString();
                Debug.Log($"Updated currency display: {currentAmount}");
            }
        }

        void UpdateMaskDisplay()
        {
            if (maskImage == null) return;

            GameObject maskSlot = null;

            if (playerInventory != null && playerInventory.maskSlot != null)
            {
                maskSlot = playerInventory.maskSlot;
            }
            else if (GameManager.Instance != null)
            {
                var inventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.maskSlot != null)
                {
                    maskSlot = inventory.maskSlot;
                }
            }

            if (maskSlot != null && maskSlot.transform.childCount > 0)
            {
                BaseMask equippedMask = maskSlot.transform.GetChild(0).GetComponent<BaseMask>();

                if (equippedMask != null)
                {
                    if (equippedMask.maskPicture != currentMaskSprite)
                    {
                        RectTransform rectTransform = maskImage.rectTransform;
                        rectTransform.sizeDelta = new Vector2(180, 180);

                        currentMaskSprite = equippedMask.maskPicture;
                        maskImage.sprite = currentMaskSprite;
                        maskImage.enabled = true;
                        Debug.Log($"Updated mask display: {equippedMask.maskName}");
                    }
                }
            }
            else
            {
                if (currentMaskSprite != null)
                {
                    currentMaskSprite = null;
                    maskImage.sprite = null;
                    maskImage.enabled = false;
                    Debug.Log("No mask equipped, hiding mask display");
                }
            }
        }

        public void ForceUpdateHUD()
        {
            FindPlayerReferences();
            UpdateUI();
        }
    }
}