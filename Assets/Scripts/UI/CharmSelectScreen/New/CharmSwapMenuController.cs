using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ProjectColombo.UI
{
    public class CharmSwapMenuController : MenuController
    {
        [Header("New Charm Display")]
        [SerializeField] TMP_Text newCharmTitleText;
        [SerializeField] TMP_Text newCharmDescriptionText;
        [SerializeField] Image newCharmImage;

        [Header("Current Charm Display")]
        [SerializeField] TMP_Text selectedCharmTitleText;
        [SerializeField] TMP_Text selectedCharmDescriptionText;

        [Header("Pentagram Navigation")]
        [SerializeField] Button[] charmButtons;
        [SerializeField] GameObject[] charmSelectors;
        [SerializeField] GameObject[] charmClefs;

        [Header("Button Animation")]
        [SerializeField] float confirmActionDelay = 0.2f;
        [SerializeField] float cancelActionDelay = 0.2f;

        [Header("Animation")]
        [SerializeField] float selectorFlickerMinAlpha = 0.5f;
        [SerializeField] float selectorFlickerMaxAlpha = 1.0f;
        [SerializeField] float selectorFlickerSpeed = 2.0f;
        [SerializeField] GameObject potionsCharmClef;

        [Header("Debug")]
        [SerializeField] bool enableDebugLogs = true;


        int currentCharmIndex = 0;

        bool isActive = false;
        bool isNavigating = false;
        bool isCancellingMenu = false;
        bool debugKeyExitEnabled = true;


        GameObject newCharm;
        GameObject lastSelectedButton;

        Coroutine selectorAnimationCoroutine;

        Dictionary<int, Button> indexToButtonMap = new Dictionary<int, Button>();

        PlayerInventory playerInventory;



        public override void Initialize()
        {
            base.Initialize();

            LogDebug("Initializing CharmSwapMenuController");

            playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

            uiInputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

            if (uiInputSwitcher == null)
            {
                GameObject uiInputSwitcherObject = new GameObject("UIInputSwitcher");
                uiInputSwitcher = uiInputSwitcherObject.AddComponent<UIInputSwitcher>();
                LogDebug("Created new UIInputSwitcher");
            }
            else
            {
                LogDebug("Found existing UIInputSwitcher");
            }

            foreach (GameObject selector in charmSelectors)
            {
                if (selector != null)
                {
                    selector.SetActive(false);
                }
            }

            foreach (GameObject clef in charmClefs)
            {
                if (clef != null)
                {
                    clef.SetActive(false);
                }
            }

            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    int buttonIndex = i;
                    indexToButtonMap[i] = charmButtons[i];

                    charmButtons[i].onClick.AddListener(() =>
                    {
                        SelectCharm(buttonIndex);
                        ConfirmSwap();
                    });

                    EventTrigger eventTrigger = charmButtons[i].GetComponent<EventTrigger>();

                    if (eventTrigger == null)
                    {
                        eventTrigger = charmButtons[i].gameObject.AddComponent<EventTrigger>();
                    }

                    AddEventTriggerEntry(eventTrigger, EventTriggerType.Select, (data) =>
                    {
                        SelectCharm(buttonIndex);
                    });

                    AddEventTriggerEntry(eventTrigger, EventTriggerType.PointerEnter, (data) =>
                    {
                        SelectCharm(buttonIndex);
                    });

                    AddEventTriggerEntry(eventTrigger, EventTriggerType.Submit, (data) =>
                    {
                        SelectCharm(buttonIndex);
                        ConfirmSwap();
                    });
                }
            }

            gameObject.SetActive(false);
            isActive = false;

            LogDebug("CharmSwapMenuController initialized");
        }

        void OnEnable()
        {
            EnsureButtonSelection();

            isCancellingMenu = false;
        }

        public void RegisterWithUIManager()
        {
            if (UIManager.Instance != null)
            {
                LogDebug("Registering with UIManager");

                UIManager.Instance.RegisterMenu(this);

                Hide();
            }
        }

        void Update()
        {
            if (isCancellingMenu)
            {
                return;
            }

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != null && currentSelected.GetComponent<Button>() != null)
            {
                lastSelectedButton = currentSelected;

                Button selectedButton = currentSelected.GetComponent<Button>();

                for (int i = 0; i < charmButtons.Length; i++)
                {
                    if (selectedButton == charmButtons[i] && currentCharmIndex != i)
                    {
                        SelectCharm(i);
                        break;
                    }
                }
            }

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                GameObject buttonToSelect = lastSelectedButton;

                if (buttonToSelect == null && charmButtons.Length > 0)
                {
                    buttonToSelect = charmButtons[0].gameObject;
                }

                if (buttonToSelect != null)
                {
                    EventSystem.current.SetSelectedGameObject(buttonToSelect);

                    if (uiInputSwitcher != null)
                    {
                        uiInputSwitcher.SetFirstSelectedButton(buttonToSelect);
                        LogDebug("Restored selection to: " + buttonToSelect.name);
                    }
                }
            }

