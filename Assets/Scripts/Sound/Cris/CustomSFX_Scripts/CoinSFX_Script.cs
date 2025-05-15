using UnityEngine;

public class CoinSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip spawnSound;
    public AudioClip animationSound;

    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Range(0f, 1f)]
    public float volume = 1f;

    void Start()
    {
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound, volume);
        }
    }

    public void PlayAnimationSound()
    {
        if (audioSource != null && animationSound != null)
        {
            audioSource.PlayOneShot(animationSound, volume);
        }
    }
}