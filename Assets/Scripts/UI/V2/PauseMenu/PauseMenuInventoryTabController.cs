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

            StartCoroutine(DelayedInitialSelection());
            StartCoroutine(WaitForTabSelection());

            UINavigationManager navigationManager = FindFirstObjectByType<UINavigationManager>();
            if (navigationManager != null && inventorySlotButtons != null && inventorySlotButtons.Length > 0)
            {
                navigationManager.RegisterFirstSelectable(UINavigationState.PauseInventoryTab, inventorySlotButtons[0].gameObject);
                LogDebug("Registered first selectable with navigation manager");
            }
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

            // 0.SlotWeapon, 1.SlotMask, 2.SlotCharm_1, 3.SlotCharm_2, 4.SlotCharm_3, 
            // 5.SlotCharm_4, 6.SlotCharm_5, 7.SlotPotion_1

            switch (buttonIndex)
            {
                // SlotWeapon (0)
                case 0:
                    nav.selectOnUp = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnDown = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnLeft = null;
                    nav.selectOnRight = null;
                    break;

                // SlotMask (1)
                case 1:
                    nav.selectOnUp = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnDown = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnLeft = null;
                    nav.selectOnRight = null;
                    break;

                // SlotCharm_1 (2)
                case 2:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[7]; // SlotPotion_1
                    nav.selectOnRight = inventorySlotButtons[3]; // SlotCharm_2
                    break;

                // SlotCharm_2 (3)
                case 3:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[2]; // SlotCharm_1
                    nav.selectOnRight = inventorySlotButtons[4]; // SlotCharm_3
                    break;

                // SlotCharm_3 (4)
                case 4:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[3]; // SlotCharm_2
                    nav.selectOnRight = inventorySlotButtons[5]; // SlotCharm_4
                    break;

                // SlotCharm_4 (5)
                case 5:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[4]; // SlotCharm_3
                    nav.selectOnRight = inventorySlotButtons[6]; // SlotCharm_5
                    break;

                // SlotCharm_5 (6)
                case 6:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[5]; // SlotCharm_4
                    nav.selectOnRight = inventorySlotButtons[7]; // SlotPotion_1
                    break;

                // SlotPotion_1 (7)
                case 7:
                    nav.selectOnUp = inventorySlotButtons[1]; // SlotMask
                    nav.selectOnDown = inventorySlotButtons[0]; // SlotWeapon
                    nav.selectOnLeft = inventorySlotButtons[6]; // SlotCharm_5
                    nav.selectOnRight = inventorySlotButtons[2]; // SlotCharm_1
                    break;
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

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();

            yield return null;

            if (inventorySlotButtons != null && inventorySlotButtons.Length > 0 && inventorySlotButtons[0] != null)
            {
                SelectSlot(0, true);
                LogDebug("Initial selection set to slot 0");
            }
            else
            {
                LogDebug("Failed to set initial selection - no buttons available");
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