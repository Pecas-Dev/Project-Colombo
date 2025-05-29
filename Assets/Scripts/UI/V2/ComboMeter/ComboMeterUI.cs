using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProjectColombo.GameManagement.Events;


namespace ProjectColombo.UI.Combat
{
    public class ComboMeterUI : MonoBehaviour
    {
        [Header("OLD UI Components (HIDDEN)")]
        public Image oldFillCircle;
        public Image oldLevelIndicator;

        [Header("NEW UI Components - Vertical Meter")]
        public Image verticalMeterFill;
        public Image verticalLevelIndicator;
        public Image meterBackground;
        public CanvasGroup canvasGroup;

        [Header("Visual Settings")]
        public Color level0Color = Color.white;
        public Color[] levelColors = new Color[3];
        [SerializeField] float targetAlpha = 0.85f;

        [Header("Level Indicator Sprites")]
        public Sprite[] levelSprites = new Sprite[3];

        [Header("Animation Settings")]
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 1.0f;
        public float stayVisibleDuration = 3.0f;
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        Coroutine fadeCoroutine;
        Coroutine visibilityCoroutine;

        bool isVisible;
        int currentLevel;
        int currentPoints;
        int maxPoints = 100;

        void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        void InitializeUI()
        {
            canvasGroup.alpha = 0f;
            isVisible = false;

            HideOldCircleUI();

            if (verticalMeterFill != null)
            {
                verticalMeterFill.type = Image.Type.Filled;
                verticalMeterFill.fillMethod = Image.FillMethod.Vertical;
                verticalMeterFill.fillOrigin = 0;
                verticalMeterFill.fillAmount = 0f;
            }

            if (verticalLevelIndicator != null)
            {
                verticalLevelIndicator.gameObject.SetActive(false);
            }

            UpdateVisuals();
        }

        void HideOldCircleUI()
        {
            if (oldFillCircle != null)
            {
                oldFillCircle.gameObject.SetActive(false);
            }

            if (oldLevelIndicator != null)
            {
                oldLevelIndicator.gameObject.SetActive(false);
            }
        }

        void SubscribeToEvents()
        {
            CustomEvents.OnComboMeterLevelIncrease += OnComboMeterLevelChanged;
            CustomEvents.OnComboMeterLevelDecrease += OnComboMeterLevelChanged;
        }

        void UnsubscribeFromEvents()
        {
            CustomEvents.OnComboMeterLevelIncrease -= OnComboMeterLevelChanged;
            CustomEvents.OnComboMeterLevelDecrease -= OnComboMeterLevelChanged;
        }

        void OnComboMeterLevelChanged(int newLevel)
        {
            ShowUI();
        }

        public void UpdateComboMeter(int points, int level)
        {
            currentPoints = points;
            currentLevel = level;

            UpdateVisuals();
            ShowUI();
        }

        void UpdateVisuals()
        {
            float fillAmount = (float)currentPoints / maxPoints;

            if (verticalMeterFill != null)
            {
                verticalMeterFill.fillAmount = fillAmount;

                if (currentLevel == 0)
                {
                    verticalMeterFill.color = level0Color;
                }
                else if (currentLevel > 0 && currentLevel <= levelColors.Length)
                {
                    verticalMeterFill.color = levelColors[currentLevel - 1];
                }
            }

            UpdateLevelIndicator();
        }

        void UpdateLevelIndicator()
        {
            if (verticalLevelIndicator == null) return;

            if (currentLevel > 0)
            {
                verticalLevelIndicator.gameObject.SetActive(true);

                if (currentLevel <= levelSprites.Length && levelSprites[currentLevel - 1] != null)
                {
                    verticalLevelIndicator.sprite = levelSprites[currentLevel - 1];
                }

                if (currentLevel <= levelColors.Length)
                {
                    verticalLevelIndicator.color = levelColors[currentLevel - 1];
                }
            }
            else
            {
                verticalLevelIndicator.gameObject.SetActive(false);
            }
        }

        void ShowUI()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (visibilityCoroutine != null)
            {
                StopCoroutine(visibilityCoroutine);
            }

            if (!isVisible)
            {
                fadeCoroutine = StartCoroutine(FadeIn());
            }

            //visibilityCoroutine = StartCoroutine(VisibilityTimer());
        }

        void HideUI()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (isVisible)
            {
                fadeCoroutine = StartCoroutine(FadeOut());
            }
        }

        IEnumerator FadeIn()
        {
            isVisible = true;
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeInDuration;
                float curveValue = fadeInCurve.Evaluate(normalizedTime);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            fadeCoroutine = null;
        }

        IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeOutDuration;
                float curveValue = fadeOutCurve.Evaluate(normalizedTime);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            isVisible = false;
            fadeCoroutine = null;
        }

        //IEnumerator VisibilityTimer()
        //{
        //    yield return new WaitForSeconds(stayVisibleDuration);

        //    if (currentLevel == 0 && currentPoints == 0)
        //    {
        //        HideUI();
        //    }

        //    visibilityCoroutine = null;
        //}
    }
}