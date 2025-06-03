using ProjectColombo.Objects.Masks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectColombo.UI.MaskSelection;

namespace ProjectColombo.UI
{
    public class MaskCanvas : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text maskNameText;
        public TMP_Text maskDescriptionText;
        public TMP_Text echoNameText;
        public TMP_Text echoAbilityText;

        [Header("MoreInfo Panel References")]
        [SerializeField] GameObject moreInfoPanel;
        [SerializeField] TMP_Text moreInfoMaskNameText;
        [SerializeField] TMP_Text moreInfoMaskDescriptionText;
        [SerializeField] TMP_Text moreInfoEchoNameText;
        [SerializeField] TMP_Text moreInfoEchoAbilityText;

        [Header("Navigation")]
        [SerializeField] MaskSelectionNavigationController navigationController;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        GameObject lastSelected;

        void Start()
        {
            InitializeNavigation();
            FindMoreInfoReferences();

            lastSelected = EventSystem.current.currentSelectedGameObject;
            UpdateTexts();

            LogDebug("MaskCanvas initialized");
        }

        void Update()
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != lastSelected)
            {
                lastSelected = current;
                UpdateTexts();
            }
        }

        void InitializeNavigation()
        {
            if (navigationController == null)
            {
                navigationController = GetComponent<MaskSelectionNavigationController>();

                if (navigationController == null)
                {
                    navigationController = gameObject.AddComponent<MaskSelectionNavigationController>();
                    LogDebug("Added MaskSelectionNavigationController component");
                }
            }

            MaskSelectionNavigationExtension navigationExtension = GetComponent<MaskSelectionNavigationExtension>();
            if (navigationExtension == null)
            {
                navigationExtension = gameObject.AddComponent<MaskSelectionNavigationExtension>();
                LogDebug("Added MaskSelectionNavigationExtension component");
            }

            navigationController.Initialize();
            LogDebug("Navigation components initialized");
        }

        void FindMoreInfoReferences()
        {
            if (moreInfoPanel == null)
            {
                Transform moreInfoTransform = transform.Find("MoreInfo");
                if (moreInfoTransform != null)
                {
                    moreInfoPanel = moreInfoTransform.gameObject;
                    LogDebug($"Found MoreInfo panel: {moreInfoPanel.name}");
                }
                else
                {
                    LogWarning("MoreInfo panel not found as child of Canvas!");
                }
            }

            if (moreInfoPanel != null)
            {
                if (moreInfoMaskNameText == null)
                {
                    Transform titleTransform = moreInfoPanel.transform.Find("Title");
                    if (titleTransform != null)
                    {
                        moreInfoMaskNameText = titleTransform.GetComponent<TMP_Text>();
                        LogDebug("Found MoreInfo Title text component");
                    }
                }

                if (moreInfoMaskDescriptionText == null)
                {
                    Transform descriptionTransform = moreInfoPanel.transform.Find("Description");
                    if (descriptionTransform != null)
                    {
                        moreInfoMaskDescriptionText = descriptionTransform.GetComponent<TMP_Text>();
                        LogDebug("Found MoreInfo Description text component");
                    }
                }

                if (moreInfoEchoNameText == null)
                {
                    Transform echoTransform = moreInfoPanel.transform.Find("Echo");
                    if (echoTransform != null)
                    {
                        moreInfoEchoNameText = echoTransform.GetComponent<TMP_Text>();
                        LogDebug("Found MoreInfo Echo text component");
                    }
                }

                if (moreInfoEchoAbilityText == null)
                {
                    Transform echoDescriptionTransform = moreInfoPanel.transform.Find("Echo Description");
                    if (echoDescriptionTransform != null)
                    {
                        moreInfoEchoAbilityText = echoDescriptionTransform.GetComponent<TMP_Text>();
                        LogDebug("Found MoreInfo Echo Description text component");
                    }
                }
            }
        }

        void UpdateTexts()
        {
            if (lastSelected == null)
            {
                ClearTexts();
                return;
            }

            MaskButton button = lastSelected.GetComponent<MaskButton>();
            if (button == null)
            {
                ClearTexts();
                return;
            }

            if (button.maskPrefab == null)
            {
                LogWarning($"MaskButton {lastSelected.name} has no maskPrefab assigned!");
                ClearTexts();
                return;
            }

            BaseMask mask = button.maskPrefab.GetComponent<BaseMask>();
            if (mask == null)
            {
                LogWarning($"Mask prefab on {lastSelected.name} has no BaseMask component!");
                ClearTexts();
                return;
            }

            if (maskNameText != null)
            {
                maskNameText.text = mask.maskName;
            }

            if (maskDescriptionText != null)
            {
                maskDescriptionText.text = mask.maskDescription;
            }

            if (echoNameText != null)
            {
                echoNameText.text = mask.echoDescription;
            }

            if (echoAbilityText != null && mask.abilityObject != null)
            {
                BaseAbility ability = mask.abilityObject.GetComponent<BaseAbility>();
                if (ability != null)
                {
                    echoAbilityText.text = ability.abilityDescription;
                }
                else
                {
                    echoAbilityText.text = "No ability description available";
                    LogWarning($"Mask {mask.maskName} has abilityObject but no BaseAbility component!");
                }
            }

            UpdateMoreInfoTexts(mask);

            LogDebug($"Updated text display for mask: {mask.maskName}");
        }

        void UpdateMoreInfoTexts(BaseMask mask)
        {
            if (moreInfoPanel == null || !moreInfoPanel.activeInHierarchy)
            {
                return;
            }

            if (moreInfoMaskNameText != null)
            {
                moreInfoMaskNameText.text = mask.maskName;
            }

            if (moreInfoMaskDescriptionText != null)
            {
                moreInfoMaskDescriptionText.text = mask.maskDescription;
            }

            if (moreInfoEchoNameText != null)
            {
                moreInfoEchoNameText.text = mask.echoDescription;
            }

            if (moreInfoEchoAbilityText != null && mask.abilityObject != null)
            {
                BaseAbility ability = mask.abilityObject.GetComponent<BaseAbility>();
                if (ability != null)
                {
                    moreInfoEchoAbilityText.text = ability.abilityDescription;
                }
                else
                {
                    moreInfoEchoAbilityText.text = "No ability description available";
                }
            }

            LogDebug($"Updated MoreInfo display for mask: {mask.maskName}");
        }

        void ClearTexts()
        {
            if (maskNameText != null) maskNameText.text = "";
            if (maskDescriptionText != null) maskDescriptionText.text = "";
            if (echoNameText != null) echoNameText.text = "";
            if (echoAbilityText != null) echoAbilityText.text = "";

            if (moreInfoPanel != null && moreInfoPanel.activeInHierarchy)
            {
                if (moreInfoMaskNameText != null) moreInfoMaskNameText.text = "";
                if (moreInfoMaskDescriptionText != null) moreInfoMaskDescriptionText.text = "";
                if (moreInfoEchoNameText != null) moreInfoEchoNameText.text = "";
                if (moreInfoEchoAbilityText != null) moreInfoEchoAbilityText.text = "";
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (navigationController != null)
            {
                navigationController.Show();
            }

            LogDebug("MaskCanvas shown");
        }

        public void Hide()
        {
            if (navigationController != null)
            {
                navigationController.Hide();
            }

            gameObject.SetActive(false);
            LogDebug("MaskCanvas hidden");
        }

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#00FFFF>[MaskCanvas] {message}</color>");
            }
        }

        void LogWarning(string message)
        {
            Debug.LogWarning($"[MaskCanvas] {message}");
        }
    }
}