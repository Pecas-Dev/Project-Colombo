using UnityEngine;

public class ChestSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chestOpenClip;

    // Public method to be called from animation event
    public void PlayChestOpenSFX()
    {
        if (audioSource != null && chestOpenClip != null)
        {
            audioSource.PlayOneShot(chestOpenClip);
        }
        else
        {
            Debug.LogWarning("Missing AudioSource or ChestOpenClip on " + gameObject.name);
        }
    }
}