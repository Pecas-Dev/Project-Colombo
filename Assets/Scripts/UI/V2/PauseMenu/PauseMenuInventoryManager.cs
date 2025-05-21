using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using ProjectColombo.GameManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuInventoryManager : MonoBehaviour
    {
        [Header("Inventory References")]
        [SerializeField] PauseMenuInventoryTabController inventoryTabController;
        [SerializeField] PlayerInventory playerInventory; 

        [Header("Charm Slot References")]
        [SerializeField] CharmButton[] charmButtons; 
        [SerializeField] CharmButton legendaryCharmButton;
        //[SerializeField] CharmButton potionButton; 

        [Header("Text Elements")]
        [SerializeField] TextMeshProUGUI charmTitleText;
        [SerializeField] TextMeshProUGUI charmDescriptionText;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

         bool isInitialized = false;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void Start()
        {
            if (charmTitleText == null)
            {
                LogDebug("Charm title text reference is missing!", true);
            }

            if (charmDescriptionText == null)
            {
                LogDebug("Charm description text reference is missing!", true);
            }

            if (charmButtons != null)
            {
                foreach (CharmButton button in charmButtons)
                {
                    if (button != null)
                    {
                        button.isPauseMenuButton = true;
                    }
                }
            }

            if (legendaryCharmButton != null)
                legendaryCharmButton.isPauseMenuButton = true;

            //if (potionButton != null)
            //{
            //    potionButton.isPauseMenuButton = true;
            //}
        }

        void OnEnable()
        {
            LogDebug("Inventory Manager enabled");
            UpdateInventoryDisplay();

            StartCoroutine(MonitorSelection());
        }

        System.Collections.IEnumerator MonitorSelection()
        {
            yield return new WaitForEndOfFrame();

            GameObject lastSelected = null;

            while (gameObject.activeInHierarchy)
            {
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject != lastSelected)
                {
                    lastSelected = EventSystem.current.currentSelectedGameObject;
                    CharmButton button = lastSelected.GetComponent<CharmButton>();

                    if (button != null)
                    {
                        UpdateCharmInfo(button.charmObject);
                        LogDebug($"Selection changed to: {button.name}");
                    }
                }

                yield return null;
            }
        }

        void Initialize()
        {
            LogDebug("Initializing Inventory Manager");

            if (playerInventory == null)
            {
                if (GameManager.Instance != null)
                {
                    playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                    LogDebug("Found PlayerInventory from GameManager");
                }

                if (playerInventory == null)
                {
                    playerInventory = FindFirstObjectByType<PlayerInventory>();
                    LogDebug("Found PlayerInventory in scene");
                }
            }

            if (charmTitleText != null)
            {
                charmTitleText.text = "";
                LogDebug($"Initialized charmTitleText: {charmTitleText.gameObject.name}");
            }

            if (charmDescriptionText != null)
            {
                charmDescriptionText.text = "";
                LogDebug($"Initialized charmDescriptionText: {charmDescriptionText.gameObject.name}");
            }

            isInitialized = true;
        }

        public void UpdateInventoryDisplay()
        {
            if (playerInventory == null)
            {
                LogDebug("Player inventory reference is missing!", true);
                return;
            }

            LogDebug("Updating inventory display");

            UpdateCharmSlots();
        }

        void UpdateCharmSlots()
        {
            if (charmButtons != null)
            {
                foreach (CharmButton button in charmButtons)
                {
                    if (button != null)
                    {
                        button.UpdateInfo(null);
                    }
                }
            }

            if (legendaryCharmButton != null)
            {
                legendaryCharmButton.UpdateInfo(null);
            }

            if (playerInventory == null)
            {
                return;
            }

            if (charmButtons != null && playerInventory.charms != null)
            {
                for (int i = 0; i < playerInventory.charms.Count && i < charmButtons.Length; i++)
                {
                    if (charmButtons[i] != null && playerInventory.charms[i] != null)
                    {
                        charmButtons[i].UpdateInfo(playerInventory.charms[i]);
                    }
                }
            }

            // Update legendary charm slot
            if (legendaryCharmButton != null && playerInventory.legendaryCharms != null && playerInventory.legendaryCharms.Count > 0)
            {
                legendaryCharmButton.UpdateInfo(playerInventory.legendaryCharms[0]);
            }

            //if (potionButton != null)
            //{
            //    potionButton.UpdateInfo(null); 
            //}
        }

        public void UpdateCharmInfo(GameObject charmObject)
        {
            if (charmTitleText == null || charmDescriptionText == null)
            {
                LogDebug("Text components are missing!", true);
                return;
            }

            if (charmObject == null)
            {
                charmTitleText.text = "";
                charmDescriptionText.text = "";

                LogDebug("Cleared charm info display");
                return;
            }

            BaseCharm charmInfo = charmObject.GetComponent<BaseCharm>();
            if (charmInfo == null)
            {
                LogDebug("Selected object doesn't have a BaseCharm component!", true);
                return;
            }

            LogDebug($"Setting charmTitleText to: {charmInfo.charmName}");
            charmTitleText.text = charmInfo.charmName;

            LogDebug($"Setting charmDescriptionText to: {charmInfo.charmDescription}");
            charmDescriptionText.text = charmInfo.charmDescription;

            charmTitleText.ForceMeshUpdate();
            charmDescriptionText.ForceMeshUpdate();

            LogDebug($"Updated charm info display for: {charmInfo.charmName}");
        }

        void LogDebug(string message, bool isWarning = false)
        {
            if (enableDebugLogs)
            {
                if (isWarning)
                    Debug.LogWarning($"[PauseMenuInventoryManager] {message}");
                else
                    Debug.Log($"<color=#FF9900>[PauseMenuInventoryManager] {message}</color>");
            }
        }
    }
}