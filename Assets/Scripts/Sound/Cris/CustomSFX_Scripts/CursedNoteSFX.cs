using UnityEngine;

public class VFXSoundOnSpawn : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private void OnEnable()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}