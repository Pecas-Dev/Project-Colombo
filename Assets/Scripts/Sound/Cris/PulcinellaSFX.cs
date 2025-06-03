using UnityEngine;

public class PulcinellaSFX : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource;

    [Header("Cursed Note Execution")]
    public AudioClip cursedNoteExecutionClip;
    [Range(0f, 1f)]
    public float cursedNoteExecutionVolume = 1f;

    [Header("Cursed Note Charging")]
    public AudioClip cursedNoteChargingClip;
    [Range(0f, 1f)]
    public float cursedNoteChargingVolume = 1f;

    [Header("Slash Sounds")]
    public AudioClip[] slashClips;
    [Range(0f, 1f)]
    public float slashVolume = 1f;

    public void PlayCursedNoteExecution()
    {
        PlaySound(cursedNoteExecutionClip, cursedNoteExecutionVolume);
    }

    public void PlayCursedNoteCharging()
    {
        PlaySound(cursedNoteChargingClip, cursedNoteChargingVolume);
    }

    public void PlaySlash()
    {
        if (slashClips.Length == 0) return;
        AudioClip selectedClip = slashClips[Random.Range(0, slashClips.Length)];
        PlaySound(selectedClip, slashVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;

        audioSource.PlayOneShot(clip, volume);
    }
}
