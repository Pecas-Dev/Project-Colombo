using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;
using ProjectColombo.Inventory;

namespace ProjectColombo.UI
{
    public class PickUpScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject pickUpCanvas;
        [SerializeField] Image charmImage;
        [SerializeField] TextMeshProUGUI charmTitleText;
        [SerializeField] TextMeshProUGUI charmDescriptionText;

        //[Header("Animation Settings")]
        //[SerializeField] Animator transitionAnimator;
        //[SerializeField] string openAnimationName = "BrushIn";
        //[SerializeField] string closeAnimationName = "BrushOut";
        //[SerializeField] float animationDuration = 1f;
        //[SerializeField] bool useAnimations = true;

        [Header("Text Rarity Colors")]
        [SerializeField] Color defaultTextColor = Color.white;
        [SerializeField] Color commonTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] Color rareTextColor = new Color(0.0f, 0.5f, 1.0f, 1f);
        [SerializeField] Color legendaryTextColor = new Color(1.0f, 0.84f, 0.0f, 1f);

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        bool isActive = false;
        bool isPlayingAnimation = false;

        GameObject currentCharmObject;
        PlayerInventory playerInventory;
        CharmSwapMenuController charmSwapController;
        UINavigationManager navigationManager;

        void Awake()
        {
            Initialize();
        }

        void Start()
        {
            navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager == null)
            {
                LogError("UINavigationManager not found!");
            }

            playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                LogError("PlayerInventory not found!");
            }

            charmSwapController = GameManager.Instance.CharmSwapMenuCtrl;
            if (charmSwapController == null)
            {
                LogError("CharmSwapMenuController not found!");
            }
        }

        void Update()
        {
            if (!isActive || isPlayingAnimation)
            {
                return;
            }

            HandleAcceptInput();
            HandleCancelInput();
        }

        void Initialize()
        {
            if (pickUpCanvas != null)
            {
                pickUpCanvas.SetActive(false);
            }

            LogDebug("PickUpScreenController initialized");
        }

        void HandleAcceptInput()
        {
            bool shouldAccept = false;

            // Check UI Submit input action
            if (gameInput != null && gameInput.inputActions.UI.Submit.WasPressedThisFrame())
            {
                shouldAccept = true;
                LogDebug("Accept input detected via UI.Submit");
            }

            // Check gamepad South button
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                shouldAccept = true;
                LogDebug("Accept input detected via Gamepad South button");
            }

            if (shouldAccept)
            {
                ProcessAcceptAction();
            }
        }

        void HandleCancelInput()
        {
            bool shouldCancel = false;

            if (gameInput != null && gameInput.inputActions.UI.Cancel.WasPressedThisFrame())
            {
                shouldCancel = true;
                LogDebug("Cancel input detected via UI.Cancel");
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                shouldCancel = true;
                LogDebug("Cancel input detected via Gamepad East button");
            }

            if (shouldCancel)
            {
                ProcessCancelAction();
            }
        }

        void ProcessAcceptAction()
        {
            if (currentCharmObject == null)
            {
                LogError("No charm object to accept!");
                return;
            }

            LogDebug("Player accepted the charm - proceeding with pickup");

            //if (useAnimations && transitionAnimator != null)
            //{
            //    PlayClosingAnimationSequence(() => {
            //        AddCharmToInventory();
            //    });
            //}
            //else
            //{
            HidePickUpScreen();
            AddCharmToInventory();
            //}
        }

        void ProcessCancelAction()
        {
            LogDebug("Player cancelled the charm pickup - dropping charm back into world");

            if (currentCharmObject != null)
            {
                DropCharmIntoWorld(currentCharmObject);
            }

            HidePickUpScreen();
            CompleteCancel();
        }

        void DropCharmIntoWorld(GameObject charmObject)
        {
            if (charmObject == null)
            {
                LogError("Cannot drop null charm object!");
                return;
            }

            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            if (charmComponent == null)
            {
                LogError("Charm object does not have BaseCharm component!");
                return;
            }

            Transform playerTransform = GameObject.Find("Player")?.transform;

            if (playerTransform == null)
            {
                LogError("Player not found! Cannot drop charm.");
                return;
            }

            Vector3 dropPosition = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
            //dropPosition += playerTransform.forward * 2f;

            DropManager dropManager = GameManager.Instance.GetComponent<DropManager>();

            if (dropManager == null)
            {
                LogError("DropManager not found on GameManager!");
                return;
            }

            dropManager.DropCharm(charmObject, dropPosition);

            LogDebug($"Successfully dropped charm '{charmComponent.charmName}' back into world at position {dropPosition}");
        }

        void AddCharmToInventory()
        {
            if (currentCharmObject == null || playerInventory == null)
            {
                LogError("Cannot add charm to inventory - missing references!");
                CompleteCancel();
                return;
            }

            BaseCharm charmComponent = currentCharmObject.GetComponent<BaseCharm>();
            if (charmComponent == null)
            {
                LogError("Charm object does not have BaseCharm component!");
                CompleteCancel();
                return;
            }

            LogDebug($"Adding charm to inventory: {charmComponent.charmName}");

            // Use the existing PlayerInventory logic to determine if we need charm swap
            RARITY newCharmRarity = charmComponent.charmRarity;

            // Check if we need to open charm swap screen
            bool needsCharmSwap = DetermineIfCharmSwapNeeded(newCharmRarity);

            if (needsCharmSwap)
            {
                LogDebug("Charm swap needed - will open charm swap screen");

                // IMPORTANT: Hide pick up screen BEFORE opening charm swap
                HidePickUpScreen();

                // Store the charm reference in the charm swap controller for proper handling
                if (charmSwapController != null)
                {
                    // Set the charm swap controller to handle this charm
                    charmSwapController.SetPendingCharm(currentCharmObject);
                }

                // Determine which charm swap mode to use
                if (newCharmRarity == RARITY.LEGENDARY && playerInventory.HasLegendaryCharmEquipped())
                {
                    LogDebug("Opening charm swap in legendary mode");
                    GameManager.Instance.PauseGame(false);
                    charmSwapController.ActivateScreenLegendaryMode(currentCharmObject);
                }
                else
                {
                    LogDebug("Opening charm swap in normal mode");
                    GameManager.Instance.PauseGame(false);
                    charmSwapController.ActivateScreen(currentCharmObject);
                }
            }
            else
            {
                LogDebug("No charm swap needed - adding directly to inventory");
                // Add directly to inventory using existing method
                playerInventory.AddCharmDirectly(currentCharmObject);

                // Hide pickup screen and return to player input
                HidePickUpScreen();
                ResumeGameplay();
            }

            // Clear the current charm reference since we've handed it off
            currentCharmObject = null;
        }

        bool DetermineIfCharmSwapNeeded(RARITY newCharmRarity)
        {
            // Use reflection to access the private field if needed, or make sure it's public
            int currentCharmAmount = GetCurrentCharmAmount();
            int maxCharms = playerInventory.maxCharms;

            if (currentCharmAmount >= maxCharms)
            {
                LogDebug("Inventory is full - charm swap needed");
                return true;
            }

            if (newCharmRarity == RARITY.LEGENDARY && playerInventory.HasLegendaryCharmEquipped())
            {
                LogDebug("New legendary charm + existing legendary charm - charm swap needed");
                return true;
            }

            LogDebug("Inventory has space and no conflicts - no charm swap needed");
            return false;
        }

        int GetCurrentCharmAmount()
        {
            // Helper method to get current charm amount
            // This counts the actual charms in the inventory
            int count = 0;

            if (playerInventory.charms != null)
            {
                count += playerInventory.charms.Count;
            }

            if (playerInventory.legendaryCharms != null)
            {
                count += playerInventory.legendaryCharms.Count;
            }

            return count;
        }

        void CompleteCancel()
        {
            currentCharmObject = null;
            ResumeGameplay();
        }

        void ResumeGameplay()
        {
            LogDebug("Resuming gameplay");

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
            }

            GameManager.Instance.ResumeGame();
        }

        public void ShowPickUpScreen(GameObject charmObject)
        {
            if (charmObject == null)
            {
                LogError("Cannot show pick up screen with null charm object!");
                return;
            }

            BaseCharm charmComponent = charmObject.GetComponent<BaseCharm>();
            if (charmComponent == null)
            {
                LogError("Charm object does not have BaseCharm component!");
                return;
            }

            LogDebug($"Showing pick up screen for charm: {charmComponent.charmName}");

            currentCharmObject = charmObject;
            isActive = true;

            UpdateCharmDisplay(charmComponent);

            // Switch to UI input
            if (gameInput != null)
            {
                gameInput.SwitchToUI();
            }

            // Set navigation state
            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.PickUpScreen);
            }

            // Show the UI
            gameObject.SetActive(true);

            //if (useAnimations && transitionAnimator != null)
            //{
            //    PlayOpeningAnimationSequence();
            //}
            //else
            //{
            ShowPickUpCanvas();
            //}

            LogDebug("Pick up screen activated");
        }

        void UpdateCharmDisplay(BaseCharm charmComponent)
        {
            if (charmImage != null && charmComponent.charmPicture != null)
            {
                charmImage.sprite = charmComponent.charmPicture;
            }

            if (charmTitleText != null)
            {
                string rarityText = GetRarityDisplayName(charmComponent.charmRarity);
                string fullTitle = $"{charmComponent.charmName} ({rarityText})";

                charmTitleText.text = fullTitle;
                charmTitleText.color = GetRarityTextColor(charmComponent.charmRarity);

                LogDebug($"Set charm title to: {fullTitle}");
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = charmComponent.charmDescription;
            }
        }

        string GetRarityDisplayName(RARITY rarity)
        {
            switch (rarity)
            {
                case RARITY.COMMON:
                    return "Common";
                case RARITY.RARE:
                    return "Rare";
                case RARITY.LEGENDARY:
                    return "Legendary";
                default:
                    return "Unknown";
            }
        }

        Color GetRarityTextColor(RARITY rarity)
        {
            switch (rarity)
            {
                case RARITY.COMMON:
                    return commonTextColor;
                case RARITY.RARE:
                    return rareTextColor;
                case RARITY.LEGENDARY:
                    return legendaryTextColor;
                default:
                    return defaultTextColor;
            }
        }

        void PlayOpeningAnimationSequence()
        {
            LogDebug("Starting opening animation sequence...");

            isPlayingAnimation = true;

            //if (transitionAnimator != null)
            //{
            //    if (!transitionAnimator.gameObject.activeInHierarchy)
            //    {
            //        transitionAnimator.gameObject.SetActive(true);
            //    }

            //    transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            //    transitionAnimator.Play(openAnimationName);
            //    LogDebug($"Playing opening animation: {openAnimationName}");

            //    Invoke("OnOpeningAnimationComplete", animationDuration);
            //}
            //else
            //{
            OnOpeningAnimationComplete();
            //}
        }

        void OnOpeningAnimationComplete()
        {
            LogDebug("Opening animation completed - showing pick up canvas");

            isPlayingAnimation = false;
            ShowPickUpCanvas();

            LogDebug("Opening animation sequence completed");
        }

        void PlayClosingAnimationSequence(System.Action onComplete = null)
        {
            LogDebug("Starting closing animation sequence...");

            isPlayingAnimation = true;

            HidePickUpCanvas();

            //if (transitionAnimator != null)
            //{
            //    if (!transitionAnimator.gameObject.activeInHierarchy)
            //    {
            //        transitionAnimator.gameObject.SetActive(true);
            //    }

            //    transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            //    transitionAnimator.Play(closeAnimationName);
            //    LogDebug($"Playing closing animation: {closeAnimationName}");

            //    System.Action completeAction = () => OnClosingAnimationComplete(onComplete);
            //    Invoke("InvokeClosingComplete", animationDuration);
            //    currentClosingAction = completeAction;
            //}
            //else
            //{
            OnClosingAnimationComplete(onComplete);
            //}
        }

        System.Action currentClosingAction;

        void InvokeClosingComplete()
        {
            currentClosingAction?.Invoke();
        }

        void OnClosingAnimationComplete(System.Action onComplete = null)
        {
            LogDebug("Closing animation completed");

            isPlayingAnimation = false;
            HidePickUpScreen();

            onComplete?.Invoke();

            LogDebug("Closing animation sequence completed");
        }

        void ShowPickUpCanvas()
        {
            if (pickUpCanvas != null)
            {
                pickUpCanvas.SetActive(true);
                LogDebug("Pick up canvas activated");
            }
        }

        void HidePickUpCanvas()
        {
            if (pickUpCanvas != null)
            {
                pickUpCanvas.SetActive(false);
                LogDebug("Pick up canvas hidden");
            }
        }

        void HidePickUpScreen()
        {
            isActive = false;
            HidePickUpCanvas();
            gameObject.SetActive(false);
            LogDebug("Pick up screen hidden");
        }

        public void Show()
        {
            if (pickUpCanvas != null)
            {
                pickUpCanvas.SetActive(true);
            }

            isActive = true;
            LogDebug("PickUpScreen shown");
        }

        public void Hide()
        {
            if (pickUpCanvas != null)
            {
                pickUpCanvas.SetActive(false);
            }

            isActive = false;
            LogDebug("PickUpScreen hidden");
        }

        public void ForceHidePickUpScreen()
        {
            LogDebug("ForceHidePickUpScreen called - hiding pickup screen immediately");

            isActive = false;
            currentCharmObject = null;
            HidePickUpCanvas();
            gameObject.SetActive(false);

            LogDebug("Pick up screen force hidden");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FF88>[PickUpScreenController] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[PickUpScreenController] {message}");
        }
    }
}