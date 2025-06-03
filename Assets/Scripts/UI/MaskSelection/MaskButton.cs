using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Masks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;


namespace ProjectColombo.UI
{
    public class MaskButton : MonoBehaviour
    {
        [Header("Mask References")]
        public GameObject maskPrefab;
        public Image imageSlot;

        [Header("Visual Feedback")]
        [SerializeField] Color selectedColor = Color.yellow;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color unselectedColor = Color.gray;
        [SerializeField] float scaleMultiplier = 1.1f;
        [SerializeField] Material selectedMaterial;
        [SerializeField] float tweenDuration = 0.3f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = false;

        Vector3 originalScale;
        Color originalColor;
        bool isSelected = false;

        void Start()
        {
            InitializeMaskButton();
        }

        void InitializeMaskButton()
        {
            if (maskPrefab != null && imageSlot != null)
            {
                BaseMask mask = maskPrefab.GetComponent<BaseMask>();
                if (mask != null && mask.maskPicture != null)
                {
                    imageSlot.sprite = mask.maskPicture;
                    LogDebug($"Set mask image for {mask.maskName}");
                }
                else
                {
                    LogWarning($"Mask prefab {maskPrefab.name} has no BaseMask component or maskPicture!");
                }
            }

            originalScale = transform.localScale;

            if (imageSlot != null)
            {
                originalColor = imageSlot.color;
            }

            SetVisualState(false);

            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                LogDebug("Removed onClick listeners to prevent direct mask selection");
            }

            SetupEventTriggers();

            LogDebug($"MaskButton {gameObject.name} initialized");
        }

        void SetupEventTriggers()
        {
            Button button = GetComponent<Button>();
            if (button == null)
            {
                LogWarning("No Button component found on MaskButton!");
                return;
            }

            EventTrigger eventTrigger = GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            eventTrigger.triggers.Clear();

            EventTrigger.Entry selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener((data) => OnSelected());
            eventTrigger.triggers.Add(selectEntry);

            EventTrigger.Entry deselectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Deselect
            };
            deselectEntry.callback.AddListener((data) => OnDeselected());
            eventTrigger.triggers.Add(deselectEntry);

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener((data) => OnPointerEnter());
            eventTrigger.triggers.Add(pointerEnterEntry);

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener((data) => OnPointerExit());
            eventTrigger.triggers.Add(pointerExitEntry);

            LogDebug("Event triggers setup complete");
        }

        void OnSelected()
        {
            SetVisualState(true);
            LogDebug($"Mask button {gameObject.name} selected");
        }

        void OnDeselected()
        {
            SetVisualState(false);
            LogDebug($"Mask button {gameObject.name} deselected");
        }

        public void SetUnselected()
        {
            if (isSelected)
            {
                SetVisualState(false);
            }
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        void OnPointerEnter()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
            LogDebug($"Mouse entered mask button {gameObject.name}");
        }

        void OnPointerExit()
        {
            LogDebug($"Mouse exited mask button {gameObject.name}");
        }

        void OnSubmit()
        {
            LogDebug($"Submit triggered on mask button {gameObject.name}");
        }

        public void SetVisualState(bool selected)
        {
            isSelected = selected;

            if (selected)
            {
                imageSlot.gameObject.transform.DOScale(originalScale * scaleMultiplier, tweenDuration).SetEase(Ease.OutBack);

                if (imageSlot != null)
                {
                    imageSlot.DOColor(selectedColor, tweenDuration);
                }

                if (imageSlot != null && selectedMaterial != null)
                {
                    imageSlot.material = selectedMaterial;
                    LogDebug($"Applied selected material to {gameObject.name}");
                }

                LogDebug($"Selected visual state applied to {gameObject.name}");
            }
            else
            {
                imageSlot.gameObject.transform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutQuart);

                if (imageSlot != null)
                {
                    imageSlot.DOColor(unselectedColor, tweenDuration);
                }

                if (imageSlot != null)
                {
                    imageSlot.material = null;
                    LogDebug($"Removed material from {gameObject.name}");
                }

                LogDebug($"Unselected visual state applied to {gameObject.name}");
            }
        }

        public void SelectMask()
        {
            if (maskPrefab == null)
            {
                LogWarning("Cannot select mask - maskPrefab is null!");
                return;
            }

            BaseMask mask = maskPrefab.GetComponent<BaseMask>();
            if (mask == null)
            {
                LogWarning($"Mask prefab {maskPrefab.name} has no BaseMask component!");
                return;
            }

            LogDebug($"Selecting mask: {mask.maskName}");

            PlayerInventory playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.EquipMask(maskPrefab);
                LogDebug($"Equipped mask: {mask.maskName}");
            }
            else
            {
                LogWarning("PlayerInventory not found on GameManager!");
            }

            Animator curtain = GameObject.FindGameObjectWithTag("Transition")?.GetComponent<Animator>();
            if (curtain != null)
            {
                StartCoroutine(Transition(curtain));
            }
            else
            {
                LogWarning("Transition animator not found! Loading scene directly.");
                LoadNextScene();
            }
        }

        IEnumerator Transition(Animator curtain)
        {
            curtain.Play("Close");
            LogDebug("Started transition animation");

            yield return new WaitForSecondsRealtime(2.5f);

            LoadNextScene();
        }

        void LoadNextScene()
        {
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            LogDebug($"Loading scene with build index: {nextScene}");
            SceneManager.LoadScene(nextScene);
        }

        public void SetMaskPrefab(GameObject newMaskPrefab)
        {
            maskPrefab = newMaskPrefab;

            if (imageSlot != null && maskPrefab != null)
            {
                BaseMask mask = maskPrefab.GetComponent<BaseMask>();
                if (mask != null && mask.maskPicture != null)
                {
                    imageSlot.sprite = mask.maskPicture;
                    LogDebug($"Updated mask image for {mask.maskName}");
                }
            }
        }

        public BaseMask GetMaskInfo()
        {
            if (maskPrefab != null)
            {
                return maskPrefab.GetComponent<BaseMask>();
            }
            return null;
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#FF8800>[MaskButton] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[MaskButton] {message}");
        }
    }
}