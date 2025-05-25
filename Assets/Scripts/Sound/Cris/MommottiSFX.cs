using UnityEngine;
using ProjectColombo.Combat;
using static ProjectColombo.Combat.HealthManager;

public class MommottiSFX : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] walkSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] damagedSounds;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float walkVolume = 1f;
    [Range(0f, 1f)] public float attackVolume = 1f;
    [Range(0f, 1f)] public float damagedVolume = 1f;

    private AudioSource walkAudioSource;
    private AudioSource attackAudioSource;
    private AudioSource damagedAudioSource;

    private HealthManager healthManager;
    private int lastHealth;

    private void Awake()
    {
        // Setup walk audio source (3D)
        walkAudioSource = GetComponent<AudioSource>();
        if (walkAudioSource == null)
            walkAudioSource = gameObject.AddComponent<AudioSource>();

        walkAudioSource.playOnAwake = false;
        walkAudioSource.spatialBlend = 1f;
        walkAudioSource.minDistance = 1f;
        walkAudioSource.maxDistance = 20f;
        walkAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        // Setup attack audio source (2D)
        attackAudioSource = gameObject.AddComponent<AudioSource>();
        attackAudioSource.playOnAwake = false;
        attackAudioSource.spatialBlend = 0f; // full 2D sound

        // Setup damaged audio source (2D) <-- NEW
        damagedAudioSource = gameObject.AddComponent<AudioSource>();
        damagedAudioSource.playOnAwake = false;
        damagedAudioSource.spatialBlend = 0f;

        healthManager = GetComponent<HealthManager>();
        if (healthManager != null)
        {
            lastHealth = healthManager.CurrentHealth;
            healthManager.HealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < lastHealth)
        {
            PlayDamagedSFX();
        }

        lastHealth = current;
    }

    public void PlayWalkSFX()
    {
        PlayRandomClip(walkSounds, walkVolume, walkAudioSource);
    }

    public void PlayAttackSFX()
    {
        PlayRandomClip(attackSounds, attackVolume, attackAudioSource);
    }

    public void PlayDamagedSFX()
    {
        PlayRandomClip(damagedSounds, damagedVolume, damagedAudioSource);
    }

    private void PlayRandomClip(AudioClip[] clips, float maxVolume, AudioSource source)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"No audio clips assigned on {gameObject.name}");
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        source.PlayOneShot(clip, maxVolume);
    }

    private void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.HealthChanged -= OnHealthChanged;
        }
    }
}