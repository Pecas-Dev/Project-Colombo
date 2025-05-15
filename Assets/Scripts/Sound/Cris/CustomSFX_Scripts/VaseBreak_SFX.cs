using UnityEngine;

namespace ProjectColombo.Objects.SFX
{
    public class VaseBreak_SFX : MonoBehaviour
    {
        public AudioClip sfxClip;

        [Range(0f, 1f)]
        public float volume = 1f;

        private static AudioSource sfxAudioSource;

        public void PlaySFX()
        {
            if (sfxClip == null)
            {
                Debug.LogWarning("No SFX clip assigned to PlaySFXOnDestroy on " + gameObject.name);
                return;
            }

            if (sfxAudioSource == null)
            {
                GameObject sfxObj = new GameObject("SFXAudioSource");
                sfxAudioSource = sfxObj.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
                DontDestroyOnLoad(sfxObj);
            }

            if (!sfxAudioSource.isPlaying)
            {
                sfxAudioSource.clip = sfxClip;
                sfxAudioSource.volume = volume;
                sfxAudioSource.spatialBlend = 0f;
                sfxAudioSource.Play();
            }
        }
    }
}
