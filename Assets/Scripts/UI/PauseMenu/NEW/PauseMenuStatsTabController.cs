using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace ProjectColombo.UI.Pausescreen
{
    public class PauseMenuStatsTabController : MonoBehaviour
    {
        [Header("Stats UI References")]
        [SerializeField] Image[] statBarImages;
        [SerializeField] TextMeshProUGUI[] statValueTexts;

        [Header("Graph References")]
        [SerializeField] GameObject graphLines;

        [Header("Debug Settings")]
        [SerializeField] bool enableDebugLogs = true;

        float healthStat = 0.75f;
        float attackStat = 0.60f;
        float defenseStat = 0.45f;
        float speedStat = 0.85f;

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

            if (statValueTexts != null)
            {
                for (int i = 0; i < statValueTexts.Length; i++)
                {
                    if (statValueTexts[i] != null)
                    {
                        switch (i)
                        {
                            case 0:
                                statValueTexts[i].text = Mathf.RoundToInt(healthStat * 100f) + "%";
                                break;
                            case 1:
                                statValueTexts[i].text = Mathf.RoundToInt(attackStat * 100f).ToString();
                                break;
                            case 2:
                                statValueTexts[i].text = Mathf.RoundToInt(defenseStat * 100f).ToString();
                                break;
                            case 3:
                                statValueTexts[i].text = Mathf.RoundToInt(speedStat * 100f).ToString();
                                break;
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


            float[] statValues = new float[] { healthStat, attackStat, defenseStat, speedStat };

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

        void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=#33AAFF>[StatsTabController] {message}</color>");
            }
        }
    }
}