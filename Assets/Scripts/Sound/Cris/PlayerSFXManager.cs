using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    public AudioClip[] footstepSounds; // 4 sounds for walking
    public AudioClip[] churchFootstepSounds; // 4 church-specific footstep sounds
    public AudioSource audioSource;

    private bool isInChurch = false; // Set this based on your scene or other conditions

    // This method will be called from the animation event
    public void PlayFootstep()
    {
        // Choose a random sound based on the scene
        AudioClip selectedClip = isInChurch ? churchFootstepSounds[Random.Range(0, churchFootstepSounds.Length)]
                                            : footstepSounds[Random.Range(0, footstepSounds.Length)];

        // Play the sound
        audioSource.PlayOneShot(selectedClip);
    }
}
