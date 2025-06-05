using System.Collections;
using UnityEngine;

namespace ProjectColombo.Sound
{
    public class VolumeFade : MonoBehaviour
    {
        public AudioManager audioManager;
        public float fadeDuration = 1f;
        float initialVolume;

        private void Start()
        {
            audioManager = FindFirstObjectByType<AudioManager>();

            if (audioManager == null)
            {
                Debug.LogError("AudioManager reference not set on VolumeFade.");
                enabled = false;
                return;
            }

            initialVolume = audioManager.masterVolume;
        }

        public void StartFadeIn()
        {
            Debug.Log("Fade Volume In");
            StopAllCoroutines();
            StartCoroutine(FadeVolume(audioManager.masterVolume, initialVolume));
        }

        public void StartFadeOut()
        {
            Debug.Log("Fade Volume Out");
            StopAllCoroutines();
            StartCoroutine(FadeVolume(audioManager.masterVolume, 0f));
        }

        private IEnumerator FadeVolume(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                audioManager.masterVolume = Mathf.Lerp(from, to, t);
                yield return null;
            }

            audioManager.masterVolume = to;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
