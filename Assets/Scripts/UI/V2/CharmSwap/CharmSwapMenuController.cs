using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using ProjectColombo.Inventory;
using UnityEngine.EventSystems;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameManagement;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI
{
    public class CharmSwapMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject charmSwapCanvas;
        [SerializeField] Button[] charmButtons;
        [SerializeField] Button legendaryCharmButton;

        [Header("New Charm Display")]
        [SerializeField] Image newCharmImage;
        [SerializeField] TMP_Text newCharmNameText;
        [SerializeField] TMP_Text newCharmDescriptionText;

        [Header("Selected Charm Display")]
        //[SerializeField] Image selectedCharmImage;
        [SerializeField] TMP_Text selectedCharmNameText;
        [SerializeField] TMP_Text selectedCharmDescriptionText;

        [Header("Legendary Mode UI")]
        [SerializeField] GameObject[] slotBlockerImages;

        [Header("Visual Feedback")]
        [SerializeField] GameObject[] slotSelectors;
        [SerializeField] float selectorPulseSpeed = 2f;
        [SerializeField] float selectorMinAlpha = 0.5f;
        [SerializeField] float selectorMaxAlpha = 1f;

        //[Header("Animation Settings")]
        //[SerializeField] Animator transitionAnimator;
        //[SerializeField] string openAnimationName = "BrushIn";
        //[SerializeField] string closeAnimationName = "BrushOut";
        //[SerializeField] float animationDuration = 1f;
        //[SerializeField] bool useAnimations = true;

        [Header("Selector Colors")]
        [SerializeField] Color defaultSelectorColor = Color.black;
        [SerializeField] Color commonSelectorColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] Color rareSelectorColor = new Color(0.0f, 0.5f, 1.0f, 1f);
        [SerializeField] Color legendarySelectorColor = new Color(1.0f, 0.84f, 0.0f, 1f);

        [Header("Text Rarity Colors")]
        [SerializeField] Color defaultTextColor = Color.white;
        [SerializeField] Color commonTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] Color rareTextColor = new Color(0.0f, 0.5f, 1.0f, 1f);
        [SerializeField] Color legendaryTextColor = new Color(1.0f, 0.84f, 0.0f, 1f);

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;


        int currentSelectedIndex = -1;
        int legendarySlotIndex = -1;

        bool isActive = false;
        bool wasActiveBeforePause = false;
        bool isPlayingAnimation = false;
        bool cameFromPickUpScreen = false;
        public bool isLegendaryReplacementMode = false;


        IEnumerator[] selectorAnimations;


        Color[] originalSelectorColors;

        GameObject currentNewCharm;
        GameObject tempCharmToReplace;
        GameObject lastSelectedGameObject;

        UINavigationManager navigationManager;
        PickUpScreenController pickUpScreenController;

        public bool WasActiveBeforePause => wasActiveBeforePause;


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
        }

        void Update()
        {
            if (!isActive)
            {
                return;
            }

            HandleSelectionChange();
            HandleCancelInput();
            HandleSubmitInput();
        }

        void Initialize()
        {
            legendarySlotIndex = 0;

            if (slotSelectors != null && slotSelectors.Length > 0)
            {
                selectorAnimations = new IEnumerator[slotSelectors.Length];
                originalSelectorColors = new Color[slotSelectors.Length];

                for (int i = 0; i < slotSelectors.Length; i++)
                {
                    if (slotSelectors[i] != null)
                    {
                        Image selectorImage = slotSelectors[i].GetComponent<Image>();
                        if (selectorImage != null)
                        {
                            originalSelectorColors[i] = selectorImage.color;
                        }

                        slotSelectors[i].SetActive(false);
                    }
                }
            }

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] == null) continue;

                int slotIndex = i + 1;
                charmButtons[i].onClick.RemoveAllListeners();
                charmButtons[i].onClick.AddListener(() => OnCharmButtonClicked(i));

                SetupButtonEventTriggers(charmButtons[i], slotIndex);
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.onClick.RemoveAllListeners();
                legendaryCharmButton.onClick.AddListener(() => OnLegendaryCharmButtonClicked());

                SetupButtonEventTriggers(legendaryCharmButton, legendarySlotIndex);
            }

            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(false);
            }

            LogDebug("CharmSwapMenuController initialized with visual feedback system");
        }

        void SetupButtonEventTriggers(Button button, int buttonIndex)
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();

            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }

            eventTrigger.triggers.Clear();


            EventTrigger.Entry selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener((data) =>
            {
                SelectSlot(buttonIndex);
                LogDebug($"Button {buttonIndex} selected via Select event");
            });
            eventTrigger.triggers.Add(selectEntry);


            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };

            pointerEnterEntry.callback.AddListener((data) =>
            {
                SelectSlot(buttonIndex);
                LogDebug($"Button {buttonIndex} selected via PointerEnter event");
            });
            eventTrigger.triggers.Add(pointerEnterEntry);


            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };

            pointerExitEntry.callback.AddListener((data) =>
            {
                if (currentSelectedIndex == buttonIndex)
                {
                    UpdateSelectionVisuals(buttonIndex);
                }
            });
            eventTrigger.triggers.Add(pointerExitEntry);
        }

        void HandleSubmitInput()
        {
            bool shouldSubmit = false;

            if (gameInput != null && gameInput.inputActions.UI.Submit.WasPressedThisFrame())
            {
                shouldSubmit = true;
                LogDebug("Submit input detected via UI.Submit");
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                shouldSubmit = true;
                LogDebug("Submit input detected via Gamepad South button");
            }

            if (shouldSubmit)
            {
                ProcessSubmitAction();
            }
        }

        void ProcessSubmitAction()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null)
            {
                LogDebug("No object currently selected for submit action");
                return;
            }

            LogDebug($"Processing submit action for: {currentSelected.name}, legendary mode: {isLegendaryReplacementMode}");

            if (currentSelected == legendaryCharmButton?.gameObject)
            {
                LogDebug("Submit action on legendary button");
                OnLegendaryCharmButtonClicked();
                return;
            }

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (currentSelected == charmButtons[i]?.gameObject)
                {
                    if (isLegendaryReplacementMode)
                    {
                        LogDebug($"Blocking submit action on regular button {i} in legendary mode");
                        return;
                    }

                    LogDebug($"Submit action on regular button {i}");
                    OnCharmButtonClicked(i);
                    return;
                }
            }

            LogDebug("Submit action on unrecognized object");
        }

        void HandleSelectionChange()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != lastSelectedGameObject)
            {
                int selectedSlotIndex = -1;

                if (currentSelected == legendaryCharmButton?.gameObject)
                {
                    selectedSlotIndex = 0;
                    LogDebug($"Legendary button selected - slotIndex: {selectedSlotIndex}");
                }
                else
                {
                    for (int i = 0; i < charmButtons.Length; i++)
                    {
                        if (currentSelected == charmButtons[i]?.gameObject)
                        {
                            selectedSlotIndex = i + 1;
                            LogDebug($"Regular button {i} selected - slotIndex: {selectedSlotIndex}");
                            break;
                        }
                    }
                }

                if (selectedSlotIndex >= 0)
                {
                    SelectSlot(selectedSlotIndex);
                }

                lastSelectedGameObject = currentSelected;
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
                DeactivateScreen();
            }
        }

        void OnButtonSelected(int buttonIndex)
        {
            SelectSlot(buttonIndex);
        }

        void UpdateSelectedCharmDisplay(GameObject selectedGameObject)
        {
            if (selectedGameObject == null)
            {
                return;
            }

            Button selectedButton = selectedGameObject.GetComponent<Button>();

            if (selectedButton == null)
            {
                return;
            }

            UpdateSelectedCharmDisplayFromButton(selectedButton);
        }

        void UpdateSelectedCharmDisplayFromButton(Button button)
        {
            CharmButton charmButton = button.GetComponent<CharmButton>();

            if (charmButton == null || charmButton.charmObject == null)
            {
                if (selectedCharmNameText != null)
                {
                    selectedCharmNameText.text = "";
                }
                if (selectedCharmDescriptionText != null)
                {
                    selectedCharmDescriptionText.text = "";
                }
                return;
            }

            BaseCharm charm = charmButton.charmObject.GetComponent<BaseCharm>();

            if (charm == null)
            {
                return;
            }

            //if (selectedCharmImage != null)
            //{
            //    selectedCharmImage.sprite = charm.charmPicture;
            //}

            if (selectedCharmNameText != null)
            {
                string rarityText = GetRarityDisplayName(charm.charmRarity);
                string fullTitle = $"{charm.charmName} ({rarityText})";

                selectedCharmNameText.text = fullTitle;
                selectedCharmNameText.color = GetRarityTextColor(charm.charmRarity);

                LogDebug($"Set selected charm title to: {fullTitle}");
            }

            if (selectedCharmDescriptionText != null)
            {
                selectedCharmDescriptionText.text = charm.charmDescription;
            }

            LogDebug($"Updated selected charm display: {charm.charmName} ({GetRarityDisplayName(charm.charmRarity)})");
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

        void UpdateFromInventory()
        {
            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                LogError("PlayerInventory not found!");
                return;
            }

            ResetAllButtonColors();

            int i = 0;
            for (; i < inventory.charms.Count && i < charmButtons.Length; i++)
            {
                CharmButton charmButton = charmButtons[i].GetComponent<CharmButton>();
                if (charmButton != null)
                {
                    charmButton.UpdateInfo(inventory.charms[i]);
                    SetButtonColorForCharm(charmButtons[i], inventory.charms[i]);
                }
            }

            while (i < charmButtons.Length)
            {
                charmButtons[i].interactable = false;
                i++;
            }

            if (legendaryCharmButton != null && inventory.legendaryCharmSlot.transform.childCount > 0)
            {
                GameObject charmInLegendarySlot = inventory.legendaryCharmSlot.transform.GetChild(0).gameObject;
                CharmButton legendaryCharmButtonComponent = legendaryCharmButton.GetComponent<CharmButton>();

                if (legendaryCharmButtonComponent != null && charmInLegendarySlot != null)
                {
                    legendaryCharmButtonComponent.UpdateInfo(charmInLegendarySlot);
                    SetButtonColorForCharm(legendaryCharmButton, charmInLegendarySlot);
                    legendaryCharmButton.interactable = true;
                    LogDebug($"Updated legendary button with charm: {charmInLegendarySlot.GetComponent<BaseCharm>().charmName}");
                }
            }
            else if (legendaryCharmButton != null)
            {
                legendaryCharmButton.interactable = false;
                LogDebug("No charm in legendary slot, disabled legendary button");
            }

            LogDebug("Updated charm swap display from inventory with correct slot mapping");
        }

        public void SetPendingCharm(GameObject charmObject)
        {
            currentNewCharm = charmObject;
            cameFromPickUpScreen = true;

            if (pickUpScreenController == null)
            {
                pickUpScreenController = GameManager.Instance.PickUpScreenCtrl;
            }

            LogDebug($"Set pending charm from pickup screen: {charmObject.GetComponent<BaseCharm>().charmName}");
        }

        void SetButtonColorForCharm(Button button, GameObject charmObject)
        {
            if (button == null || charmObject == null) return;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage == null) return;

            BaseCharm charm = charmObject.GetComponent<BaseCharm>();
            if (charm == null) return;

            Color newColor = Color.white;

            switch (charm.charmRarity)
            {
                case RARITY.COMMON:
                    newColor = commonSelectorColor;
                    break;
                case RARITY.RARE:
                    newColor = rareSelectorColor;
                    break;
                case RARITY.LEGENDARY:
                    newColor = legendarySelectorColor;
                    break;
                default:
                    newColor = Color.white;
                    break;
            }

            float currentAlpha = buttonImage.color.a;
            buttonImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);

            LogDebug($"Set button color for charm {charm.charmName} (Rarity: {charm.charmRarity})");
        }

        void OnCharmButtonClicked(int buttonIndex)
        {
            LogDebug($"OnCharmButtonClicked called for button {buttonIndex}, legendary mode: {isLegendaryReplacementMode}");

            if (isLegendaryReplacementMode)
            {
                LogDebug($"Blocking regular charm button {buttonIndex} click in legendary replacement mode");
                return;
            }

            if (buttonIndex < 0 || buttonIndex >= charmButtons.Length || currentNewCharm == null)
            {
                LogDebug($"Invalid conditions: buttonIndex={buttonIndex}, charmButtons.Length={charmButtons.Length}, currentNewCharm={currentNewCharm != null}");
                return;
            }

            CharmButton charmButton = charmButtons[buttonIndex].GetComponent<CharmButton>();
            if (charmButton == null || charmButton.charmObject == null)
            {
                LogDebug($"CharmButton component or charmObject is null for button {buttonIndex}");
                return;
            }

            LogDebug($"Processing charm replacement for button {buttonIndex} in normal mode");

            //if (useAnimations && transitionAnimator != null)
            //{
            //    GameObject charmToReplace = charmButton.charmObject;

            //    if (charmSwapCanvas != null)
            //    {
            //        charmSwapCanvas.SetActive(false);
            //    }

            //    isPlayingAnimation = true;
            //    transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            //    transitionAnimator.Play(closeAnimationName);

            //    Invoke("CompleteCharmSwap", animationDuration);

            //    tempCharmToReplace = charmToReplace;
            //}
            //else
            //{
            AddCharm(charmButton.charmObject);
            //}

            LogDebug($"Initiated charm replacement for slot {buttonIndex}");
        }

        void OnLegendaryCharmButtonClicked()
        {
            LogDebug($"OnLegendaryCharmButtonClicked called, legendary mode: {isLegendaryReplacementMode}");

            if (currentNewCharm == null)
            {
                LogDebug("No new charm to swap");
                return;
            }

            CharmButton charmButton = legendaryCharmButton.GetComponent<CharmButton>();
            if (charmButton == null || charmButton.charmObject == null)
            {
                LogDebug("Legendary button has no charm component or charm object");
                return;
            }

            LogDebug("Processing legendary charm replacement");

            //if (useAnimations && transitionAnimator != null)
            //{
            //    GameObject charmToReplace = charmButton.charmObject;

            //    if (charmSwapCanvas != null)
            //    {
            //        charmSwapCanvas.SetActive(false);
            //    }

            //    isPlayingAnimation = true;
            //    transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            //    transitionAnimator.Play(closeAnimationName);

            //    Invoke("CompleteCharmSwap", animationDuration);

            //    tempCharmToReplace = charmToReplace;
            //}
            //else
            //{
            AddCharm(charmButton.charmObject);
            //}

            LogDebug("Initiated legendary charm replacement");
        }

        public void ActivateScreen(GameObject newCharmObject)
        {
            if (newCharmObject == null)
            {
                LogError("Cannot activate charm swap screen with null charm object!");
                return;
            }

            LogDebug("ActivateScreen called with charm: " + newCharmObject.name);

            bool wasInLegendaryMode = isLegendaryReplacementMode;
            isLegendaryReplacementMode = false;

            currentNewCharm = newCharmObject;
            isActive = true;
            lastSelectedGameObject = null;

            UpdateNewCharmDisplay(newCharmObject);
            UpdateFromInventory();

            ResetLegendaryModeUI();

            if (wasInLegendaryMode)
            {
                LogDebug("Transitioned from legendary mode to normal mode - UI reset completed");
            }

            if (gameInput != null)
            {
                gameInput.SwitchToUI();
            }

            gameObject.SetActive(true);
            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(false);
            }
            LogDebug("CharmSwapController GameObject activated in normal mode");

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
                LogDebug("Navigation state set to CharmSwapScreen (Normal Mode)");
            }
            else
            {
                LogError("NavigationManager is null!");
            }

            CharmSwapNavigationExtension navExtension = GetComponent<CharmSwapNavigationExtension>();
            if (navExtension != null)
            {
                navExtension.RefreshNavigationForMode();
                LogDebug("Navigation refreshed for normal mode");
            }

            //if (useAnimations && transitionAnimator != null)
            //{
            //    PlayOpeningAnimationSequence();
            //}
            //else
            //{
            ShowCharmSwapUI();
            Invoke("SetInitialSelectionDelayed", 0.1f);
            //}

            LogDebug("CharmSwapScreen normal mode activation started");
        }

        public void DeactivateScreen()
        {
            LogDebug("DeactivateScreen called");

            //if (useAnimations && transitionAnimator != null)
            //{
            //    PlayClosingAnimationSequence();
            //}
            //else
            //{
            PerformActualDeactivation();
            //}
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
            dropPosition += playerTransform.forward * 2f;

            DropManager dropManager = GameManager.Instance.GetComponent<DropManager>();

            if (dropManager == null)
            {
                LogError("DropManager not found on GameManager!");
                return;
            }

            dropManager.DropCharm(charmObject, dropPosition);

            //Destroy(charmObject);

            LogDebug($"Successfully dropped charm '{charmComponent.charmName}' back into world at position {dropPosition}");
        }

        void UpdateSelectionVisuals(int slotIndex)
        {
            LogDebug($"UpdateSelectionVisuals called with slotIndex: {slotIndex} (Legendary mode: {isLegendaryReplacementMode})");

            HideAllSelectors();
            StopAllSelectorAnimations();

            GameObject selectorToShow = null;
            Button selectedButton = null;
            int selectorArrayIndex = -1;

            if (slotIndex == 0)
            {
                selectedButton = legendaryCharmButton;
                selectorArrayIndex = 0;
                LogDebug("Selected legendary slot - will show selector 0");
            }
            else if (slotIndex >= 1 && slotIndex <= 4)
            {
                int arrayIndex = slotIndex - 1;
                if (arrayIndex < charmButtons.Length)
                {
                    selectedButton = charmButtons[arrayIndex];
                    selectorArrayIndex = slotIndex;
                    LogDebug($"Selected regular slot {slotIndex} - will show selector {selectorArrayIndex}");
                }
            }

            if (selectorArrayIndex >= 0 && slotSelectors != null && selectorArrayIndex < slotSelectors.Length)
            {
                selectorToShow = slotSelectors[selectorArrayIndex];
            }

            if (selectorToShow != null && selectedButton != null)
            {
                selectorToShow.SetActive(true);
                LogDebug($"Activated selector: {selectorToShow.name}");

                UpdateSelectorColorForSlot(selectorToShow, selectedButton);

                if (isLegendaryReplacementMode && slotIndex != legendarySlotIndex)
                {
                    LogDebug($"Showing selector for blocked slot {slotIndex} in legendary mode");
                }

                if (selectorArrayIndex >= 0 && selectorArrayIndex < selectorAnimations.Length)
                {
                    selectorAnimations[selectorArrayIndex] = AnimateSelector(selectorToShow);
                    StartCoroutine(selectorAnimations[selectorArrayIndex]);
                    LogDebug($"Started selector animation for index {selectorArrayIndex}");
                }
            }
            else
            {
                LogDebug($"Could not show selector - slotIndex: {slotIndex}, selectorArrayIndex: {selectorArrayIndex}, selectorToShow: {selectorToShow?.name}, selectedButton: {selectedButton?.name}");
            }

            LogDebug($"Updated selection visuals for slot {slotIndex}");
        }

        void UpdateSelectorColorForSlot(GameObject selector, Button button)
        {
            if (selector == null || button == null)
            {
                return;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                return;
            }

            CharmButton charmButton = button.GetComponent<CharmButton>();
            Color newColor = defaultSelectorColor;

            if (charmButton != null && charmButton.charmObject != null)
            {
                BaseCharm charm = charmButton.charmObject.GetComponent<BaseCharm>();
                if (charm != null)
                {
                    switch (charm.charmRarity)
                    {
                        case RARITY.COMMON:
                            newColor = commonSelectorColor;
                            break;
                        case RARITY.RARE:
                            newColor = rareSelectorColor;
                            break;
                        case RARITY.LEGENDARY:
                            newColor = legendarySelectorColor;
                            break;
                        default:
                            newColor = defaultSelectorColor;
                            break;
                    }
                }
            }

            float currentAlpha = selectorImage.color.a;
            selectorImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);
        }

        void UpdateButtonImageColorForSlot(Button button)
        {
            if (button == null)
            {
                return;
            }

            CharmButton charmButton = button.GetComponent<CharmButton>();
            if (charmButton == null)
            {
                return;
            }

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage == null)
            {
                return;
            }

            Color newColor = Color.white;

            if (charmButton.charmObject != null)
            {
                BaseCharm charm = charmButton.charmObject.GetComponent<BaseCharm>();

                if (charm != null)
                {
                    switch (charm.charmRarity)
                    {
                        case RARITY.COMMON:
                            newColor = commonSelectorColor;
                            break;
                        case RARITY.RARE:
                            newColor = rareSelectorColor;
                            break;
                        case RARITY.LEGENDARY:
                            newColor = legendarySelectorColor;
                            break;
                        default:
                            newColor = Color.white;
                            break;
                    }
                }
            }

            float currentAlpha = buttonImage.color.a;
            buttonImage.color = new Color(newColor.r, newColor.g, newColor.b, currentAlpha);
        }

        void ResetAllButtonColors()
        {
            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    Image buttonImage = charmButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        float currentAlpha = buttonImage.color.a;
                        buttonImage.color = new Color(1f, 1f, 1f, currentAlpha);
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                Image buttonImage = legendaryCharmButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    float currentAlpha = buttonImage.color.a;
                    buttonImage.color = new Color(1f, 1f, 1f, currentAlpha);
                }
            }
        }


        void HideAllSelectors()
        {
            if (slotSelectors == null)
            {
                return;
            }

            foreach (var selector in slotSelectors)
            {
                if (selector != null)
                {
                    selector.SetActive(false);
                }
            }
        }

        void StopAllSelectorAnimations()
        {
            if (selectorAnimations == null)
            {
                return;
            }

            for (int i = 0; i < selectorAnimations.Length; i++)
            {
                if (selectorAnimations[i] != null)
                {
                    StopCoroutine(selectorAnimations[i]);
                    selectorAnimations[i] = null;
                }
            }
        }

        int GetSelectorAnimationIndex(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex <= 4)
            {
                return slotIndex;
            }
            return -1;
        }

        IEnumerator AnimateSelector(GameObject selector)
        {
            if (selector == null) yield break;

            Image selectorImage = selector.GetComponent<Image>();
            if (selectorImage == null) yield break;

            Color baseColor = selectorImage.color;
            float time = 0f;

            while (selector != null && selector.activeInHierarchy && selectorImage != null)
            {
                float alpha = Mathf.Lerp(selectorMinAlpha, selectorMaxAlpha, (Mathf.Sin(time * selectorPulseSpeed) + 1f) * 0.5f);

                Color newColor = baseColor;
                newColor.a = alpha;
                selectorImage.color = newColor;

                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public void SelectSlot(int slotIndex)
        {
            LogDebug($"SelectSlot called with index: {slotIndex}, legendary mode: {isLegendaryReplacementMode}");

            if (isLegendaryReplacementMode && slotIndex != legendarySlotIndex)
            {
                LogDebug($"In legendary mode - allowing navigation to slot {slotIndex} but will restrict interaction");
            }

            if (slotIndex == currentSelectedIndex)
            {
                return;
            }

            LogDebug($"Selecting charm swap slot {slotIndex} (Legendary mode: {isLegendaryReplacementMode})");

            currentSelectedIndex = slotIndex;
            UpdateSelectionVisuals(slotIndex);

            Button selectedButton = null;

            if (slotIndex == 0)
            {
                selectedButton = legendaryCharmButton;
                LogDebug("Selected legendary button for display update");
            }
            else if (slotIndex >= 1 && slotIndex <= 4)
            {
                int arrayIndex = slotIndex - 1;
                if (arrayIndex < charmButtons.Length)
                {
                    selectedButton = charmButtons[arrayIndex];
                    LogDebug($"Selected regular button {arrayIndex} for display update");
                }
            }

            if (selectedButton != null)
            {
                UpdateSelectedCharmDisplayFromButton(selectedButton);
            }
        }

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();
            yield return null;

            SetInitialSelection();
        }

        void UpdateNewCharmDisplay(GameObject newCharmObject)
        {
            BaseCharm newCharmInfo = newCharmObject.GetComponent<BaseCharm>();
            if (newCharmInfo == null)
            {
                LogError("New charm object does not have BaseCharm component!");
                return;
            }

            if (newCharmImage != null)
            {
                newCharmImage.sprite = newCharmInfo.charmPicture;
            }

            if (newCharmNameText != null)
            {
                string rarityText = GetRarityDisplayName(newCharmInfo.charmRarity);
                string fullTitle = $"{newCharmInfo.charmName} ({rarityText})";

                newCharmNameText.text = fullTitle;
                newCharmNameText.color = GetRarityTextColor(newCharmInfo.charmRarity);

                LogDebug($"Set new charm title to: {fullTitle}");
            }

            if (newCharmDescriptionText != null)
            {
                newCharmDescriptionText.text = newCharmInfo.charmDescription;
            }

            LogDebug($"Updated new charm display: {newCharmInfo.charmName} ({GetRarityDisplayName(newCharmInfo.charmRarity)})");
        }

        void SetInitialSelection()
        {
            LogDebug($"SetInitialSelection called - isLegendaryReplacementMode: {isLegendaryReplacementMode}");

            if (isLegendaryReplacementMode)
            {
                if (legendaryCharmButton != null && legendaryCharmButton.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(legendaryCharmButton.gameObject);
                    SelectSlot(0);

                    if (navigationManager != null)
                    {
                        navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, legendaryCharmButton.gameObject);
                    }

                    LogDebug("Set initial selection to legendary charm button (legendary mode)");
                    return;
                }
                else
                {
                    LogError("Legendary mode but legendary button not available!");
                }
            }

            if (legendaryCharmButton != null && legendaryCharmButton.interactable)
            {
                EventSystem.current.SetSelectedGameObject(legendaryCharmButton.gameObject);
                SelectSlot(0);

                if (navigationManager != null)
                {
                    navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, legendaryCharmButton.gameObject);
                }

                LogDebug("Set initial selection to legendary charm button (normal mode)");
                return;
            }

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null && charmButtons[i].interactable)
                {
                    EventSystem.current.SetSelectedGameObject(charmButtons[i].gameObject);
                    SelectSlot(i + 1);

                    if (navigationManager != null)
                    {
                        navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, charmButtons[i].gameObject);
                    }

                    LogDebug($"Set initial selection to charm button {i} (normal mode fallback)");
                    return;
                }
            }

            LogError("No buttons available for selection!");
        }

        void AddCharm(GameObject charmToRemove)
        {
            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();

            if (inventory == null)
            {
                LogError("PlayerInventory not found!");
                return;
            }

            LogDebug($"Successfully swapping charm - replacing {charmToRemove.name} with {currentNewCharm.name}");

            inventory.ReplaceCharm(charmToRemove, currentNewCharm);

            cameFromPickUpScreen = false;
            currentNewCharm = null;

            DeactivateScreen();
        }

        void CompleteCharmSwap()
        {
            LogDebug("Completing charm swap after closing animation");

            isPlayingAnimation = false;

            if (tempCharmToReplace != null)
            {
                AddCharm(tempCharmToReplace);
                tempCharmToReplace = null;
            }
            else
            {
                LogError("tempCharmToReplace is null! Cannot complete swap.");
                PerformActualDeactivation();
            }
        }

        public void ActivateScreenLegendaryMode(GameObject newLegendaryCharm)
        {
            if (newLegendaryCharm == null)
            {
                LogError("Cannot activate charm swap screen with null legendary charm!");
                return;
            }

            BaseCharm charmComponent = newLegendaryCharm.GetComponent<BaseCharm>();

            if (charmComponent == null || charmComponent.charmRarity != RARITY.LEGENDARY)
            {
                LogError("Charm is not legendary! Cannot activate legendary replacement mode.");
                return;
            }

            LogDebug("ActivateScreenLegendaryMode called with legendary charm: " + newLegendaryCharm.name);

            isLegendaryReplacementMode = true;

            currentNewCharm = newLegendaryCharm;
            isActive = true;
            lastSelectedGameObject = null;

            UpdateNewCharmDisplay(newLegendaryCharm);
            UpdateFromInventory();

            if (gameInput != null)
            {
                gameInput.SwitchToUI();
            }

            gameObject.SetActive(true);
            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(false);
            }
            LogDebug("CharmSwapController GameObject activated in legendary mode");

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.CharmSwapScreen);
                LogDebug("Navigation state set to CharmSwapScreen (Legendary Mode)");
            }

            SetupLegendaryModeUI();

            CharmSwapNavigationExtension navExtension = GetComponent<CharmSwapNavigationExtension>();
            if (navExtension != null)
            {
                navExtension.RefreshNavigationForMode();
            }

            //if (useAnimations && transitionAnimator != null)
            //{
            //    PlayOpeningAnimationSequence();
            //}
            //else
            //{
            ShowCharmSwapUI();
            Invoke("SetInitialSelectionDelayed", 0.1f);
            //}

            LogDebug("CharmSwapScreen legendary mode activation started");
        }


        void SetupLegendaryModeUI()
        {
            LogDebug("Setting up legendary mode UI...");

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    charmButtons[i].interactable = true;
                    LogDebug($"Set regular charm button {i} interactable for navigation (clicks blocked in legendary mode)");
                }
            }

            if (slotBlockerImages != null)
            {
                for (int i = 0; i < slotBlockerImages.Length; i++)
                {
                    if (slotBlockerImages[i] != null)
                    {
                        slotBlockerImages[i].SetActive(true);
                        LogDebug($"Activated slot blocker {i}");
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.interactable = true;

                CharmButton legendaryCharmButtonComponent = legendaryCharmButton.GetComponent<CharmButton>();
                if (legendaryCharmButtonComponent != null && legendaryCharmButtonComponent.charmObject != null)
                {
                    LogDebug($"Legendary button has charm: {legendaryCharmButtonComponent.charmObject.GetComponent<BaseCharm>().charmName}");
                }
                else
                {
                    LogError("Legendary button has no charm object! This will cause issues.");
                }

                LogDebug("Legendary button set to interactable");
            }

            StartCoroutine(DelayedLegendarySelection());
        }

        IEnumerator DelayedLegendarySelection()
        {
            yield return new WaitForEndOfFrame();
            yield return null;
            yield return null;

            if (legendaryCharmButton != null)
            {
                LogDebug("Starting legendary selection process...");

                EventSystem.current.SetSelectedGameObject(null);
                yield return null;

                EventSystem.current.SetSelectedGameObject(legendaryCharmButton.gameObject);
                LogDebug($"EventSystem selection set to: {EventSystem.current.currentSelectedGameObject?.name}");

                SelectSlot(legendarySlotIndex);
                LogDebug("Called SelectSlot for legendary slot");

                if (navigationManager != null)
                {
                    navigationManager.RegisterFirstSelectable(UINavigationState.CharmSwapScreen, legendaryCharmButton.gameObject);
                }

                LogDebug("Legendary selection process completed with visual feedback");
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
            LogDebug("Opening animation completed - showing UI");

            isPlayingAnimation = false;

            ShowCharmSwapUI();

            Invoke("SetInitialSelectionDelayed", 0.1f);

            LogDebug("Opening animation sequence completed");
        }

        void ResetLegendaryModeUI()
        {
            LogDebug("Resetting legendary mode UI for normal mode...");

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    charmButtons[i].interactable = true;
                    LogDebug($"Reset regular charm button {i} to interactable for normal mode");
                }
            }

            if (slotBlockerImages != null)
            {
                for (int i = 0; i < slotBlockerImages.Length; i++)
                {
                    if (slotBlockerImages[i] != null)
                    {
                        slotBlockerImages[i].SetActive(false);
                        LogDebug($"Deactivated slot blocker {i}");
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.interactable = true;
                LogDebug("Reset legendary button to interactable for normal mode");
            }

            LogDebug("Reset completed - all buttons enabled and functional for normal mode");
        }

        void PlayClosingAnimationSequence()
        {
            LogDebug("Starting closing animation sequence...");

            isPlayingAnimation = true;

            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(false);
                LogDebug("CharmSwapCanvas hidden for closing animation");
            }

            //if (transitionAnimator != null)
            //{
            //    if (!transitionAnimator.gameObject.activeInHierarchy)
            //    {
            //        transitionAnimator.gameObject.SetActive(true);
            //    }

            //    transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            //    transitionAnimator.Play(closeAnimationName);
            //    LogDebug($"Playing closing animation: {closeAnimationName}");

            //    Invoke("OnClosingAnimationComplete", animationDuration);
            //}
            //else
            //{
            OnClosingAnimationComplete();
            //}
        }

        void OnClosingAnimationComplete()
        {
            LogDebug("Closing animation completed - performing deactivation");

            isPlayingAnimation = false;

            PerformActualDeactivation();

            LogDebug("Closing animation sequence completed");
        }

        void SetInitialSelectionDelayed()
        {
            LogDebug("Setting initial selection after animation");

            if (isLegendaryReplacementMode)
            {
                StartCoroutine(DelayedLegendarySelection());
            }
            else
            {
                StartCoroutine(DelayedInitialSelection());
            }
        }

        void PerformActualDeactivation()
        {
            LogDebug("Performing actual deactivation");

            HideAllSelectors();
            StopAllSelectorAnimations();
            currentSelectedIndex = -1;

            if (isLegendaryReplacementMode)
            {
                ResetLegendaryModeUI();
                isLegendaryReplacementMode = false;
                LogDebug("Reset legendary mode UI");
            }

            isActive = false;
            wasActiveBeforePause = false;

            if (currentNewCharm != null)
            {
                if (cameFromPickUpScreen)
                {
                    LogDebug($"Player cancelled charm swap that came from pickup screen - dropping charm and closing both screens");

                    DropCharmIntoWorld(currentNewCharm);

                    if (pickUpScreenController != null)
                    {
                        pickUpScreenController.ForceHidePickUpScreen();
                    }
                }
                else
                {
                    LogDebug($"Player cancelled without swapping - dropping charm: {currentNewCharm.name}");
                    DropCharmIntoWorld(currentNewCharm);
                }
            }

            currentNewCharm = null;
            cameFromPickUpScreen = false;
            lastSelectedGameObject = null;

            gameObject.SetActive(false);
            LogDebug("CharmSwapController GameObject deactivated");

            if (navigationManager != null)
            {
                navigationManager.SetNavigationState(UINavigationState.None);
                LogDebug("Navigation state set to None");
            }

            GameManager.Instance.ResumeGame();

            PlayerInventory inventory = GameManager.Instance.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.inShop)
            {
                inventory.currentShopKeeper.OpenShopScreen();
            }

            LogDebug("CharmSwapScreen deactivation completed");
        }

        void ShowCharmSwapUI()
        {
            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(true);
                LogDebug("CharmSwapCanvas activated after animation");
            }
        }

        public void Show()
        {
            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(true);
            }

            isActive = true;
            LogDebug("CharmSwapScreen shown");
        }

        public void Hide()
        {
            if (charmSwapCanvas != null)
            {
                charmSwapCanvas.SetActive(false);
            }

            isActive = false;
            LogDebug("CharmSwapScreen hidden");
        }

        public void RestoreAfterPause()
        {
            wasActiveBeforePause = false;
            Show();
            LogDebug("CharmSwapScreen restored after pause");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF6600>[CharmSwapMenuController] {message}</color>");
            }
        }

        void LogError(string message)
        {
            Debug.LogError($"[CharmSwapMenuController] {message}");
        }
    }
}