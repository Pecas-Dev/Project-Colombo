using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ProjectColombo.GameInputSystem;


namespace ProjectColombo.UI
{
    public abstract class MenuController : MonoBehaviour
    {
        [Header("Game Input")]
        [SerializeField] protected GameInputSO gameInputSO;

        [Header("Common Menu Properties")]
        [SerializeField] protected GameObject menuContainer;

        [Header("Common Animation Settings")]
        [SerializeField] protected float selectedMinFontSize = 75f;
        [SerializeField] protected float selectedMaxFontSize = 105f;
        [SerializeField] protected float defaultMinFontSize = 70f;
        [SerializeField] protected float defaultMaxFontSize = 100f;
        [SerializeField] protected float animationDuration = 0.3f;

        [Header("Button Click Animation Settings")]
        [SerializeField] protected float clickShrinkScale = 0.8f;
        [SerializeField] protected float clickShrinkDuration = 0.1f;
        [SerializeField] protected float clickBounceDuration = 0.2f;
        [SerializeField] protected AnimationCurve clickBounceCurve;

        [Header("Navigation Settings")]
        [SerializeField] protected float navigationDelay = 0.2f;
        [SerializeField] protected bool enableMenuNavigation = true;
        [SerializeField] protected bool wrapNavigation = true;

        protected Animator transitionAnimation;
        protected UIInputSwitcher uiInputSwitcher;

        protected Dictionary<RectTransform, Vector3> originalButtonScales = new Dictionary<RectTransform, Vector3>();

        protected float lastNavigationTime = 0f;
        protected bool isNavigating = false;

        protected virtual void Update()
        {
            if (gameObject.activeInHierarchy && enabled)
            {
                HandleInput();
            }
        }

        public virtual void Show()
        {
            menuContainer.SetActive(true);

            if (gameInputSO != null)
            {
                if (gameInputSO.inputActions == null)
                {
                    gameInputSO.Initialize();
                }

                gameInputSO.SwitchToUI();
            }
        }

        public virtual void Hide()
        {
            menuContainer.SetActive(false);
        }

        public virtual void Initialize()
        {
            GameObject transitionObject = GameObject.FindGameObjectWithTag("Transition");

            if (transitionObject != null)
            {
                transitionAnimation = transitionObject.GetComponent<Animator>();

                if (transitionAnimation == null)
                {
                    Debug.LogWarning("GameObject with tag 'Transition' doesn't have an Animator component.");
                }
            }

            if (FindFirstObjectByType<UIInputSwitcher>() == null)
            {
                GameObject uiInputSwitcherObject = new GameObject("UIInputSwitcher");
                uiInputSwitcherObject.AddComponent<UIInputSwitcher>();
            }

            if (clickBounceCurve == null || clickBounceCurve.keys.Length == 0)
            {
                Keyframe[] keys = new Keyframe[3];
                keys[0] = new Keyframe(0f, 0f, 0f, 2f);
                keys[1] = new Keyframe(0.5f, 1.1f, 0f, 0f);
                keys[2] = new Keyframe(1f, 1f, -1f, 0f);
                clickBounceCurve = new AnimationCurve(keys);
            }

            SetupButtonClickHandlers();

            if (enableMenuNavigation)
            {
                SetupButtonNavigation();
            }
        }

        public virtual void HandleInput()
        {
        }

        public virtual void Reinitialize()
        {
            GameObject transitionObject = GameObject.FindGameObjectWithTag("Transition");

            if (transitionObject != null)
            {
                transitionAnimation = transitionObject.GetComponent<Animator>();
            }

            SetupButtonClickHandlers();

            if (enableMenuNavigation)
            {
                SetupButtonNavigation();
            }
        }

        protected virtual void SetupButtonClickHandlers()
        {
            if (menuContainer == null) return;

            Button[] buttons = menuContainer.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();

                if (rectTransform != null && !originalButtonScales.ContainsKey(rectTransform))
                {
                    originalButtonScales[rectTransform] = rectTransform.localScale;
                }

                button.onClick.AddListener(() => PlayButtonClickAnimation(rectTransform));

                EventTrigger eventTrigger = button.GetComponent<EventTrigger>();

                if (eventTrigger == null)
                {
                    eventTrigger = button.gameObject.AddComponent<EventTrigger>();
                }

                bool hasSubmitTrigger = false;

                if (eventTrigger.triggers != null)
                {
                    foreach (EventTrigger.Entry entry in eventTrigger.triggers)
                    {
                        if (entry.eventID == EventTriggerType.Submit)
                        {
                            hasSubmitTrigger = true;
                            break;
                        }
                    }
                }

                if (!hasSubmitTrigger)
                {
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.Submit;
                    entry.callback.AddListener((data) => PlayButtonClickAnimation(rectTransform));
                    eventTrigger.triggers.Add(entry);
                }
            }
        }

