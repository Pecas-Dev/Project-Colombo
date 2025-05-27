using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectColombo.Combat;
using ProjectColombo.Inventory;
using UnityEngine.SceneManagement;
using ProjectColombo.GameManagement;
using ProjectColombo.Objects.Charms;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.GameManagement.Stats;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuStatsTabController : MonoBehaviour
    {
        [Header("Stats UI References")]
        [SerializeField] Image[] statBarImages;
        [SerializeField] TextMeshProUGUI[] statValueTexts;

        [Header("Graph References")]
        [SerializeField] GameObject graphLines;

        [Header("Player References (Real-Time Stats)")]
        [SerializeField] PlayerStateMachine playerStateMachine;
        [SerializeField] EntityAttributes playerEntityAttributes;
        [SerializeField] HealthManager playerHealthManager;
        [SerializeField] PlayerInventory playerInventory;

        [Header("Global Stats Reference")]
        [SerializeField] GlobalStats globalStats;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        Coroutine[] statBarAnimations;
        bool isInitialized = false;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void OnEnable()
        {
            UpdateStats();
            AnimateStatBars();
            LogDebug("Stats tab enabled with real-time stats");
        }

        public void Initialize()
        {
            LogDebug("Initializing Stats Tab with real-time player references");

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                playerStateMachine = playerObject.GetComponent<PlayerStateMachine>();
                playerEntityAttributes = playerObject.GetComponent<EntityAttributes>();
                playerHealthManager = playerObject.GetComponent<HealthManager>();
                LogDebug("Found Player GameObject with real-time components");
            }
            else
            {
                LogDebug("Player GameObject not found - stats may not be real-time!", true);
            }

            if (GameManager.Instance != null)
            {
                playerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                globalStats = GameManager.Instance.GetComponent<GlobalStats>();
                LogDebug("Found GameManager components (PlayerInventory and GlobalStats)");
            }
            else
            {
                LogDebug("GameManager.Instance not found!", true);
            }

            if (statBarImages != null)
            {
                statBarAnimations = new Coroutine[statBarImages.Length];
            }

            isInitialized = true;
        }

        public void UpdateStats()
        {
            LogDebug("Updating stats with real-time values");

            bool shouldResetStats = (SceneManager.GetActiveScene().buildIndex == 1);

            if (statValueTexts != null && statValueTexts.Length >= 5)
            {
                if (shouldResetStats)
                {
                    LogDebug("Scene build index is 1 - setting all stats to 0");

                    statValueTexts[0].text = "Damage (0%)";
                    statValueTexts[1].text = "Defense (0%)";
                    statValueTexts[2].text = "Attack Speed (0%)";
                    statValueTexts[3].text = "Move Speed (0%)";
                    statValueTexts[4].text = "Luck (0%)";
                    statValueTexts[5].text = "Critical Chance (0%)";
                    statValueTexts[6].text = "Evade Chance (0%)";
                }
                else
                {

                    // 1. Damage (calculate estimated damage boost from charms + base multiplier)
                    float damagePercent = GetEstimatedDamageBoostPercent();
                    statValueTexts[0].text = "Damage (" + Mathf.RoundToInt(damagePercent).ToString() + "%)";
                    LogDebug($"Updated Damage: {damagePercent}%");

                    // 2. Defense (calculate estimated damage reduction from charms + base block)
                    float defensePercent = GetEstimatedDefensePercent();
                    statValueTexts[1].text = "Defense (" + Mathf.RoundToInt(defensePercent).ToString() + "%)";
                    LogDebug($"Updated Defense: {defensePercent}%");

                    // 3. Attack Speed (percentage increase from base)
                    float attackSpeedPercent = GetAttackSpeedIncreasePercent();
                    statValueTexts[2].text = "Attack Speed (" + Mathf.RoundToInt(attackSpeedPercent).ToString() + "%)";
                    LogDebug($"Updated Attack Speed: {attackSpeedPercent}%");

                    // 4. Movement Speed (percentage increase from base)
                    float movementSpeedPercent = GetMovementSpeedIncreasePercent();
                    statValueTexts[3].text = "Move Speed (" + Mathf.RoundToInt(movementSpeedPercent).ToString() + "%)";
                    LogDebug($"Updated Movement Speed: {movementSpeedPercent}%");

                    // 5. Luck (each point = 1%)
                    int luckPercent = GetRealTimeLuck();
                    statValueTexts[4].text = "Luck (" + luckPercent.ToString() + "%)";
                    LogDebug($"Updated Luck: {luckPercent}%");

                    // 6. Critical Chance 
                    if (statValueTexts.Length > 5)
                    {
                        int criticalChance = GetCriticalChancePercent();
                        statValueTexts[5].text = "Critical Chance (" + criticalChance.ToString() + "%)";
                        LogDebug($"Updated Critical Chance: {criticalChance}%");
                    }

                    // 7. Evade Chance 
                    if (statValueTexts.Length > 6)
                    {
                        int evadeChance = GetEvadeChancePercent();
                        statValueTexts[6].text = "Evade Chance (" + evadeChance.ToString() + "%)";
                        LogDebug($"Updated Evade Chance: {evadeChance}%");
                    }
                }
            }
            else
            {
                LogDebug("WARNING: statValueTexts array is null or has insufficient elements!", true);
            }
        }

        float GetMovementSpeedIncreasePercent()
        {
            if (playerEntityAttributes != null && globalStats != null)
            {
                float currentSpeed = playerEntityAttributes.moveSpeed;
                float baseSpeed = globalStats.defaultPlayerSpeed;
                return ((currentSpeed - baseSpeed) / baseSpeed) * 100f;
            }
            return 0f;
        }

        float GetAttackSpeedIncreasePercent()
        {
            if (playerEntityAttributes != null && globalStats != null)
            {
                float currentAttackSpeed = playerEntityAttributes.attackSpeed;
                float baseAttackSpeed = globalStats.defaultPlayerAttackSpeed;
                return ((currentAttackSpeed - baseAttackSpeed) / baseAttackSpeed) * 100f;
            }
            return 0f;
        }

        int GetRealTimeLuck()
        {
            if (playerInventory != null)
            {
                return playerInventory.currentLuck;
            }

            if (globalStats != null)
            {
                return globalStats.currentLuckPoints;
            }

            return 0;
        }

        int GetCriticalChancePercent()
        {
            int currentLuck = GetRealTimeLuck();
            return Mathf.RoundToInt(currentLuck / 2f);
        }

        int GetEvadeChancePercent()
        {
            int currentLuck = GetRealTimeLuck();
            return Mathf.RoundToInt(currentLuck / 5f);
        }

        float GetEstimatedDamageBoostPercent()
        {
            float totalDamageBoost = 0f;

            if (globalStats != null)
            {
                float baseDamageMultiplier = globalStats.currentMajorDamageMultiplyer;
                totalDamageBoost += (baseDamageMultiplier - 1.0f) * 100f;
            }

            if (playerInventory != null)
            {
                foreach (GameObject charm in playerInventory.charms)
                {
                    if (charm != null)
                    {
                        totalDamageBoost += GetCharmDamageBonus(charm);
                    }
                }
                foreach (GameObject legendaryCharm in playerInventory.legendaryCharms)
                {
                    if (legendaryCharm != null)
                    {
                        totalDamageBoost += GetCharmDamageBonus(legendaryCharm);
                    }
                }
            }

            return Mathf.Max(0f, totalDamageBoost);
        }

        float GetEstimatedDefensePercent()
        {
            float totalDefense = 0f;

            if (globalStats != null)
            {
                totalDefense += globalStats.currentBlockReductionPercent;
            }

            if (playerInventory != null)
            {
                foreach (GameObject charm in playerInventory.charms)
                {
                    if (charm != null)
                    {
                        totalDefense += GetCharmDefenseBonus(charm);
                    }
                }

                foreach (GameObject legendaryCharm in playerInventory.legendaryCharms)
                {
                    if (legendaryCharm != null)
                    {
                        totalDefense += GetCharmDefenseBonus(legendaryCharm);
                    }
                }
            }

            return Mathf.Max(0f, totalDefense);
        }

        float GetCharmDamageBonus(GameObject charm)
        {
            if (charm == null) return 0f;

            float damageBonus = 0f;

            DamagePercentage damagePercentage = charm.GetComponent<DamagePercentage>();

            if (damagePercentage != null)
            {
                damageBonus += damagePercentage.majorDamagePercentage;
            }

            DamagePercentageLuck damageLuck = charm.GetComponent<DamagePercentageLuck>();
            if (damageLuck != null && playerInventory != null)
            {
                int luckMultiplier = Mathf.FloorToInt(playerInventory.currentLuck / damageLuck.forHowManyLuck);
                damageBonus += luckMultiplier * damageLuck.damagePercentageLuck;
            }

            return damageBonus;
        }

        float GetCharmDefenseBonus(GameObject charm)
        {
            if (charm == null) return 0f;

            float defenseBonus = 0f;

            BlockPercentage blockPercentage = charm.GetComponent<BlockPercentage>();

            if (blockPercentage != null)
            {
                defenseBonus += blockPercentage.extraBlockDamagePercentage;
            }

            return defenseBonus;
        }

        void AnimateStatBars()
        {
            if (statBarImages == null)
            {
                LogDebug("Cannot animate stat bars: statBarImages array is null");
                return;
            }

            bool shouldResetStats = (SceneManager.GetActiveScene().buildIndex == 1);

            float[] statValues;

            if (shouldResetStats)
            {
                statValues = new float[statBarImages.Length];
                for (int i = 0; i < statValues.Length; i++)
                {
                    statValues[i] = 0f;
                }
            }
            else
            {
                statValues = new float[]
                {
                    Mathf.Clamp01(GetEstimatedDamageBoostPercent() / 100f), // Damage 
                    Mathf.Clamp01(GetEstimatedDefensePercent() / 100f), // Defense 
                    Mathf.Clamp01(GetAttackSpeedIncreasePercent() / 100f), // Attack Speed 
                    Mathf.Clamp01(GetMovementSpeedIncreasePercent() / 100f), // Movement Speed 
                    Mathf.Clamp01(GetRealTimeLuck() / 100f), // Luck 
                    Mathf.Clamp01(GetCriticalChancePercent() / 100f), // Critical Chance 
                    Mathf.Clamp01(GetEvadeChancePercent() / 100f) // Evade Chance 
                };
            }

            for (int i = 0; i < statBarImages.Length; i++)
            {
                if (statBarImages[i] == null)
                {
                    LogDebug($"WARNING: statBarImages[{i}] is null!");
                    continue;
                }

                if (statBarAnimations[i] != null)
                {
                    StopCoroutine(statBarAnimations[i]);
                }

                float targetValue = (i < statValues.Length) ? statValues[i] : 0f;

                statBarAnimations[i] = StartCoroutine(AnimateStatBar(statBarImages[i], targetValue));

                LogDebug($"Animating stat bar {i} to value: {targetValue} (percentage: {targetValue * 100f}%)");
            }
        }

        IEnumerator AnimateStatBar(Image statBar, float targetValue)
        {
            float duration = 1.0f;
            float elapsed = 0f;

            if (statBar == null)
            {
                LogDebug("Cannot animate: statBar is null");
                yield break;
            }

            statBar.fillAmount = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float smoothT = t * t * (3f - 2f * t);

                float newFill = Mathf.Lerp(0f, targetValue, smoothT);
                statBar.fillAmount = newFill;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            statBar.fillAmount = targetValue;
        }

        void LogDebug(string message, bool isWarning = false)
        {
            if (enableDebugLogs)
            {
                if (isWarning)
                {
                    Debug.LogWarning($"[StatsTabController] {message}");
                }
                else
                {
                    Debug.Log($"<color=#33AAFF>[StatsTabController] {message}</color>");
                }
            }
        }
    }
}