#if UNITY_EDITOR
            if (Gamepad.current != null)
            {
                Vector2 navegationVector = Gamepad.current.dpad.ReadValue();

                if (navegationVector != Vector2.zero)
                {
                    Debug.Log($"<color=#FFAA00>[CharmSwap D-Pad] {navegationVector}</color>");
                }
            }
#endif
        }

        void EnsureButtonSelection()
        {
            if (!isActive || charmButtons == null || charmButtons.Length == 0)
            {
                return;
            }

            GameObject buttonToSelect = null;

            if (lastSelectedButton != null)
            {
                buttonToSelect = lastSelectedButton;
            }
            else if (currentCharmIndex >= 0 && currentCharmIndex < charmButtons.Length)
            {
                buttonToSelect = charmButtons[currentCharmIndex].gameObject;
            }
            else if (charmButtons.Length > 0)
            {
                buttonToSelect = charmButtons[0].gameObject;
            }

            if (buttonToSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(buttonToSelect);

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(buttonToSelect);
                }
            }
        }

        void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((data) => { action((BaseEventData)data); });
            trigger.triggers.Add(entry);
        }

        public override void Show()
        {
            base.Show();

            isCancellingMenu = false;

            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
            isActive = true;

            if (gameInputSO != null)
            {
                gameInputSO.EnableUIMode();
            }

            SetupButtonNavigation();

            currentCharmIndex = 0;
            SelectCharm(currentCharmIndex);

            StartCoroutine(SetInitialButtonSelection());

            if (uiInputSwitcher != null && charmButtons.Length > 0 && charmButtons[0] != null)
            {
                uiInputSwitcher.SetFirstSelectedButton(charmButtons[0].gameObject);
                LogDebug("Registered first button with UIInputSwitcher");
            }

            LogDebug("CharmSwapMenuController shown");
        }

        public override void Hide()
        {
            base.Hide();

            if (selectorAnimationCoroutine != null)
            {
                StopCoroutine(selectorAnimationCoroutine);
                selectorAnimationCoroutine = null;
            }

            foreach (GameObject selector in charmSelectors)
            {
                if (selector != null)
                {
                    selector.SetActive(false);
                }
            }

            foreach (GameObject clef in charmClefs)
            {
                if (clef != null)
                {
                    clef.SetActive(false);
                }
            }

            isActive = false;
            gameObject.SetActive(false);

            LogDebug("CharmSwapMenuController hidden");
        }

        public override void HandleInput()
        {
            base.HandleInput();

            if (!isActive || isCancellingMenu)
            {
                return;
            }

            if (gameInputSO == null || gameInputSO.playerInputActions == null)
            {
                LogDebug("HandleInput: Skipping due to missing input references");
                return;
            }

            // NOT WORKING!
            if (gameInputSO.playerInputActions.UI.Cancel.WasPressedThisFrame())
            {
                LogDebug("Cancel input detected, closing menu");
                CancelSwap();
                return;
            }

            if (gameInputSO.playerInputActions.UI.Cancel.IsPressed())
            {
                LogDebug("Cancel is being pressed but not detected as WasPressedThisFrame");
            }
            //-------------------

            Vector2 navigationValue = gameInputSO.playerInputActions.UI.Navigate.ReadValue<Vector2>();

            if (navigationValue.magnitude > 0.5f)
            {
                bool isHorizontalNav = Mathf.Abs(navigationValue.x) > Mathf.Abs(navigationValue.y);

                if (isHorizontalNav && !isNavigating)
                {
                    isNavigating = true;

                    if (navigationValue.x > 0.5f)
                    {
                        NavigateRight();
                        StartCoroutine(ResetNavigationFlag(0.2f));
                    }
                    else if (navigationValue.x < -0.5f)
                    {
                        NavigateLeft();
                        StartCoroutine(ResetNavigationFlag(0.2f));
                    }
                }
            }
            else
            {
                isNavigating = false;
            }

            if (gameInputSO.playerInputActions.UI.MoveRightShoulder.WasPressedThisFrame())
            {
                NavigateRight();
            }
            else if (gameInputSO.playerInputActions.UI.MoveLeftShoulder.WasPressedThisFrame())
            {
                NavigateLeft();
            }

            if (gameInputSO.playerInputActions.UI.Submit.WasPressedThisFrame())
            {
                ConfirmSwap();
            }
        }

        void SetupButtonNavigation()
        {
            for (int i = 0; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    Navigation navigation = charmButtons[i].navigation;
                    navigation.mode = Navigation.Mode.Explicit;

                    if (i > 0)
                    {
                        navigation.selectOnLeft = charmButtons[i - 1];
                    }
                    else
                    {
                        navigation.selectOnLeft = charmButtons[charmButtons.Length - 1];
                    }

                    if (i < charmButtons.Length - 1)
                    {
                        navigation.selectOnRight = charmButtons[i + 1];
                    }
                    else
                    {
                        navigation.selectOnRight = charmButtons[0];
                    }

                    charmButtons[i].navigation = navigation;
                }
            }
        }

        public void ActivateScreen(GameObject charm)
        {
            if (charm == null)
            {
                LogDebug("Cannot activate with null charm");
                return;
            }

            newCharm = charm;

            BaseCharm charmInfo = newCharm.GetComponent<BaseCharm>();

            if (charmInfo != null)
            {
                newCharmTitleText.text = charmInfo.charmName;
                newCharmDescriptionText.text = charmInfo.charmDescription;
                newCharmImage.sprite = charmInfo.charmPicture;
            }

            UpdateCharmButtons();

            Show();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame(false);
            }

            LogDebug("CharmSwapMenuController activated with new charm: " + charmInfo.charmName);
        }

        void UpdateCharmButtons()
        {
            if (playerInventory == null)
            {
                LogDebug("PlayerInventory is null");
                return;
            }

            for (int i = 0; i < Mathf.Min(charmButtons.Length, playerInventory.charms.Count); i++)
            {
                if (charmButtons[i] != null)
                {
                    CharmButton charmButton = charmButtons[i].GetComponent<CharmButton>();

                    if (charmButton != null)
                    {
                        charmButton.UpdateInfo(playerInventory.charms[i]);
                    }
                }
            }

            if (charmButtons.Length > playerInventory.charms.Count && playerInventory.legendaryCharms.Count > 0)
            {
                CharmButton legendaryButton = charmButtons[playerInventory.charms.Count].GetComponent<CharmButton>();

                if (legendaryButton != null)
                {
                    legendaryButton.UpdateInfo(playerInventory.legendaryCharms[0]);
                }
            }
        }

        void SelectCharm(int index)
        {
            if (index < 0 || index >= charmButtons.Length)
            {
                LogDebug("Invalid charm index: " + index);
                return;
            }

            if (currentCharmIndex >= 0 && currentCharmIndex < charmSelectors.Length)
            {
                charmSelectors[currentCharmIndex].SetActive(false);
            }

            currentCharmIndex = index;

            if (currentCharmIndex < charmSelectors.Length && charmSelectors[currentCharmIndex] != null)
            {
                charmSelectors[currentCharmIndex].SetActive(true);

                if (selectorAnimationCoroutine != null)
                {
                    StopCoroutine(selectorAnimationCoroutine);
                }

                selectorAnimationCoroutine = StartCoroutine(AnimateSelectorFlicker(charmSelectors[currentCharmIndex]));
            }

            CharmButton selectedButton = charmButtons[currentCharmIndex].GetComponent<CharmButton>();

            if (selectedButton != null && selectedButton.charmObject != null)
            {
                BaseCharm charmInfo = selectedButton.charmObject.GetComponent<BaseCharm>();

                if (charmInfo != null)
                {
                    selectedCharmTitleText.text = charmInfo.charmName;
                    selectedCharmDescriptionText.text = charmInfo.charmDescription;
                }
            }

            foreach (GameObject clef in charmClefs)
            {
                if (clef != null)
                {
                    clef.SetActive(false);
                }
            }

            if (potionsCharmClef != null)
            {
                potionsCharmClef.SetActive(true);
            }

            LogDebug("Selected charm " + currentCharmIndex);
        }

        void NavigateLeft()
        {
            int newIndex = currentCharmIndex - 1;

            if (newIndex < 0)
            {
                newIndex = Mathf.Min(charmButtons.Length, playerInventory.charms.Count + playerInventory.legendaryCharms.Count) - 1;
            }

            SelectCharm(newIndex);

            if (indexToButtonMap.ContainsKey(newIndex) && indexToButtonMap[newIndex] != null)
            {
                EventSystem.current.SetSelectedGameObject(indexToButtonMap[newIndex].gameObject);
            }
        }

        void NavigateRight()
        {
            int newIndex = (currentCharmIndex + 1) % Mathf.Min(charmButtons.Length, playerInventory.charms.Count + playerInventory.legendaryCharms.Count);

            SelectCharm(newIndex);

            if (indexToButtonMap.ContainsKey(newIndex) && indexToButtonMap[newIndex] != null)
            {
                EventSystem.current.SetSelectedGameObject(indexToButtonMap[newIndex].gameObject);
            }
        }

        void ConfirmSwap()
        {
            CharmButton selectedButton = charmButtons[currentCharmIndex].GetComponent<CharmButton>();

            if (selectedButton == null || selectedButton.charmObject == null)
            {
                LogDebug("No valid charm selected for swap");
                return;
            }

            RectTransform buttonRect = selectedButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                PlayButtonClickAnimation(buttonRect);
            }

            StartCoroutine(DelayedConfirm(selectedButton.charmObject));

            LogDebug("Starting confirm swap with delay");
        }

        void CancelSwap()
        {
            LogDebug("CancelSwap called - closing menu without swap");

            isCancellingMenu = true;

            if (currentCharmIndex >= 0 && currentCharmIndex < charmButtons.Length)
            {
                Button button = charmButtons[currentCharmIndex];
                if (button != null)
                {
                    RectTransform buttonRect = button.GetComponent<RectTransform>();

                    if (buttonRect != null)
                    {
                        PlayButtonClickAnimation(buttonRect);
                    }
                }
            }

            StartCoroutine(DelayedCancel());
        }

        IEnumerator AnimateSelectorFlicker(GameObject selector)
        {
            if (selector == null)
            {
                yield break;
            }

            Image selectorImage = selector.GetComponent<Image>();

            if (selectorImage == null)
            {
                yield break;
            }

            Color originalColor = selectorImage.color;

            float time = 0f;

            while (true)
            {
                float alpha = Mathf.Lerp(selectorFlickerMinAlpha, selectorFlickerMaxAlpha, (Mathf.Sin(time * selectorFlickerSpeed) + 1f) * 0.5f);

                Color newColor = originalColor;
                newColor.a = alpha;
                selectorImage.color = newColor;

                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        IEnumerator ResetNavigationFlag(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            isNavigating = false;
        }

        IEnumerator SetInitialButtonSelection()
        {
            yield return new WaitForSecondsRealtime(0.05f);

            if (charmButtons.Length > 0 && charmButtons[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(charmButtons[0].gameObject);

                if (uiInputSwitcher != null)
                {
                    uiInputSwitcher.SetFirstSelectedButton(charmButtons[0].gameObject);
                }
            }
        }

        IEnumerator DelayedConfirm(GameObject charmToSwap)
        {
            yield return new WaitForSecondsRealtime(confirmActionDelay);

            playerInventory.ReplaceCharm(charmToSwap, newCharm);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            Hide();

            LogDebug("Swap confirmed after delay");
        }

        IEnumerator DelayedCancel()
        {
            LogDebug("Starting DelayedCancel coroutine");

            yield return new WaitForSecondsRealtime(cancelActionDelay);

            if (GameManager.Instance != null)
            {
                LogDebug("Resuming game via GameManager");
                GameManager.Instance.ResumeGame();
            }
            else
            {
                LogDebug("GameManager not found, manually resuming");
                Time.timeScale = 1.0f;
            }

            isCancellingMenu = false;

            LogDebug("Hiding charm swap menu");
            Hide();

            LogDebug("Swap canceled successfully");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[CharmSwapMenu] {message}</color>");
            }
        }
    }
}