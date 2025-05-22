using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectColombo.GameManagement;
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

        [Header("Global Stats Reference")]
        [SerializeField] GlobalStats globalStats;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        [Header("Missing Stats (Manual Override)")]
        [SerializeField] float criticalChancePercent = 15f;
        [SerializeField] float evadeChancePercent = 25f;

        Coroutine[] statBarAnimations;

        bool isInitialized = false;

        void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            UpdateStats();
            AnimateStatBars();

            LogDebug("Stats tab enabled");
        }

        public void Initialize()
        {
            if (globalStats == null)
            {
                if (GameManager.Instance != null)
                {
                    globalStats = GameManager.Instance.GetComponent<GlobalStats>();
                    LogDebug("Found GlobalStats from GameManager");
                }

                if (globalStats == null)
                {
                    globalStats = FindFirstObjectByType<GlobalStats>();
                    LogDebug("Found GlobalStats in scene");
                }
            }

            if (statBarImages != null)
            {
                statBarAnimations = new Coroutine[statBarImages.Length];
            }
            else
            {
                LogDebug("WARNING: statBarImages array is null!");
            }

            isInitialized = true;
        }

        public void UpdateStats()
        {
            if (globalStats == null)
            {
                LogDebug("WARNING: GlobalStats reference is missing!", true);
                return;
            }

            if (statValueTexts != null)
            {
                for (int i = 0; i < statValueTexts.Length; i++)
                {
                    if (statValueTexts[i] != null)
                    {
                        switch (i)
                        {
                            case 0: // Damage (using Major Damage Multiplier)
                                float damageValue = globalStats.currentMajorDamageMultiplyer * 100f;
                                statValueTexts[i].text = "Damage (" + Mathf.RoundToInt(damageValue).ToString() + ")";
                                LogDebug($"Updated Damage: {damageValue}%");
                                break;
                            case 1: // Defense (using Block Reduction Percent)
                                float defenseValue = globalStats.currentBlockReductionPercent;
                                statValueTexts[i].text = "Defense (" + Mathf.RoundToInt(defenseValue) + "%)";
                                LogDebug($"Updated Defense: {defenseValue}%");
                                break;
                            case 2: // Attack Speed
                                float attackSpeedValue = globalStats.currentPlayerAttackSpeed * 100f;
                                statValueTexts[i].text = "Attack Speed (" + Mathf.RoundToInt(attackSpeedValue).ToString() + ")";
                                LogDebug($"Updated Attack Speed: {attackSpeedValue}");
                                break;
                            case 3: // Movement Speed
                                float movementSpeedValue = globalStats.currentPlayerSpeed;
                                statValueTexts[i].text = "Move Speed (" + movementSpeedValue.ToString("F1") + ")";
                                LogDebug($"Updated Movement Speed: {movementSpeedValue}");
                                break;
                            case 4: // Luck
                                int luckValue = globalStats.currentLuckPoints;
                                statValueTexts[i].text = "Luck (" + luckValue.ToString() + ")";
                                LogDebug($"Updated Luck: {luckValue}");
                                break;
                                //case 5: // Critical Chance 
                                //    statValueTexts[i].text = Mathf.RoundToInt(criticalChancePercent) + "%";
                                //    LogDebug($"Updated Critical Chance (placeholder): {criticalChancePercent}%");
                                //    break;
                                //case 6: // Evade Chance 
                                //    statValueTexts[i].text = Mathf.RoundToInt(evadeChancePercent) + "%";
                                //    LogDebug($"Updated Evade Chance (placeholder): {evadeChancePercent}%");
                                //    break;
                        }
                    }
                    else
                    {
                        LogDebug($"WARNING: statValueTexts[{i}] is null!");
                    }
                }
            }
            else
            {
                LogDebug("WARNING: statValueTexts array is null!");
            }
        }

        void AnimateStatBars()
        {
            if (statBarImages == null)
            {
                LogDebug("Cannot animate stat bars: statBarImages array is null");
                return;
            }

            if (globalStats == null)
            {
                LogDebug("Cannot animate stat bars: GlobalStats reference is missing");
                return;
            }

            float[] statValues = new float[]
            {
                Mathf.Clamp01(globalStats.currentMajorDamageMultiplyer), // Damage
                Mathf.Clamp01(globalStats.currentBlockReductionPercent / 100f), // Defense
                Mathf.Clamp01(globalStats.currentPlayerAttackSpeed), // Attack Speed
                Mathf.Clamp01(globalStats.currentPlayerSpeed / 10f), // Movement Speed (normalized by dividing by 10)
                Mathf.Clamp01(globalStats.currentLuckPoints / 20f), // Luck (normalized by dividing by 20, assuming max 20)
                //Mathf.Clamp01(criticalChancePercent / 100f), // Critical Chance
                //Mathf.Clamp01(evadeChancePercent / 100f) // Evade Chance
            };

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