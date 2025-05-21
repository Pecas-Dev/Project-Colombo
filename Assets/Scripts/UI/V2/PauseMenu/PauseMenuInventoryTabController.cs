using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryTabController : MonoBehaviour
    {
        [Header("Inventory Slots")]
        [SerializeField] Button[] inventorySlotButtons;
        [SerializeField] GameObject[] slotSelectors;

        [Header("Pentagram Areas")]
        [SerializeField] GameObject weaponPentagram;
        [SerializeField] GameObject maskPentagram;
        [SerializeField] GameObject potionCharmPentagram;

        [Header("Clef Indicators")]
        [SerializeField] GameObject weaponClef;
        [SerializeField] GameObject maskClef;
        [SerializeField] GameObject potionCharmClef;

        [Header("Selector Animation")]
        [SerializeField] float selectorPulseSpeed = 2f;
        [SerializeField] float selectorMinAlpha = 0.5f;
        [SerializeField] float selectorMaxAlpha = 1f;

        [Header("Input Settings")]
        [SerializeField] GameInputSO gameInput;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        int currentSelectedIndex = -1;

        bool isInitialized = false;
        bool isActive = false;
        bool isTabSelected = false;


        IEnumerator[] selectorAnimations;



        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void OnEnable()
        {
            LogDebug("OnEnable called");
            isActive = true;

            ResetSelectionState();

            StartCoroutine(WaitForTabSelection());
        }

        void OnDisable()
        {
            LogDebug("OnDisable called");
            isActive = false;
            isTabSelected = false;

            ResetSelectionState();
        }

        IEnumerator WaitForTabSelection()
        {
            yield return new WaitForEndOfFrame();
            yield return null;

            UINavigationManager navManager = FindFirstObjectByType<UINavigationManager>();
            if (navManager != null)
            {
                isTabSelected = navManager.GetCurrentState() == UINavigationState.PauseInventoryTab;
                LogDebug($"Tab selection status from UINavigationManager: {isTabSelected}");
            }

        }

        void Update()
        {
            if (!isActive || !isTabSelected)
                return;

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                bool isOurButton = false;
                for (int i = 0; i < inventorySlotButtons.Length; i++)
                {
                    if (EventSystem.current.currentSelectedGameObject == inventorySlotButtons[i].gameObject)
                    {
                        if (i != currentSelectedIndex)
                        {
                            SelectSlot(i, false);
                        }
                        isOurButton = true;
                        break;
                    }
                }

                if (!isOurButton && currentSelectedIndex >= 0)
                {
                    ResetSelectionState();
                }
            }
            else if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (currentSelectedIndex >= 0)
                {
                    ResetSelectionState();
                }
            }
        }

        public void ResetSelectionState()
        {
            LogDebug("Resetting selection state");

            HideAllClefs();
            HideAllSelectors();

            if (selectorAnimations != null)
            {
                for (int i = 0; i < selectorAnimations.Length; i++)
                {
                    StopSelectorAnimation(i);
                }
            }

            currentSelectedIndex = -1;
        }

        public void Initialize()
        {
            LogDebug("Initializing inventory tab controller");

            selectorAnimations = new IEnumerator[slotSelectors.Length];

            HideAllClefs();
            HideAllSelectors();

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                if (inventorySlotButtons[i] == null) continue;

                int slotIndex = i;

                inventorySlotButtons[i].onClick.RemoveAllListeners();
                inventorySlotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));

                EventTrigger eventTrigger = inventorySlotButtons[i].GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = inventorySlotButtons[i].gameObject.AddComponent<EventTrigger>();
                }

                eventTrigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Select
                };
                selectEntry.callback.AddListener((data) => SelectSlot(slotIndex));
                eventTrigger.triggers.Add(selectEntry);

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnterEntry.callback.AddListener((data) => SelectSlot(slotIndex));
                eventTrigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerExit
                };
                pointerExitEntry.callback.AddListener((data) => EnsureVisualSelection());
                eventTrigger.triggers.Add(pointerExitEntry);

                SetupExplicitNavigation(i);
            }

            isInitialized = true;
        }

        void SetupExplicitNavigation(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= inventorySlotButtons.Length || inventorySlotButtons[buttonIndex] == null)
                return;

            Button button = inventorySlotButtons[buttonIndex];

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (IsChildOf(button.transform, weaponPentagram))
            {
                Button maskButton = FindFirstButtonInArea(maskPentagram);
                if (maskButton != null)
                {
                    nav.selectOnDown = maskButton;
                }

                Button firstCharmButton = FindFirstButtonInArea(potionCharmPentagram);
                if (firstCharmButton != null)
                {
                    nav.selectOnRight = firstCharmButton;
                }

                nav.selectOnUp = null;
                nav.selectOnLeft = null;
            }
            else if (IsChildOf(button.transform, maskPentagram))
            {
                Button weaponButton = FindFirstButtonInArea(weaponPentagram);
                if (weaponButton != null)
                {
                    nav.selectOnUp = weaponButton;
                }

                Button firstCharmButton = FindFirstButtonInArea(potionCharmPentagram);
                if (firstCharmButton != null)
                {
                    nav.selectOnDown = firstCharmButton;
                    nav.selectOnRight = firstCharmButton;
                }

                nav.selectOnLeft = null;
            }
            else if (IsChildOf(button.transform, potionCharmPentagram))
            {
                Button[] charmButtons = GetButtonsInArea(potionCharmPentagram);
                int buttonPosition = System.Array.IndexOf(charmButtons, button);

                if (buttonPosition != -1)
                {
                    if (buttonPosition > 0)
                    {
                        nav.selectOnLeft = charmButtons[buttonPosition - 1];
                    }
                    else
                    {
                        Button _maskButton = FindFirstButtonInArea(maskPentagram);
                        if (_maskButton != null)
                        {
                            nav.selectOnLeft = _maskButton;
                        }
                    }

                    if (buttonPosition < charmButtons.Length - 1)
                    {
                        nav.selectOnRight = charmButtons[buttonPosition + 1];
                    }

                    Button maskButton = FindFirstButtonInArea(maskPentagram);
                    if (maskButton != null)
                    {
                        nav.selectOnUp = maskButton;
                    }

                    nav.selectOnDown = null;
                }
            }

            button.navigation = nav;
        }

        Button FindFirstButtonInArea(GameObject area)
        {
            if (area == null) return null;

            foreach (Button button in inventorySlotButtons)
            {
                if (button != null && IsChildOf(button.transform, area))
                {
                    return button;
                }
            }

            return null;
        }

        Button[] GetButtonsInArea(GameObject area)
        {
            if (area == null) return new Button[0];

            System.Collections.Generic.List<Button> buttonsInArea = new System.Collections.Generic.List<Button>();

            foreach (Button button in inventorySlotButtons)
            {
                if (button != null && IsChildOf(button.transform, area))
                {
                    buttonsInArea.Add(button);
                }
            }

            return buttonsInArea.ToArray();
        }

        void EnsureVisualSelection()
        {
            if (currentSelectedIndex >= 0 && currentSelectedIndex < inventorySlotButtons.Length)
            {
                UpdateSelectionVisuals(currentSelectedIndex);
            }
        }

        public void SelectSlot(int slotIndex, bool setEventSystemSelection = true)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length)
            {
                LogDebug($"Invalid slot index: {slotIndex}");
                return;
            }

            if (slotIndex == currentSelectedIndex && inventorySlotButtons[slotIndex].gameObject == EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            LogDebug($"Selecting inventory slot {slotIndex}, setEventSystemSelection: {setEventSystemSelection}");

            if (currentSelectedIndex >= 0 && currentSelectedIndex < slotSelectors.Length)
            {
                if (slotSelectors[currentSelectedIndex] != null)
                {
                    slotSelectors[currentSelectedIndex].SetActive(false);
                }

                if (selectorAnimations[currentSelectedIndex] != null)
                {
                    StopCoroutine(selectorAnimations[currentSelectedIndex]);
                    selectorAnimations[currentSelectedIndex] = null;
                }
            }

            currentSelectedIndex = slotIndex;

            UpdateSelectionVisuals(slotIndex);

            UpdateClefs(slotIndex);

            if (setEventSystemSelection && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(inventorySlotButtons[slotIndex].gameObject);
            }
        }

        void UpdateSelectionVisuals(int slotIndex)
        {
            if (slotIndex < slotSelectors.Length && slotSelectors[slotIndex] != null)
            {
                slotSelectors[slotIndex].SetActive(true);

                StopSelectorAnimation(slotIndex);
                selectorAnimations[slotIndex] = AnimateSelector(slotSelectors[slotIndex]);
                StartCoroutine(selectorAnimations[slotIndex]);
            }
        }

        void StopSelectorAnimation(int index)
        {
            if (index >= 0 && index < selectorAnimations.Length && selectorAnimations[index] != null)
            {
                StopCoroutine(selectorAnimations[index]);
                selectorAnimations[index] = null;
            }
        }

        void UpdateClefs(int slotIndex)
        {
            HideAllClefs();

            Button selectedButton = inventorySlotButtons[slotIndex];

            if (selectedButton == null)
            {
                return;
            }

            if (IsChildOf(selectedButton.transform, weaponPentagram))
            {
                if (weaponClef != null)
                {
                    weaponClef.SetActive(true);
                }
            }
            else if (IsChildOf(selectedButton.transform, maskPentagram))
            {
                if (maskClef != null)
                {
                    maskClef.SetActive(true);
                }
            }
            else if (IsChildOf(selectedButton.transform, potionCharmPentagram))
            {
                if (potionCharmClef != null)
                {
                    potionCharmClef.SetActive(true);
                }
            }
        }

        public void OnTabSelected()
        {
            isTabSelected = true;
            LogDebug("Tab selected notification received");
        }

        public void OnTabDeselected()
        {
            isTabSelected = false;
            LogDebug("Tab deselected notification received");
            ResetSelectionState();
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

        void HideAllClefs()
        {
            if (weaponClef != null)
            {
                weaponClef.SetActive(false);
            }
            if (maskClef != null)
            {
                maskClef.SetActive(false);
            }
            if (potionCharmClef != null)
            {
                potionCharmClef.SetActive(false);
            }
        }

        void HideAllSelectors()
        {
            if (slotSelectors == null) return;

            foreach (var selector in slotSelectors)
            {
                if (selector != null)
                {
                    selector.SetActive(false);
                }
            }
        }

        bool IsChildOf(Transform child, GameObject parent)
        {
            if (child == null || parent == null) return false;

            Transform current = child;
            while (current != null)
            {
                if (current.gameObject == parent)
                {
                    return true;
                }
                current = current.parent;
            }

            return false;
        }

        public void Show()
        {
            isActive = true;
            gameObject.SetActive(true);

            LogDebug("Show called - waiting for parent UI to handle selection");
        }

        public void Hide()
        {
            isActive = false;
            isTabSelected = false;
            ResetSelectionState();
            gameObject.SetActive(false);
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[InventoryTabController] {message}</color>");
            }
        }
    }
}