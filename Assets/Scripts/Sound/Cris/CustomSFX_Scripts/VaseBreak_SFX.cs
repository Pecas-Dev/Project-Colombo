using UnityEngine;
using UnityEngine.Audio;

namespace ProjectColombo.Objects.SFX
{
    public class VaseBreak_SFX : MonoBehaviour
    {
        public AudioClip sfxClip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Audio Mixer")]
        public AudioMixerGroup sfxMixerGroup;

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
                    sfxAudioSource.spatialBlend = 0f;
                    DontDestroyOnLoad(sfxObj);
                }

                if (sfxAudioSource.outputAudioMixerGroup != sfxMixerGroup)
                {
                    sfxAudioSource.outputAudioMixerGroup = sfxMixerGroup;
                }

                sfxAudioSource.clip = sfxClip;
                sfxAudioSource.volume = volume;
                sfxAudioSource.Play();

                nextPlayTime = Time.time + 0.4f;
            }
        }
    }
}