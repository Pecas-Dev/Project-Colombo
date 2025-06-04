using UnityEngine;
using UnityEngine.Audio; // Required for AudioMixer
using ProjectColombo.Combat;
using static ProjectColombo.Combat.HealthManager;

public class MommottiSFX : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] walkSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] damagedSounds;
    public AudioClip[] deathSounds;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float walkVolume = 1f;
    [Range(0f, 1f)] public float attackVolume = 1f;
    [Range(0f, 1f)] public float damagedVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;

    [Header("Audio Mixer")]
    public AudioMixerGroup sfxMixerGroup; // Assign this in the Inspector

    private AudioSource walkAudioSource;
    private AudioSource attackAudioSource;
    private AudioSource damagedAudioSource;
    private AudioSource deathAudioSource;

    private HealthManager healthManager;
    private int lastHealth;

    private void Awake()
    {
        // Setup walk audio source (3D)
        walkAudioSource = GetComponent<AudioSource>();
        if (walkAudioSource == null)
            walkAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(walkAudioSource, 1f, sfxMixerGroup);

        attackAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(attackAudioSource, 0f, sfxMixerGroup);

        damagedAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(damagedAudioSource, 0f, sfxMixerGroup);

        deathAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(deathAudioSource, 0f, sfxMixerGroup);

        // HealthManager setup
        healthManager = GetComponent<HealthManager>();
        if (healthManager != null)
        {
            lastHealth = healthManager.CurrentHealth;
            healthManager.HealthChanged += OnHealthChanged;
        }
    }

    private void SetupAudioSource(AudioSource source, float spatialBlend, AudioMixerGroup mixerGroup)
    {
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.outputAudioMixerGroup = mixerGroup;

        if (spatialBlend > 0f)
        {
            source.minDistance = 1f;
            source.maxDistance = 20f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
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
    public void PlayDeathSFX()
    {
        PlayRandomClip(deathSounds, deathVolume, deathAudioSource);
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
