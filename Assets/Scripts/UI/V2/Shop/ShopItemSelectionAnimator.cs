using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
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

        Color originalColor = Color.white;

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

            Invoke(nameof(SetInitialColorStateDelayed), 0.1f);
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
                if (image.name.ToLower().Contains("reference") ||image.name.ToLower().Contains("icon") || image.name.ToLower().Contains("item") ||image.name.ToLower().Contains("potion"))
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

        void SetInitialColorStateDelayed()
        {
            if (!enableColorChange || !isInitialized) 
            {
return;
            }

            bool isAvailable = IsItemAvailable();

            if (isAvailable)
            {
                targetImage.color = unselectedColor;
                LogDebug("Set initial state to unselected color for available item");
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
                LogDebug("Item unavailable - not refreshing color state");
                return;
            }

            if (isSelected)
            {
                SetColorImmediate(originalColor);
                LogDebug("Refreshed to original color (selected available item)");
            }
            else
            {
                SetColorImmediate(unselectedColor);
                LogDebug("Refreshed to unselected color (unselected available item)");
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

                if (shopItems != null)
                {
                    shopItems.CheckActive(); 
                }

                StartCoroutine(DelayedAnimateToSelected());
                LogDebug("Button selected - scheduling delayed animation");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (targetImage != null && isInitialized)
            {
                isSelected = false;

                if (shopItems != null)
                {
                    shopItems.CheckActive(); 
                }

                StartCoroutine(DelayedAnimateToDeselected());
                LogDebug("Button deselected - scheduling delayed animation");
            }
        }

        #endregion

        #region Animation Methods

        IEnumerator DelayedAnimateToSelected()
        {
            yield return null; 
            AnimateToSelected();
        }

        IEnumerator DelayedAnimateToDeselected()
        {
            yield return null; 
            AnimateToDeselected();
        }

        void AnimateToSelected()
        {
            if (!IsItemAvailable())
            {
                LogDebug("Item unavailable after forced check - only animating scale");
                currentScaleTween?.Kill();
                Vector3 _targetScale = originalScale * selectedScale;
                currentScaleTween = targetImage.transform.DOScale(_targetScale, animationDuration).SetEase(animationEase) .SetUpdate(true);
                return;
            }

            currentScaleTween?.Kill();
            currentColorTween?.Kill();

            Vector3 targetScale = originalScale * selectedScale;
            currentScaleTween = targetImage.transform   .DOScale(targetScale, animationDuration)  .SetEase(animationEase)  .SetUpdate(true);

            if (enableColorChange)
            {
                currentColorTween = targetImage  .DOColor(originalColor, animationDuration)   .SetEase(animationEase) .SetUpdate(true);
                LogDebug("Animating to selected state (available item)");
            }
        }

        void AnimateToDeselected()
        {
            if (!IsItemAvailable())
            {
                LogDebug("Item unavailable after forced check - only animating scale");
                currentScaleTween?.Kill();
                currentScaleTween = targetImage.transform.DOScale(originalScale, animationDuration) .SetEase(animationEase)  .SetUpdate(true);
                return;
            }

            currentScaleTween?.Kill();
            currentColorTween?.Kill();

            currentScaleTween = targetImage.transform .DOScale(originalScale, animationDuration).SetEase(animationEase) .SetUpdate(true);

            if (enableColorChange)
            {
                currentColorTween = targetImage.DOColor(unselectedColor, animationDuration).SetEase(animationEase).SetUpdate(true);
                LogDebug("Animating to deselected state (available item)");
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
                    bool available = shopItems.item.price <= shopScreen.GetCurrency();
                    LogDebug($"ShopItems availability check: {available} (price: {shopItems.item.price}, currency: {shopScreen.GetCurrency()})");
                    return available;
                }
            }

            if (shopPotion != null)
            {
                PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (playerInventory != null)
                {
                    bool available = playerInventory.currencyAmount >= shopPotion.price;
                    LogDebug($"ShopPotion availability check: {available} (price: {shopPotion.price}, currency: {playerInventory.currencyAmount})");
                    return available;
                }
            }

            LogDebug("No shop component found - defaulting to available");
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