using UnityEngine;

namespace ProjectColombo.Objects.SFX
{
    public class VaseBreak_SFX : MonoBehaviour
    {
        public AudioClip sfxClip;

        [Range(0f, 1f)]
        public float volume = 1f;

        private static AudioSource sfxAudioSource;

        private static float nextPlayTime = 0f;

        public void PlaySFX()
        {
            if (sfxClip == null)
            {
                Debug.LogWarning("No SFX clip assigned to PlaySFXOnDestroy on " + gameObject.name);
                return;
            }

            if (Time.time >= nextPlayTime)
            {
                if (sfxAudioSource == null)
                {
                    GameObject sfxObj = new GameObject("SFXAudioSource");
                    sfxAudioSource = sfxObj.AddComponent<AudioSource>();
                    sfxAudioSource.playOnAwake = false;
                    DontDestroyOnLoad(sfxObj);
                }

                sfxAudioSource.clip = sfxClip;
                sfxAudioSource.volume = volume;
                sfxAudioSource.spatialBlend = 0f;
                sfxAudioSource.Play();

                nextPlayTime = Time.time + 0.4f;
            }
        }
    }
}
