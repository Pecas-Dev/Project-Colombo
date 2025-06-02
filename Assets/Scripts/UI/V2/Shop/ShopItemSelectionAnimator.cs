using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using ProjectColombo.Inventory;
using UnityEngine.EventSystems;
using ProjectColombo.GameManagement;


namespace ProjectColombo.Shop
{
    public class ShopItemSelectionAnimator : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("Animation Settings")]
        [SerializeField] float animationDuration = 0.3f;
        [SerializeField] float selectedScale = 1.2f;
        [SerializeField] Ease animationEase = Ease.OutBack;
        [SerializeField] bool enableDebugLogs = false;

        [Header("Color Settings")]
        [SerializeField] bool enableColorChange = true;
        [SerializeField] Color unselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("Target Image")]
        [SerializeField] Image targetImage;

        Vector3 originalScale;
        Color originalColor;
        Tween currentScaleTween;
        Tween currentColorTween;
        bool isInitialized = false;
        bool isSelected = false;

        ShopItems shopItems;
        ShopPotion shopPotion;

        #region Unity Lifecycle

        void Awake()
        {
            InitializeComponent();
        }

        void Start()
        {
            if (!isInitialized)
            {
                InitializeComponent();
            }
        }

        void OnDestroy()
        {
            currentScaleTween?.Kill();
            currentColorTween?.Kill();
        }

        #endregion

        #region Initialization

        void InitializeComponent()
        {
            if (targetImage == null)
            {
                FindTargetImage();
            }

            shopItems = GetComponent<ShopItems>();
            shopPotion = GetComponent<ShopPotion>();

            if (targetImage != null)
            {
                originalScale = targetImage.transform.localScale;
                originalColor = targetImage.color;

                SetInitialColorState();

                isInitialized = true;
                LogDebug($"Initialized with target image: {targetImage.name}");
            }
            else
            {
                LogError("No target image found for selection animation!");
            }
        }

        void FindTargetImage()
        {
            ShopItems shopItemsComponent = GetComponent<ShopItems>();

            if (shopItemsComponent != null && shopItemsComponent.referenceImage != null)
            {
                targetImage = shopItemsComponent.referenceImage;
                LogDebug("Found target image from ShopItems component");
                return;
            }

            Image[] images = GetComponentsInChildren<Image>();

            foreach (Image image in images)
            {
                if (image.name.ToLower().Contains("reference") ||
                    image.name.ToLower().Contains("icon") ||
                    image.name.ToLower().Contains("item") ||
                    image.name.ToLower().Contains("potion"))
                {
                    targetImage = image;
                    LogDebug($"Found target image by name: {image.name}");
                    return;
                }
            }

            if (images.Length > 1)
            {
                targetImage = images[1];
                LogDebug("Using second image as target (assuming first is background)");
            }
            else if (images.Length > 0)
            {
                targetImage = images[0];
                LogDebug("Using first image as target");
            }
        }

        void SetInitialColorState()
        {
            if (!enableColorChange)
            {
                return;
            }

            bool isAvailable = IsItemAvailable();

            if (!isAvailable)
            {
                return;
            }
            else
            {
                targetImage.color = unselectedColor;
            }
        }

        #endregion

        #region Public Methods

        public void SetTargetImage(Image image)
        {
            if (targetImage != null)
            {
                currentScaleTween?.Kill();
                currentColorTween?.Kill();
                targetImage.transform.localScale = originalScale;
            }

            targetImage = image;
            if (targetImage != null)
            {
                originalScale = targetImage.transform.localScale;
                originalColor = targetImage.color;

                SetInitialColorState();

                isInitialized = true;
                LogDebug($"Target image set to: {targetImage.name}");
            }
        }

        public void ForceSelect()
        {
            if (targetImage != null && isInitialized)
            {
                AnimateToSelected();
            }
        }

        public void ForceDeselect()
        {
            if (targetImage != null && isInitialized)
            {
                AnimateToDeselected();
            }
        }

        public void RefreshColorState()
        {
            if (!isInitialized || !enableColorChange)
            {
                return;
            }

            bool isAvailable = IsItemAvailable();

            if (!isAvailable)
            {
                return;
            }

            if (isSelected)
            {
                SetColorImmediate(originalColor);
            }
            else
            {
                SetColorImmediate(unselectedColor);
            }
        }

        #endregion

        #region Event System Handlers

        public void OnSelect(BaseEventData eventData)
        {
            if (!isInitialized)
            {
                InitializeComponent();
            }

            if (targetImage != null)
            {
                isSelected = true;
                AnimateToSelected();
                LogDebug("Button selected - animating to selected state");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (targetImage != null && isInitialized)
            {
                isSelected = false;
                AnimateToDeselected();
                LogDebug("Button deselected - animating to original state");
            }
        }

        #endregion

        #region Animation Methods

        void AnimateToSelected()
        {
            currentScaleTween?.Kill();
            currentColorTween?.Kill();

            Vector3 targetScale = originalScale * selectedScale;
            currentScaleTween = targetImage.transform.DOScale(targetScale, animationDuration).SetEase(animationEase).SetUpdate(true);

            if (enableColorChange && IsItemAvailable())
            {
                currentColorTween = targetImage.DOColor(originalColor, animationDuration).SetEase(animationEase).SetUpdate(true);
            }
        }

        void AnimateToDeselected()
        {
            currentScaleTween?.Kill();
            currentColorTween?.Kill();

            currentScaleTween = targetImage.transform.DOScale(originalScale, animationDuration).SetEase(animationEase).SetUpdate(true);

            if (enableColorChange && IsItemAvailable())
            {
                currentColorTween = targetImage.DOColor(unselectedColor, animationDuration).SetEase(animationEase).SetUpdate(true);
            }
        }

        void SetColorImmediate(Color color)
        {
            currentColorTween?.Kill();
            if (targetImage != null)
            {
                targetImage.color = color;
            }
        }

        #endregion

        #region Availability Checking

        bool IsItemAvailable()
        {
            if (shopItems != null)
            {
                ShopScreen shopScreen = shopItems.GetComponentInParent<ShopScreen>();
                if (shopScreen != null)
                {
                    return shopItems.item.price <= shopScreen.GetCurrency();
                }
            }

            if (shopPotion != null)
            {
                PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (playerInventory != null)
                {
                    return playerInventory.currencyAmount >= shopPotion.price;
                }
            }

            return true;
        }

        #endregion

        #region Debug Logging

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF6600>[ShopItemSelectionAnimator] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[ShopItemSelectionAnimator] {message}");
        }

        #endregion
    }
}