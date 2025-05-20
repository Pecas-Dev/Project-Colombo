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

        IEnumerator currentSelectorAnimation;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void OnEnable()
        {
            StartCoroutine(DelayedInitialSelection());

            if (currentSelectedIndex < 0 && inventorySlotButtons.Length > 0)
            {
                SelectSlot(0);
            }
            else if (currentSelectedIndex >= 0)
            {
                SelectSlot(currentSelectedIndex);
            }
        }

        void Update()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                for (int i = 0; i < inventorySlotButtons.Length; i++)
                {
                    if (EventSystem.current.currentSelectedGameObject == inventorySlotButtons[i].gameObject && i != currentSelectedIndex)
                    {
                        SelectSlot(i);
                        break;
                    }
                }
            }
        }

        public void Initialize()
        {
            LogDebug("Initializing inventory tab controller");

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
            }

            isInitialized = true;
        }

        public void SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlotButtons.Length)
            {
                LogDebug($"Invalid slot index: {slotIndex}");
                return;
            }

            if (slotIndex == currentSelectedIndex)
            {
                return;
            }

            LogDebug($"Selecting inventory slot {slotIndex}");

            if (currentSelectedIndex >= 0 && currentSelectedIndex < slotSelectors.Length)
            {
                if (slotSelectors[currentSelectedIndex] != null)
                {
                    slotSelectors[currentSelectedIndex].SetActive(false);
                }
            }

            if (currentSelectorAnimation != null)
            {
                StopCoroutine(currentSelectorAnimation);
                currentSelectorAnimation = null;
            }

            currentSelectedIndex = slotIndex;

            if (currentSelectedIndex < slotSelectors.Length && slotSelectors[currentSelectedIndex] != null)
            {
                slotSelectors[currentSelectedIndex].SetActive(true);

                currentSelectorAnimation = AnimateSelector(slotSelectors[currentSelectedIndex]);
                StartCoroutine(currentSelectorAnimation);
            }

            UpdateClefs(slotIndex);

            EventSystem.current.SetSelectedGameObject(inventorySlotButtons[slotIndex].gameObject);
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

        IEnumerator DelayedInitialSelection()
        {
            yield return new WaitForEndOfFrame();

            if (currentSelectedIndex < 0 && inventorySlotButtons.Length > 0)
            {
                SelectSlot(0);
            }
            else if (currentSelectedIndex >= 0)
            {
                SelectSlot(currentSelectedIndex);
            }
        }
        IEnumerator AnimateSelector(GameObject selector)
        {
            if (selector == null) yield break;

            Image selectorImage = selector.GetComponent<Image>();
            if (selectorImage == null) yield break;

            Color baseColor = selectorImage.color;
            float time = 0f;

            while (true)
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

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00AAFF>[InventoryTabController] {message}</color>");
            }
        }
    }
}