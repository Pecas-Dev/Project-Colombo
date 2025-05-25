using System.Collections.Generic;
using UnityEngine;

public class DropSFXManager : MonoBehaviour
{
    public AudioClip commonClip;
    public AudioClip rareClip;
    public AudioClip legendaryClip;

    private AudioSource audioSource;
    public DropRarityTag rarityTag;

    private static List<DropSFXManager> activeDrops = new List<DropSFXManager>();

    private Rarity dropRarity;

    private void Awake()
    {
        var rarityTag = GetComponentInChildren<DropRarityTag>();
        if (rarityTag != null)
        {
            dropRarity = rarityTag.rarity;
        }
        else
        {
            dropRarity = Rarity.Common;
            Debug.LogWarning("DropRarityTag not found on child prefab, defaulting to Common");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = GetClipForRarity(dropRarity);
        audioSource.Play();

        activeDrops.Add(this);
        UpdateAllDropVolumes();
    }

    private void OnDestroy()
    {
        activeDrops.Remove(this);
        UpdateAllDropVolumes();
    }

    private AudioClip GetClipForRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonClip,
            Rarity.Rare => rareClip,
            Rarity.Legendary => legendaryClip,
            _ => commonClip
        };
    }

    private static void UpdateAllDropVolumes()
    {
        if (activeDrops.Count == 0) return;

        DropSFXManager highest = null;
        foreach (var drop in activeDrops)
        {
            if (highest == null || drop.dropRarity > highest.dropRarity)
                highest = drop;
        }

        foreach (var drop in activeDrops)
        {
            if (drop.audioSource != null)
                drop.audioSource.mute = drop != highest;
        }
    }
}