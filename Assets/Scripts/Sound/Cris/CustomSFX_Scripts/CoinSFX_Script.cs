using UnityEngine;

public class CoinSound : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip spawnSound;
    public AudioClip animationSound;

    [Header("Audio Settings")]
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    public void PlayAnimationSound()
    {
        if (audioSource != null && animationSound != null)
        {
            audioSource.PlayOneShot(animationSound);
        }
    }
}
