using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LineArtImages : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] Sprite[] lineArtSprites;

    [Header("Animation Settings")]
    [SerializeField] float growDuration = 10f;
    [SerializeField] float fadeInDuration = 2.5f;
    [SerializeField] float targetAlpha = 0.3f;
    [SerializeField] float transitionOverlapPercent = 0.25f;

    [Header("Transition Settings")]
    [SerializeField] AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] float fadeOutStartPoint = 0.7f;

    [Header("Size Settings")]
    [SerializeField] Vector2 initialSize = new Vector2(965f, 965f);
    [SerializeField] Vector2 targetSize = new Vector2(1050f, 1050f);

    int currentIndex = 0;
    private bool isTransitioning = false;

    Image targetImage;
    RectTransform imageRectTransform;

    Coroutine animationCoroutine;

    void Start()
    {
        targetImage = GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.LogError("Target Image not assigned!");
            return;
        }

        imageRectTransform = targetImage.GetComponent<RectTransform>();

        Color color = targetImage.color;
        color.a = 0f;
        targetImage.color = color;

        imageRectTransform.sizeDelta = initialSize;

        if (lineArtSprites.Length > 0)
        {
            StartAnimationLoop();
        }
        else
        {
            Debug.LogWarning("No sprites assigned to the array!");
        }
    }

    void StartAnimationLoop()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateSprites());
    }

    IEnumerator AnimateSprites()
    {
        while (true)
        {
            if (lineArtSprites.Length == 0) yield break;

            isTransitioning = false;

            Sprite currentSprite = lineArtSprites[currentIndex];
            int nextIndex = (currentIndex + 1) % lineArtSprites.Length;

            if (currentSprite != null)
            {
                targetImage.sprite = currentSprite;
                imageRectTransform.sizeDelta = initialSize;

                yield return StartCoroutine(AnimateWithOverlap(nextIndex));

                currentIndex = nextIndex;
            }
            else
            {
                currentIndex = nextIndex;
                yield return null;
            }
        }
    }

    IEnumerator AnimateWithOverlap(int nextIndex)
    {
        float elapsedTime = 0f;
        float growthProgress = 0f;

        float fadeOutDuration = growDuration * (1f - fadeOutStartPoint);
        float overlapStartTime = growDuration - (fadeOutDuration * transitionOverlapPercent);

        Color color = targetImage.color;
        color.a = 0f;
        targetImage.color = color;

        while (growthProgress < 1f)
        {
            elapsedTime += Time.deltaTime;
            growthProgress = Mathf.Min(elapsedTime / growDuration, 1f);

            float easedGrowth = growCurve.Evaluate(growthProgress);
            imageRectTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, easedGrowth);

            if (growthProgress < fadeInDuration / growDuration)
            {
                float fadeInProgress = growthProgress / (fadeInDuration / growDuration);
                color.a = fadeInCurve.Evaluate(fadeInProgress) * targetAlpha;
            }
            else if (growthProgress < fadeOutStartPoint)
            {
                color.a = targetAlpha;
            }
            else
            {
                float fadeOutProgress = (growthProgress - fadeOutStartPoint) / (1f - fadeOutStartPoint);
                color.a = Mathf.Lerp(targetAlpha, 0f, fadeOutCurve.Evaluate(fadeOutProgress));
            }

            targetImage.color = color;

            if (elapsedTime >= overlapStartTime && !isTransitioning && lineArtSprites[nextIndex] != null)
            {
                isTransitioning = true;
                StartCoroutine(PrepareNextImage(nextIndex));
            }

            if (growthProgress > fadeOutStartPoint && color.a <= 0.01f)
            {
                break;
            }

            yield return null;
        }

        color.a = 0f;
        targetImage.color = color;
        imageRectTransform.sizeDelta = initialSize;
    }

    IEnumerator PrepareNextImage(int nextIndex)
    {
        yield return new WaitForSeconds(0.1f);

        targetImage.sprite = lineArtSprites[nextIndex];

        imageRectTransform.sizeDelta = initialSize;

        float fadeInTime = 0f;
        Color color = targetImage.color;

        while (fadeInTime < fadeInDuration)
        {
            fadeInTime += Time.deltaTime;
            float fadeInProgress = fadeInTime / fadeInDuration;

            color.a = fadeInCurve.Evaluate(fadeInProgress) * targetAlpha;
            targetImage.color = color;

            yield return null;
        }

        color.a = targetAlpha;
        targetImage.color = color;
    }

    public void RestartAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        Color color = targetImage.color;
        color.a = 0f;

        targetImage.color = color;
        imageRectTransform.sizeDelta = initialSize;

        currentIndex = 0;
        isTransitioning = false;
        StartAnimationLoop();
    }
}