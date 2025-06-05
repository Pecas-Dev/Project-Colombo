using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using ProjectColombo.GameManagement.Events;

[RequireComponent(typeof(AudioSource))]
public class PickUpAudio : MonoBehaviour
{
    [Header("Pickup Sound Settings")]
    public AudioClip pickup2DSound;
    public AudioSource audioSource2D;

    [Header("Audio Settings")]
    public float maxDistance = 1f; // Distance at which audio is 0
    public float minDistance = 20f;  // Distance at which audio is full volume

    public AudioClip pickupSound;
    public AudioMixerGroup sfxMixerGroup;

    private AudioSource audioSource;
    private Transform player;

    private static List<PickUpAudio> activePickUps = new List<PickUpAudio>();

    void OnEnable()
    {
        activePickUps.Add(this);
        CustomEvents.OnCharmCollected += HandleCharmCollected;
    }

    void OnDisable()
    {
        activePickUps.Remove(this);
        CustomEvents.OnCharmCollected -= HandleCharmCollected;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        if (audioSource2D != null)
        {
            audioSource2D.spatialBlend = 0f;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null && pickupSound != null)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null) return;

        // Determine closest pickup to player
        PickUpAudio closest = null;
        float closestDist = float.MaxValue;

        foreach (var pickup in activePickUps)
        {
            if (pickup == null) continue;
            float dist = Vector3.Distance(pickup.transform.position, player.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = pickup;
            }
        }

        // Only closest plays with volume based on distance, others mute
        if (closest == this)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            float volume = Mathf.InverseLerp(maxDistance, minDistance, distance);
            audioSource.volume = Mathf.Clamp01(1f - volume);
        }
        else
        {
            audioSource.volume = 0f;
        }
    }

    private void HandleCharmCollected(GameObject charm)
    {
        if (charm == gameObject && pickup2DSound != null && audioSource2D != null)
        {
            audioSource2D.PlayOneShot(pickup2DSound);
        }
    }
}