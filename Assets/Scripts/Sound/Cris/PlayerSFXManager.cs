using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    //      [STEPS]
    public AudioClip[] footstepSounds;
    public AudioClip[] churchFootstepSounds;

    //      [ATTACKS]
    public AudioClip[] minorStab1Sounds;
    public AudioClip[] churchMinorStab1Sounds;
    public AudioClip[] minorStab2Sounds;
    public AudioClip[] churchMinorStab2Sounds;
    public AudioClip[] minorStab3Sounds;
    public AudioClip[] churchMinorStab3Sounds;

    public AudioClip[] majorSlash1Sounds;
    public AudioClip[] churchMajorSlash1Sounds;
    public AudioClip[] majorSlash2Sounds;
    public AudioClip[] churchMajorSlash2Sounds;
    public AudioClip[] majorSlash3Sounds;
    public AudioClip[] churchMajorSlash3Sounds;

    //      [BLOCK]
    public AudioClip[] blockExecutionSounds;
    public AudioClip[] churchBlockExecutionSounds;

    //      [PARRY]
    public AudioClip[] parryExecutionSounds;
    public AudioClip[] churchParryExecutionSounds;

    //      [ROLL]  
    public AudioClip[] rollSounds;
    public AudioClip[] churchRollSounds;
  
    public AudioSource audioSource;

    private bool isInChurch = false; // Set this based on your scene or other conditions

    // All the following functions randomize and play one of the sounds for the chosen action.
    // They all count in also the church variations

    public void PlayFootstep()
    {
        AudioClip selectedClip = isInChurch ?
            churchFootstepSounds[Random.Range(0, churchFootstepSounds.Length)] :
            footstepSounds[Random.Range(0, footstepSounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMinorStab1()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab1Sounds[Random.Range(0, churchMinorStab1Sounds.Length)] :
            minorStab1Sounds[Random.Range(0, minorStab1Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMinorStab2()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab2Sounds[Random.Range(0, churchMinorStab2Sounds.Length)] :
            minorStab2Sounds[Random.Range(0, minorStab2Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMinorStab3()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab3Sounds[Random.Range(0, churchMinorStab3Sounds.Length)] :
            minorStab3Sounds[Random.Range(0, minorStab3Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMajorSlash1()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash1Sounds[Random.Range(0, churchMajorSlash1Sounds.Length)] :
            majorSlash1Sounds[Random.Range(0, majorSlash1Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMajorSlash2()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash2Sounds[Random.Range(0, churchMajorSlash2Sounds.Length)] :
            majorSlash2Sounds[Random.Range(0, majorSlash2Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayMajorSlash3()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash3Sounds[Random.Range(0, churchMajorSlash3Sounds.Length)] :
            majorSlash3Sounds[Random.Range(0, majorSlash3Sounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayBlockExecution()
    {
        AudioClip selectedClip = isInChurch ?
            churchBlockExecutionSounds[Random.Range(0, churchBlockExecutionSounds.Length)] :
            blockExecutionSounds[Random.Range(0, blockExecutionSounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayParryExecution()
    {
        AudioClip selectedClip = isInChurch ?
            churchParryExecutionSounds[Random.Range(0, churchParryExecutionSounds.Length)] :
            parryExecutionSounds[Random.Range(0, parryExecutionSounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }

    public void PlayRoll()
    {
        AudioClip selectedClip = isInChurch ?
            churchRollSounds[Random.Range(0, churchRollSounds.Length)] :
            rollSounds[Random.Range(0, rollSounds.Length)];

        audioSource.PlayOneShot(selectedClip);
    }
}
