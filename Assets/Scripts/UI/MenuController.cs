using TMPro;
using UnityEngine;
using System.Collections;
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


        protected Animator transitionAnimation;


        public virtual void Show()
        {
            menuContainer.SetActive(true);

            if (gameInputSO != null)
            {
                if (gameInputSO.playerInputActions == null)
                {
                    gameInputSO.Initialize();
                }

                gameInputSO.EnableUIMode();
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
        }

        public virtual void HandleInput()
        {
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

                elapsedTime += Time.deltaTime;
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