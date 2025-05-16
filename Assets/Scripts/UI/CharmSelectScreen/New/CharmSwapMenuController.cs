using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using ProjectColombo.Inventory;
using System.Collections.Generic;
using ProjectColombo.GameManagement;
using ProjectColombo.Objects.Charms;


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
        bool isExistingCharm = false;
        bool isCancellingMenu = false;
        bool wasActiveBeforePause = false;

        public bool WasActiveBeforePause => wasActiveBeforePause;


        Vector3 dropPosition;


        GameObject newCharm;
        GameObject lastSelectedButton;

        Coroutine selectorAnimationCoroutine;

        Dictionary<int, Button> indexToButtonMap = new Dictionary<int, Button>();

        List<int> availableButtonIndices = new List<int>();

        PlayerInventory playerInventory;
        DropManager dropManager;

        public override void Initialize()
        {
            base.Initialize();

            LogDebug("Initializing CharmSwapMenuController");

            if (GameManager.Instance != null)
            {
                playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                dropManager = GameManager.Instance.GetComponent<DropManager>();

                if (dropManager == null)
                {
                    LogDebug("WARNING: DropManager not found. Charm dropping may not work correctly!");
                }
            }
            else
            {
                LogDebug("WARNING: GameManager.Instance is null!");
            }

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

            indexToButtonMap.Clear();

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
            /*if (UIManager.Instance != null)
            {
                LogDebug("Registering with UIManager");
                UIManager.Instance.RegisterMenu(this);
                Hide();
            }*/
        }

        void Update()
        {
            if (isCancellingMenu || !isActive)
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

                if (buttonToSelect == null && availableButtonIndices.Count > 0)
                {
                    buttonToSelect = charmButtons[availableButtonIndices[0]].gameObject;
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

#if UNITY_EDITOR
            if (Gamepad.current != null)
            {
                Vector2 dpadValue = Gamepad.current.dpad.ReadValue();
                if (dpadValue.sqrMagnitude > 0.5f)
                {
                    Debug.Log($"<color=#FFAA00>[CharmSwap D-Pad] {dpadValue}</color>");
                }
            }
#endif
        }

        void EnsureButtonSelection()
        {
            if (!isActive || availableButtonIndices.Count == 0)
            {
                return;
            }

            GameObject buttonToSelect = null;

            if (lastSelectedButton != null)
            {
                buttonToSelect = lastSelectedButton;
            }
            else if (currentCharmIndex >= 0 && currentCharmIndex < charmButtons.Length &&
                    charmButtons[currentCharmIndex] != null && charmButtons[currentCharmIndex].isActiveAndEnabled)
            {
                buttonToSelect = charmButtons[currentCharmIndex].gameObject;
            }
            else if (availableButtonIndices.Count > 0)
            {
                buttonToSelect = charmButtons[availableButtonIndices[0]].gameObject;
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

            SetupButtonNavigation();


            if (availableButtonIndices.Count > 0)
            {
                currentCharmIndex = availableButtonIndices[0];
                SelectCharm(currentCharmIndex);
            }
            else
            {
                currentCharmIndex = 0;
                SelectCharm(currentCharmIndex);
            }

            StartCoroutine(SetInitialButtonSelection());

            LogDebug("CharmSwapMenuController shown - PauseCharmSwap enabled");
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

            if (gameInputSO.CharmSwapPausePressed)
            {
                LogDebug("CharmSwapPausePressed detected!");

                gameInputSO.ResetCharmSwapPausePressed();

                wasActiveBeforePause = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PauseGame(true);
                }

                return;
            }

            bool canNavigate = Time.unscaledTime - lastNavigationTime >= navigationDelay;

            if (!canNavigate)
            {
                return;
            }

            if (gameInputSO.playerInputActions.UI.Submit.WasPressedThisFrame())
            {
                ConfirmSwap();
            }
        }

        void SetupButtonNavigation()
        {
            availableButtonIndices.Clear();
            int availableButtonCount = Mathf.Min(charmButtons.Length, playerInventory != null ? playerInventory.charms.Count + playerInventory.legendaryCharms.Count : 0);

            for (int i = 0; i < availableButtonCount; i++)
            {
                if (charmButtons[i] != null && charmButtons[i].isActiveAndEnabled && charmButtons[i].interactable)
                {
                    availableButtonIndices.Add(i);
                }
            }

            LogDebug($"Found {availableButtonIndices.Count} available buttons");

            for (int i = 0; i < availableButtonIndices.Count; i++)
            {
                int buttonIndex = availableButtonIndices[i];
                Button button = charmButtons[buttonIndex];

                if (button != null)
                {
                    Navigation navigation = button.navigation;
                    navigation.mode = Navigation.Mode.Explicit;

                    if (i > 0)
                    {
                        navigation.selectOnLeft = charmButtons[availableButtonIndices[i - 1]];
                    }
                    else if (availableButtonIndices.Count > 1)
                    {
                        navigation.selectOnLeft = charmButtons[availableButtonIndices[availableButtonIndices.Count - 1]];
                    }

                    if (i < availableButtonIndices.Count - 1)
                    {
                        navigation.selectOnRight = charmButtons[availableButtonIndices[i + 1]];
                    }
                    else if (availableButtonIndices.Count > 1)
                    {
                        navigation.selectOnRight = charmButtons[availableButtonIndices[0]];
                    }

                    button.navigation = navigation;

                    LogDebug($"Setup navigation for button {buttonIndex}: Left={navigation.selectOnLeft?.name}, Right={navigation.selectOnRight?.name}");
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
            isExistingCharm = charm.scene.IsValid();

            if (isExistingCharm)
            {
                LogDebug("Received an existing scene charm: " + charm.name);
            }
            else
            {
                LogDebug("Received a prefab charm: " + charm.name);
            }

            BaseCharm charmInfo = newCharm.GetComponent<BaseCharm>();

            if (charmInfo != null)
            {
                newCharmTitleText.text = charmInfo.charmName;
                newCharmDescriptionText.text = charmInfo.charmDescription;
                newCharmImage.sprite = charmInfo.charmPicture;
            }

            UpdateCharmButtons();

            /*if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMenu(this);
            }
            else
            {
                Show();
            }*/

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame(false);
            }

            LogDebug("CharmSwapMenuController activated with charm: " + charmInfo.charmName);
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
                        charmButtons[i].interactable = true;
                    }
                }
            }

            if (charmButtons.Length > playerInventory.charms.Count && playerInventory.legendaryCharms.Count > 0)
            {
                int legendaryIndex = playerInventory.charms.Count;
                if (legendaryIndex < charmButtons.Length)
                {
                    CharmButton legendaryButton = charmButtons[legendaryIndex].GetComponent<CharmButton>();
                    if (legendaryButton != null)
                    {
                        legendaryButton.UpdateInfo(playerInventory.legendaryCharms[0]);
                        charmButtons[legendaryIndex].interactable = true;
                    }
                }
            }

            for (int i = playerInventory.charms.Count + playerInventory.legendaryCharms.Count; i < charmButtons.Length; i++)
            {
                if (charmButtons[i] != null)
                {
                    charmButtons[i].interactable = false;
                    CharmButton charmButton = charmButtons[i].GetComponent<CharmButton>();
                    if (charmButton != null)
                    {
                        charmButton.UpdateInfo(null);
                    }
                }
            }

            SetupButtonNavigation();
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


        public void RestoreAfterPause()
        {
            if (wasActiveBeforePause && newCharm != null)
            {
                /*if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMenu(this);
                }*/

                LogDebug("Restoring CharmSwapMenu after pause");

                gameObject.SetActive(true);
                isActive = true;

                StartCoroutine(SetInitialButtonSelection());

                wasActiveBeforePause = false;
            }
        }

        public void CancelSwap()
        {
            LogDebug("CancelSwap called - closing menu without swap");
            isCancellingMenu = true;
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
        }

        IEnumerator SetInitialButtonSelection()
        {
            yield return new WaitForSecondsRealtime(0.05f);

            if (availableButtonIndices.Count > 0)
            {
                int firstIndex = availableButtonIndices[0];
                if (charmButtons[firstIndex] != null)
                {
                    EventSystem.current.SetSelectedGameObject(charmButtons[firstIndex].gameObject);
                    SelectCharm(firstIndex);

                    if (uiInputSwitcher != null)
                    {
                        uiInputSwitcher.SetFirstSelectedButton(charmButtons[firstIndex].gameObject);
                    }
                }
            }
        }

        IEnumerator DelayedConfirm(GameObject charmToSwap)
        {
            yield return new WaitForSecondsRealtime(confirmActionDelay);

            if (playerInventory != null && newCharm != null)
            {
                playerInventory.ReplaceCharm(charmToSwap, newCharm);
                newCharm = null;
            }

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

            if (newCharm != null)
            {
                if (isExistingCharm)
                {
                    LogDebug("Preserving existing scene charm on cancel: " + newCharm.name);
                }
                else
                {
                    LogDebug("Creating a world pickup for prefab charm on cancel: " + newCharm.name);

                    Vector3 dropPosition = Vector3.zero;
                    GameObject player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        dropPosition = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
                    }

                    if (dropManager != null)
                    {
                        BaseCharm charmComponent = newCharm.GetComponent<BaseCharm>();
                        if (charmComponent != null)
                        {
                            LogDebug("Dropping charm via DropManager: " + charmComponent.charmName);
                            dropManager.DropCharm(charmComponent, dropPosition);
                        }
                    }
                    else
                    {
                        dropManager = FindFirstObjectByType<DropManager>();

                        if (dropManager != null)
                        {
                            BaseCharm charmComponent = newCharm.GetComponent<BaseCharm>();
                            if (charmComponent != null)
                            {
                                LogDebug("Dropping charm via found DropManager: " + charmComponent.charmName);
                                dropManager.DropCharm(charmComponent, dropPosition);
                            }
                        }
                        else
                        {
                            LogDebug("ERROR: Could not find DropManager! Charm will be lost.");
                        }
                    }
                }
            }

            newCharm = null;

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