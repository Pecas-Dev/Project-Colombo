using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectColombo.Combat;
using ProjectColombo.Inventory;
using UnityEngine.SceneManagement;
using ProjectColombo.Objects.Masks;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.UI
{
    public class PlayerHUDManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the TextMeshProUGUI component displaying currency amount")]
        [SerializeField] TextMeshProUGUI currencyText;

        [Tooltip("Reference to the Image component displaying the equipped mask")]
        [SerializeField] Image maskImage;

        [Header("Debug Settings")]
        [Tooltip("Enable debug logs for echo mission updates")]
        [SerializeField] bool debugEchoMission = false;

        [Tooltip("Reference to the TextMeshProUGUI component displaying echo mission title")]
        [SerializeField] TextMeshProUGUI echoMissionTitleText;

        [Tooltip("Reference to the TextMeshProUGUI component displaying echo mission progress")]
        [SerializeField] TextMeshProUGUI echoMissionProgressText;

        [Tooltip("How long until the mission text fades out")]
        [SerializeField] float missionTextDisplayTime = 5f;

        [Tooltip("How fast the text fades in/out")]
        [SerializeField] float missionTextFadeSpeed = 2f;

        [Tooltip("The color to transition to when echo is unlocked")]
        [SerializeField] Color echoUnlockedColor = Color.red;


        string lastMissionProgressText = "";

        float lastEventTime;
        float missionTextTimer;

        int currentCurrencyAmount = -1;

        bool shouldBeVisible = true;
        bool isFadingIn = false;
        bool isFadingOut = false;
        bool echoWasUnlocked = false;
        bool isChangingLevel = false;



        PlayerInventory playerInventory;
        BaseMask currentMask;

        Coroutine fadeCoroutine;
        Sprite currentMaskSprite = null;



        void Awake()
        {
            if (currencyText == null)
            {
                Debug.LogError("Currency Text reference is missing in PlayerHUDManager!");
            }

            if (maskImage == null)
            {
                Debug.LogError("Mask Image reference is missing in PlayerHUDManager!");
            }

            if (echoMissionTitleText == null)
            {
                Debug.LogError("Echo Mission Title Text reference is missing in PlayerHUDManager!");
            }

            if (echoMissionProgressText == null)
            {
                Debug.LogError("Echo Mission Progress Text reference is missing in PlayerHUDManager!");
            }

            if (echoMissionTitleText != null)
            {
                Color color = echoMissionTitleText.color;
                color.a = 1f;
                echoMissionTitleText.color = color;
            }

            if (echoMissionProgressText != null)
            {
                Color color = echoMissionProgressText.color;
                color.a = 1f;
                echoMissionProgressText.color = color;
            }
        }

        void Start()
        {
            CustomEvents.OnCoinsCollected += HandleCoinsCollected;
            CustomEvents.OnMaskCollected += HandleMaskCollected;
            CustomEvents.OnLevelChange += HandleLevelChanged;
            CustomEvents.OnEchoUnlocked += HandleEchoUnlocked;

            CustomEvents.OnMaxHealthGained += HandleMaxHealthGained;
            CustomEvents.OnEnemyDeath += HandleEnemyDeath;
            CustomEvents.OnDamageDelt += HandleDamageDelt;

            FindPlayerReferences();
            UpdateUI();

            missionTextTimer = missionTextDisplayTime;
            lastEventTime = Time.time;
        }

        void OnDestroy()
        {
            CustomEvents.OnCoinsCollected -= HandleCoinsCollected;
            CustomEvents.OnMaskCollected -= HandleMaskCollected;
            CustomEvents.OnLevelChange -= HandleLevelChanged;
            CustomEvents.OnEchoUnlocked -= HandleEchoUnlocked;

            CustomEvents.OnMaxHealthGained -= HandleMaxHealthGained;
            CustomEvents.OnEnemyDeath -= HandleEnemyDeath;
            CustomEvents.OnDamageDelt -= HandleDamageDelt;

        }

        void FindPlayerReferences()
        {
            if (GameManager.Instance != null)
            {
                playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();

                if (playerInventory == null)
                {
                    Debug.LogWarning("PlayerInventory not found on GameManager, checking for direct access...");
                }
            }
            else
            {
                Debug.LogWarning("GameManager instance not found, attempting to find PlayerInventory directly...");
            }

            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();

                if (playerInventory == null)
                {
                    Debug.LogError("PlayerInventory not found in scene! HUD updates will not work correctly.");
                }
            }
        }

        void Update()
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (isChangingLevel)
            {
                if (echoMissionTitleText != null && echoMissionTitleText.color.a > 0)
                {
                    Color titleColor = echoMissionTitleText.color;
                    titleColor.a = 0f;
                    echoMissionTitleText.color = titleColor;
                }

                if (echoMissionProgressText != null && echoMissionProgressText.color.a > 0)
                {
                    Color progressColor = echoMissionProgressText.color;
                    progressColor.a = 0f;
                    echoMissionProgressText.color = progressColor;
                }

                return;
            }

            if (Time.frameCount % 10 == 0)
            {
                UpdateUI();

                if (currentMask != null && currentMask.echoMission != null && (!currentMask.echoUnlocked || isLevelOne) && !isChangingLevel)
                {
                    RefreshMissionDisplay();
                }
            }

            if (!isChangingLevel && shouldBeVisible && Time.time - lastEventTime > missionTextDisplayTime && !isFadingOut && !isFadingIn && echoMissionTitleText != null && echoMissionTitleText.color.a > 0 && (!echoWasUnlocked || isLevelOne))
            {
                StartFadeOut();
            }
        }

        #region Event Handlers
        void HandleCoinsCollected(int amount)
        {
            Debug.Log($"Coins collected: {amount}");
            UpdateCurrencyDisplay();

            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (currentMask != null && currentMask.echoMission != null && currentMask.echoMission is CollectGold && (!currentMask.echoUnlocked || isLevelOne))
            {
                RefreshMissionDisplay();
                TriggerFadeIn();
            }
        }

        void HandleMaxHealthGained(int amount)
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (currentMask != null && currentMask.echoMission != null && currentMask.echoMission is CollectMaxHealth && (!currentMask.echoUnlocked || isLevelOne))
            {
                RefreshMissionDisplay();
                TriggerFadeIn();
            }
        }

        void HandleEnemyDeath(GameGlobals.MusicScale scale, GameObject enemy)
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (currentMask != null && currentMask.echoMission != null && currentMask.echoMission is MajorKills && (!currentMask.echoUnlocked || isLevelOne))
            {
                if (scale == GameGlobals.MusicScale.MAJOR)
                {
                    RefreshMissionDisplay();
                    TriggerFadeIn();
                }
            }
        }

        void HandleDamageDelt(int damage, GameGlobals.MusicScale scale, bool sameScale, HealthManager enemyHealthManager, int comboLength)
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (currentMask != null && currentMask.echoMission != null && currentMask.echoMission is DamageDelt && (!currentMask.echoUnlocked || isLevelOne))
            {
                RefreshMissionDisplay();
                TriggerFadeIn();
            }
        }

        void HandleMaskCollected(GameObject mask)
        {
            Debug.Log($"Mask collected: {mask.name}");
            Invoke("UpdateMaskDisplay", 0.1f);
        }

        void HandleLevelChangeStarted()
        {
            isChangingLevel = true;
            DebugLog("Level change started - hiding mission text");

            if (echoMissionTitleText != null && echoMissionProgressText != null)
            {
                Color titleColor = echoMissionTitleText.color;
                Color progressColor = echoMissionProgressText.color;

                titleColor.a = 0f;
                progressColor.a = 0f;

                echoMissionTitleText.color = titleColor;
                echoMissionProgressText.color = progressColor;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }

        void HandleLevelChanged()
        {
            Debug.Log("Level changed, updating UI");

            isChangingLevel = false;

            Invoke("FindPlayerReferences", 0.5f);
            Invoke("UpdateUI", 1.0f);

            if (SceneManager.GetActiveScene().name == "03_LevelTwo")
            {
                echoWasUnlocked = false;
                shouldBeVisible = true;
                lastMissionProgressText = "";

                Invoke("ResetEchoStateInLevelOne", 0.7f);

                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                    fadeCoroutine = null;
                }

                isFadingIn = false;
                isFadingOut = false;

                Invoke("RefreshMissionDisplay", 1.5f);
                Invoke("ForceTextVisible", 1.6f);

                DebugLog("Reset mission display state for Level One");
            }
        }

        void HandleEchoUnlocked()
        {
            echoWasUnlocked = true;

            if (isChangingLevel)
            {
                DebugLog("Echo unlocked during level change - suppressing text display");
                return;
            }

            if (currentMask != null && currentMask.echoUnlocked)
            {
                RefreshMissionDisplay();
                StartCoroutine(TransitionToUnlockedColor());
            }
        }
        #endregion

        #region UI Updates
        void UpdateUI()
        {
            UpdateCurrencyDisplay();
            UpdateMaskDisplay();
            UpdateEchoMissionDisplay();
        }

        void UpdateCurrencyDisplay()
        {
            if (currencyText == null) return;

            int currentAmount = 0;

            if (playerInventory != null)
            {
                currentAmount = playerInventory.currencyAmount;
            }
            else if (GameManager.Instance != null)
            {
                var inventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    currentAmount = inventory.currencyAmount;
                }
            }

            if (currentAmount != currentCurrencyAmount)
            {
                currentCurrencyAmount = currentAmount;
                currencyText.text = currentAmount.ToString();
                Debug.Log($"Updated currency display: {currentAmount}");
            }
        }

        void UpdateMaskDisplay()
        {
            if (maskImage == null)
            {
                return;
            }

            GameObject maskSlot = null;

            if (playerInventory != null && playerInventory.maskSlot != null)
            {
                maskSlot = playerInventory.maskSlot;
            }
            else if (GameManager.Instance != null)
            {
                var inventory = GameManager.Instance.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.maskSlot != null)
                {
                    maskSlot = inventory.maskSlot;
                }
            }

            if (maskSlot != null && maskSlot.transform.childCount > 0)
            {
                BaseMask equippedMask = maskSlot.transform.GetChild(0).GetComponent<BaseMask>();

                if (equippedMask != null)
                {
                    if (equippedMask != currentMask)
                    {
                        currentMask = equippedMask;
                        echoWasUnlocked = currentMask.echoUnlocked;
                        RefreshMissionDisplay();

                        if (!currentMask.echoUnlocked)
                        {
                            TriggerFadeIn();
                        }
                    }

                    if (equippedMask.maskPicture != currentMaskSprite)
                    {
                        RectTransform rectTransform = maskImage.rectTransform;
                        rectTransform.sizeDelta = new Vector2(110, 110);

                        currentMaskSprite = equippedMask.maskPicture;
                        maskImage.sprite = currentMaskSprite;
                        maskImage.enabled = true;
                        Debug.Log($"Updated mask display: {equippedMask.maskName}");
                    }
                }
            }
            else
            {
                if (currentMaskSprite != null)
                {
                    currentMaskSprite = null;
                    maskImage.sprite = null;
                    maskImage.enabled = false;
                    currentMask = null;
                    Debug.Log("No mask equipped, hiding mask display");

                    if (echoMissionTitleText != null)
                    {
                        echoMissionTitleText.text = "";
                    }

                    if (echoMissionProgressText != null)
                    {
                        echoMissionProgressText.text = "";
                    }
                }
            }
        }

        void UpdateEchoMissionDisplay()
        {
            if (currentMask == null || echoMissionTitleText == null || echoMissionProgressText == null)
            {
                return;
            }

            if (currentMask.echoUnlocked && !echoWasUnlocked)
            {
                echoWasUnlocked = true;
                StartCoroutine(TransitionToUnlockedColor());
                return;
            }

            RefreshMissionDisplay();
        }

        void RefreshMissionDisplay()
        {
            if (isChangingLevel)
            {
                return;
            }

            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (currentMask == null || echoMissionTitleText == null || echoMissionProgressText == null)
            {
                return;
            }

            if ((currentMask.echoUnlocked || echoWasUnlocked) && !isLevelOne)
            {
                if (echoMissionTitleText.color.a > 0 || echoMissionProgressText.color.a > 0)
                {
                    Color titleColor = echoMissionTitleText.color;
                    Color progressColor = echoMissionProgressText.color;

                    titleColor.a = 0f;
                    progressColor.a = 0f;

                    echoMissionTitleText.color = titleColor;
                    echoMissionProgressText.color = progressColor;
                }
                return;
            }

            if (isChangingLevel)
            {
                return;
            }

            echoMissionTitleText.text = $"Echo Mission ({currentMask.maskName})";
            DebugLog($"Refreshing mission display for: {currentMask.maskName}");

            string progressText = "";


            if (currentMask.echoMission is CollectGold collectGold)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(CollectGold).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentCollected = 0;
                if (fieldInfo != null)
                {
                    currentCollected = (int)fieldInfo.GetValue(collectGold);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'currentCollected' field in CollectGold");
                }

                DebugLog($"CollectGold mission - Current: {currentCollected}, Target: {collectGold.goldToUnlockEcho}");
                progressText = $"Gold Found: {currentCollected} / {collectGold.goldToUnlockEcho}";
            }
            else if (currentMask.echoMission is CollectMaxHealth collectHealth)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(CollectMaxHealth).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentCollected = 0;
                if (fieldInfo != null)
                {
                    currentCollected = (int)fieldInfo.GetValue(collectHealth);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'currentCollected' field in CollectMaxHealth");
                }

                DebugLog($"CollectMaxHealth mission - Current: {currentCollected}, Target: {collectHealth.maxHealthToCollect}");
                progressText = $"Collected Health: {currentCollected} / {collectHealth.maxHealthToCollect}";
            }
            else if (currentMask.echoMission is MajorKills majorKills)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(MajorKills).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentKills = 0;
                if (fieldInfo != null)
                {
                    currentKills = (int)fieldInfo.GetValue(majorKills);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'currentCollected' field in MajorKills");
                }

                System.Reflection.FieldInfo requiredKillsField = typeof(MajorKills).GetField("majorKillsToDo", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                int requiredKills = 0;
                if (requiredKillsField != null)
                {
                    requiredKills = (int)requiredKillsField.GetValue(majorKills);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'majorKillsToDo' field in MajorKills");
                }

                DebugLog($"MajorKills mission - Current: {currentKills}, Target: {requiredKills}");
                progressText = $"Enemies Killed with Major Scale Attack: {currentKills} / {requiredKills}";
            }
            else if (currentMask.echoMission is DamageDelt damageDelt)
            {
                System.Reflection.FieldInfo fieldInfo = typeof(DamageDelt).GetField("currentCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                int currentDamage = 0;
                if (fieldInfo != null)
                {
                    currentDamage = (int)fieldInfo.GetValue(damageDelt);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'currentCollected' field in DamageDelt");
                }

                System.Reflection.FieldInfo requiredDamageField = typeof(DamageDelt).GetField("damageToDeal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                int requiredDamage = 0;
                if (requiredDamageField != null)
                {
                    requiredDamage = (int)requiredDamageField.GetValue(damageDelt);
                }
                else
                {
                    DebugLog("ERROR: Cannot find 'damageToDeal' field in DamageDelt");
                }

                DebugLog($"DamageDelt mission - Current: {currentDamage}, Target: {requiredDamage}");
                progressText = $"Damage Dealt: {currentDamage} / {requiredDamage}";
            }
            else
            {
                DebugLog($"Unknown mission type: {currentMask.echoMission.GetType().Name}");
                progressText = "Unknown Mission Type";
            }

            if (progressText != lastMissionProgressText)
            {
                lastMissionProgressText = progressText;
                echoMissionProgressText.text = progressText;

                if (!currentMask.echoUnlocked || isLevelOne)
                {
                    TriggerFadeIn();
                }
            }

        }
        #endregion

        #region Fade Effects
        void TriggerFadeIn()
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (isChangingLevel || (currentMask != null && (currentMask.echoUnlocked || echoWasUnlocked) && !isLevelOne))
            {
                return;
            }

            lastEventTime = Time.time;

            if (!isFadingIn && ((echoMissionTitleText != null && echoMissionTitleText.color.a < 1f) ||(echoMissionProgressText != null && echoMissionProgressText.color.a < 1f)))
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeTextIn());
            }
            else
            {
                shouldBeVisible = true;
            }
        }

        void StartFadeOut()
        {
            bool isLevelOne = SceneManager.GetActiveScene().name == "03_LevelTwo";

            if (!isFadingOut && !isFadingIn && currentMask != null && (!currentMask.echoUnlocked || isLevelOne) && (!echoWasUnlocked || isLevelOne))
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeTextOut());
            }
        }

        IEnumerator FadeTextIn()
        {
            isFadingIn = true;
            isFadingOut = false;
            shouldBeVisible = true;

            while ((echoMissionTitleText != null && echoMissionTitleText.color.a < 1f) || (echoMissionProgressText != null && echoMissionProgressText.color.a < 1f))
            {
                if (echoMissionTitleText != null)
                {
                    Color color = echoMissionTitleText.color;
                    color.a = Mathf.Min(1f, color.a + Time.deltaTime * missionTextFadeSpeed);
                    echoMissionTitleText.color = color;
                }

                if (echoMissionProgressText != null)
                {
                    Color color = echoMissionProgressText.color;
                    color.a = Mathf.Min(1f, color.a + Time.deltaTime * missionTextFadeSpeed);
                    echoMissionProgressText.color = color;
                }

                yield return null;
            }

            isFadingIn = false;
            fadeCoroutine = null;
        }

        IEnumerator FadeTextOut()
        {
            isFadingOut = true;
            isFadingIn = false;
            shouldBeVisible = false;

            while ((echoMissionTitleText != null && echoMissionTitleText.color.a > 0f) || (echoMissionProgressText != null && echoMissionProgressText.color.a > 0f))
            {
                if (echoMissionTitleText != null)
                {
                    Color color = echoMissionTitleText.color;
                    color.a = Mathf.Max(0f, color.a - Time.deltaTime * missionTextFadeSpeed);
                    echoMissionTitleText.color = color;
                }

                if (echoMissionProgressText != null)
                {
                    Color color = echoMissionProgressText.color;
                    color.a = Mathf.Max(0f, color.a - Time.deltaTime * missionTextFadeSpeed);
                    echoMissionProgressText.color = color;
                }

                yield return null;
            }

            isFadingOut = false;
            fadeCoroutine = null;
        }

        IEnumerator TransitionToUnlockedColor()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            isFadingIn = false;
            isFadingOut = false;
            shouldBeVisible = true;
            echoWasUnlocked = true;

            if (isChangingLevel)
            {
                DebugLog("Level changing during transition - aborting color transition");
                yield break;
            }

            if (echoMissionTitleText != null)
            {
                Color color = echoMissionTitleText.color;
                color.a = 1f;
                echoMissionTitleText.color = color;
            }

            if (echoMissionProgressText != null)
            {
                Color color = echoMissionProgressText.color;
                color.a = 1f;
                echoMissionProgressText.color = color;

                echoMissionProgressText.text = "Echo Unlocked!";
            }

            float transitionTime = 0f;
            float transitionDuration = 1.5f;

            Color originalTitleColor = echoMissionTitleText.color;
            Color originalProgressColor = echoMissionProgressText.color;

            Color targetTitleColor = new Color(echoUnlockedColor.r, echoUnlockedColor.g, echoUnlockedColor.b, 1f);
            Color targetProgressColor = new Color(echoUnlockedColor.r, echoUnlockedColor.g, echoUnlockedColor.b, 1f);

            while (transitionTime < transitionDuration)
            {
                transitionTime += Time.deltaTime;
                float t = transitionTime / transitionDuration;

                if (echoMissionTitleText != null)
                {
                    echoMissionTitleText.color = Color.Lerp(originalTitleColor, targetTitleColor, t);
                }

                if (echoMissionProgressText != null)
                {
                    echoMissionProgressText.color = Color.Lerp(originalProgressColor, targetProgressColor, t);
                }

                yield return null;
            }

            yield return new WaitForSeconds(2f);

            yield return FadeTextOut();

            shouldBeVisible = false;
        }
        #endregion

        public void ForceUpdateHUD()
        {
            FindPlayerReferences();
            UpdateUI();
        }

        void ForceTextVisible()
        {
            if (currentMask == null || echoMissionTitleText == null || echoMissionProgressText == null)
                return;

            Color titleColor = echoMissionTitleText.color;
            Color progressColor = echoMissionProgressText.color;

            titleColor.a = 1f;
            progressColor.a = 1f;

            echoMissionTitleText.color = titleColor;
            echoMissionProgressText.color = progressColor;

            lastEventTime = Time.time;

            DebugLog("Forced mission text visible after Level One load");
        }

        void DebugLog(string message)
        {
            if (debugEchoMission)
            {
                Debug.Log($"<color=#FFAA00>[Echo Mission] {message}</color>");
            }
        }
    }
}