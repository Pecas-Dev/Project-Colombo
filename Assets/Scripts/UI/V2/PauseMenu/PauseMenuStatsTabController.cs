using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Charms;
using ProjectColombo.Objects.Masks;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] GlobalStats myGlobalStats;
        [SerializeField] PlayerInventory myPlayerInventory;


        [Header("Graph Baseline Settings")]
        [SerializeField] float statBarBaseline = 0f;
        [SerializeField] float statBarMaxRange = 100f;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        Coroutine[] statBarAnimations;
        bool isInitialized = false;

        // Stat values
        [ReadOnlyInspector] public float currentMajorAttackPercentage = 0;
        [ReadOnlyInspector] public float currentMinorAttackPercentage = 0;
        [ReadOnlyInspector] public float currentIncomingDamagePercentage = 0;
        [ReadOnlyInspector] public float currentMoveSpeedPercentage = 0;
        [ReadOnlyInspector] public float currentAttackSpeedPercentage = 0;
        [ReadOnlyInspector] public float currentCritChancePercentage = 0;
        [ReadOnlyInspector] public float currentEvadeChancePercentage = 0;
        [ReadOnlyInspector] public float currentLuckPoints = 0;

        void Awake()
        {
            if (!isInitialized)
                Initialize();
        }

        void OnEnable()
        {
            UpdateStats();
            AnimateStatBars();
            LogDebug("Stats tab enabled with real-time stats");
        }

        public void Initialize()
        {
            if (GameManager.Instance != null)
            {
                myGlobalStats = GameManager.Instance.GetComponent<GlobalStats>();
                myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
                LogDebug("Found GameManager components");
            }

            statBarAnimations = new Coroutine[statBarImages.Length];
            isInitialized = true;
        }

        public void ResetStatNumbers()
        {
            currentMajorAttackPercentage = 100;
            currentMinorAttackPercentage = 100;
            currentIncomingDamagePercentage = 100;
            currentMoveSpeedPercentage = 100;
            currentAttackSpeedPercentage = 100;
            currentLuckPoints = 0;
            currentCritChancePercentage = 0;
            currentEvadeChancePercentage = 0;
        }

        public void UpdateStatNumbers()
        {
            ResetStatNumbers();

            foreach (var charm in myPlayerInventory.charms)
            {
                AttributesStatSheet stats = charm.GetComponent<BaseCharm>().GetStats();
                AddStats(stats);
            }

            foreach (var charm in myPlayerInventory.legendaryCharms)
            {
                AttributesStatSheet stats = charm.GetComponent<BaseCharm>().GetStats();
                AddStats(stats);
            }

            if (myPlayerInventory.maskSlot.transform.childCount != 0)
            {
                var stats = myPlayerInventory.maskSlot.GetComponentInChildren<BaseMask>().GetStats();
                AddStats(stats);
            }

            currentLuckPoints = myPlayerInventory.currentLuck;
            currentCritChancePercentage = Mathf.RoundToInt(currentLuckPoints / 2f);
            currentEvadeChancePercentage += Mathf.RoundToInt(currentLuckPoints / 5f);
        }

        void AddStats(AttributesStatSheet stats)
        {
            currentMajorAttackPercentage += stats.majorDamagePercentage;
            currentMinorAttackPercentage += stats.minorDamagePercentage;
            currentIncomingDamagePercentage -= ((stats.incomingMajorDamagePercentage + stats.incomingMinorDamagePercentage)/2);
            currentMoveSpeedPercentage += stats.moveSpeedPercentage;
            currentAttackSpeedPercentage += stats.attackSpeedPercentage;
            currentEvadeChancePercentage += stats.evadeChancePercentage;
        }

        public void UpdateStats()
        {
            LogDebug("Updating stats with real-time values");

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                ResetStatNumbers();
            }
            else
            {
                UpdateStatNumbers();
            }

            if (statValueTexts == null || statValueTexts.Length < 8)
            {
                LogDebug("WARNING: statValueTexts array is null or insufficient!", true);
                return;
            }

            statValueTexts[0].text = $"Dur Damage ({Mathf.RoundToInt(currentMajorAttackPercentage)}%)";
            statValueTexts[1].text = $"Moll Damage ({Mathf.RoundToInt(currentMinorAttackPercentage)}%)";
            statValueTexts[2].text = $"Resistance ({Mathf.RoundToInt(currentIncomingDamagePercentage)}%)";
            statValueTexts[3].text = $"Move Speed ({Mathf.RoundToInt(currentMoveSpeedPercentage)}%)";
            statValueTexts[4].text = $"Attack Speed ({Mathf.RoundToInt(currentAttackSpeedPercentage)}%)";
            statValueTexts[5].text = $"Crit Chance ({Mathf.RoundToInt(currentCritChancePercentage)}%)";
            statValueTexts[6].text = $"Evade Chance ({Mathf.RoundToInt(currentEvadeChancePercentage)}%)";
            statValueTexts[7].text = $"Luck ({Mathf.RoundToInt(currentLuckPoints)}%)";
        }

        void AnimateStatBars()
        {
            if (statBarImages == null)
            {
                LogDebug("Cannot animate stat bars: statBarImages is null");
                return;
            }

            float[] statValues = new float[]
            {
                currentMajorAttackPercentage,
                currentMinorAttackPercentage,
                currentIncomingDamagePercentage,
                currentMoveSpeedPercentage,
                currentAttackSpeedPercentage,
                currentCritChancePercentage,
                currentEvadeChancePercentage,
                currentLuckPoints
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

                float rawValue = i < statValues.Length ? statValues[i] : 0f;
                float normalized;

                if (i >= 5) //so crit, evade, luck
                {
                    normalized = Mathf.Clamp01(rawValue / 100f); // 100% = full fill
                }
                else
                {
                    normalized = Mathf.Clamp01(rawValue / 200f); // 100% = 0.5 fill, 200% = full fill
                }

                statBarAnimations[i] = StartCoroutine(AnimateStatBar(statBarImages[i], normalized));
                LogDebug($"Animating stat bar {i}: raw={rawValue}, normalized={normalized}");
            }
        }


        IEnumerator AnimateStatBar(Image statBar, float targetValue)
        {
            float duration = 1.0f;
            float elapsed = 0f;

            float initialValue = statBar.fillAmount;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float smoothT = t * t * (3f - 2f * t);
                statBar.fillAmount = Mathf.Lerp(initialValue, targetValue, smoothT);

                //maybe but then carefull with crit, evade and luck
                //if (statBar.fillAmount < 0.5f)
                //{
                //    statBar.color = Color.red;
                //}
                //else
                //{
                //    statBar.color = Color.black;
                //}

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            statBar.fillAmount = targetValue;
        }

        void LogDebug(string message, bool isWarning = false)
        {
            if (!enableDebugLogs) return;
            if (isWarning)
                Debug.LogWarning($"[StatsTabController] {message}");
            else
                Debug.Log($"<color=#33AAFF>[StatsTabController] {message}</color>");
        }
    }
}