        protected virtual void SetupButtonNavigation()
        {
            if (menuContainer == null) return;

            Button[] buttons = menuContainer.GetComponentsInChildren<Button>(true);
            if (buttons.Length == 0) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.Explicit;

                if (i > 0)
                {
                    nav.selectOnUp = buttons[i - 1];
                }
                else if (wrapNavigation)
                {
                    nav.selectOnUp = buttons[buttons.Length - 1];
                }

                if (i < buttons.Length - 1)
                {
                    nav.selectOnDown = buttons[i + 1];
                }
                else if (wrapNavigation)
                {
                    nav.selectOnDown = buttons[0];
                }

                button.navigation = nav;
            }

            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>();
            }

            if (eventSystem != null && buttons.Length > 0)
            {
                eventSystem.SetSelectedGameObject(buttons[0].gameObject);

                UIInputSwitcher inputSwitcher = FindFirstObjectByType<UIInputSwitcher>();

                if (inputSwitcher != null)
                {
                    inputSwitcher.SetFirstSelectedButton(buttons[0].gameObject);
                }
            }
        }

        protected bool CanNavigate()
        {
            if (Time.unscaledTime - lastNavigationTime < navigationDelay)
            {
                return false;
            }

            lastNavigationTime = Time.unscaledTime;
            return true;
        }

        protected void PlayButtonClickAnimation(RectTransform rectTransform)
        {
            if (rectTransform == null) return;

            Vector3 originalScale = Vector3.one;

            if (originalButtonScales.ContainsKey(rectTransform))
            {
                originalScale = originalButtonScales[rectTransform];
            }
            else
            {
                originalScale = rectTransform.localScale;
                originalButtonScales[rectTransform] = originalScale;
            }

            rectTransform.localScale = originalScale * clickShrinkScale;

            try
            {
                StartCoroutine(ShrinkAndBounceAnimation(rectTransform, originalScale));
            }
            catch (System.Exception)
            {
                UnityEngine.Application.onBeforeRender += () => ResetButtonScale(rectTransform, originalScale);
            }
        }

        void ResetButtonScale(RectTransform rectTransform, Vector3 originalScale)
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }

            UnityEngine.Application.onBeforeRender -= () => ResetButtonScale(rectTransform, originalScale);
        }

        protected IEnumerator ShrinkAndBounceAnimation(RectTransform rectTransform, Vector3 originalScale)
        {
            if (rectTransform == null)
            {
                yield break;
            }

            Vector3 targetScale = originalScale * clickShrinkScale;

            rectTransform.localScale = targetScale;

            yield return new WaitForEndOfFrame();

            if (!gameObject.activeInHierarchy || rectTransform == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < clickBounceDuration && gameObject.activeInHierarchy && rectTransform != null)
            {
                float t = elapsedTime / clickBounceDuration;
                float curveValue = clickBounceCurve.Evaluate(t);

                Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, curveValue);
                rectTransform.localScale = currentScale;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;

                if (!gameObject.activeInHierarchy || rectTransform == null)
                {
                    yield break;
                }
            }

            if (gameObject.activeInHierarchy && rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }
        }

        protected IEnumerator AnimateTextSize(TextMeshProUGUI text, float startMin, float startMax, float endMin, float endMax, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float smoothT = t * t * (3f - 2f * t);
                float currentMin = Mathf.Lerp(startMin, endMin, smoothT);
                float currentMax = Mathf.Lerp(startMax, endMax, smoothT);

                text.enableAutoSizing = true;
                text.fontSizeMin = currentMin;
                text.fontSizeMax = currentMax;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            text.fontSizeMin = endMin;
            text.fontSizeMax = endMax;
        }

        protected void SetTextToDefaultSize(TextMeshProUGUI text)
        {
            if (text != null)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = defaultMinFontSize;
                text.fontSizeMax = defaultMaxFontSize;
            }
        }
    }
}