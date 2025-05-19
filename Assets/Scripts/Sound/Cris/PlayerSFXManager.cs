using System.Collections.Generic;
using UnityEngine;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Combat;
using ProjectColombo;
using System;

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

    //      [COMBO SYSTEM]
    [Header("1st Attack Sounds")]
    public List<AudioClip> firstAttackClips;

    [System.Serializable]
    public class SecondAttackGroup
    {
        public string note;
        public List<AudioClip> clips;
    }

    [Header("2nd Attack Sounds")]
    public List<SecondAttackGroup> secondAttackClips;

    [System.Serializable]
    public class ThirdAttack
    {
        public string label;
        public List<AudioClip> clips;
    }

    [System.Serializable]
    public class ThirdComboGroup
    {
        public string groupLabel; // FirstNote

        public List<ThirdAttackGroup> attackGroups;
    }

    // Nested structure for sound groupings within the third attack
    [System.Serializable]
    public class ThirdAttackGroup
    {
        public string attackType; //SecondNote
        public List<ThirdAttack> attacks;
    }

    [Header("3rd Attack Groups")]
    public List<ThirdComboGroup> thirdAttackGroups;

    private string lastFirstNote = "";
    private string lastSecondNote = "";
    private GameGlobals.MusicScale firstNote;
    private GameGlobals.MusicScale secondNote;
    
    public bool hasPlayedComboSound = false;
    public AudioSource audioSource;
    public bool isInChurch = false;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float footstepVolume = 1f;
    [Range(0f, 1f)] public float attackVolume = 1f;
    [Range(0f, 1f)] public float blockVolume = 1f;
    [Range(0f, 1f)] public float parryVolume = 1f;
    [Range(0f, 1f)] public float rollVolume = 1f;
    [Range(0f, 1f)] public float comboVolume = 1f;

    private string ExtractNoteFromClipName(string clipName)
    {
        // If there's an underscore, we expect the note to be after it.
        int lastUnderscore = clipName.LastIndexOf('_');
        if (lastUnderscore >= 0 && lastUnderscore < clipName.Length - 1)
        {
            return clipName.Substring(lastUnderscore + 1);
        }

        // Otherwise, check if the clip name ends with a valid note.
        string[] validNotes = new string[] { "A4", "C5", "DS5", "F5", "AS4", "CS5", "GS5", "E5", "FS5", "A5", "CS6" };
        foreach (var note in validNotes)
        {
            if (clipName.EndsWith(note))
            {
                return note;
            }
        }

        Debug.LogWarning($"Could not extract note from clip name: {clipName}");
        return ""; // Return empty string if extraction fails
    }

    // All the following functions randomize and play one of the sounds for the chosen action.
    // They all count in also the church variations

    public void PlayFootstep()
    {
        AudioClip selectedClip = isInChurch ?
            churchFootstepSounds[UnityEngine.Random.Range(0, churchFootstepSounds.Length)] :
            footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)];

        audioSource.PlayOneShot(selectedClip, footstepVolume);
    }

    public void PlayMinorStab1()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab1Sounds[UnityEngine.Random.Range(0, churchMinorStab1Sounds.Length)] :
            minorStab1Sounds[UnityEngine.Random.Range(0, minorStab1Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayMinorStab2()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab2Sounds[UnityEngine.Random.Range(0, churchMinorStab2Sounds.Length)] :
            minorStab2Sounds[UnityEngine.Random.Range(0, minorStab2Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayMinorStab3()
    {
        AudioClip selectedClip = isInChurch ?
            churchMinorStab3Sounds[UnityEngine.Random.Range(0, churchMinorStab3Sounds.Length)] :
            minorStab3Sounds[UnityEngine.Random.Range(0, minorStab3Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayMajorSlash1()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash1Sounds[UnityEngine.Random.Range(0, churchMajorSlash1Sounds.Length)] :
            majorSlash1Sounds[UnityEngine.Random.Range(0, majorSlash1Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayMajorSlash2()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash2Sounds[UnityEngine.Random.Range(0, churchMajorSlash2Sounds.Length)] :
            majorSlash2Sounds[UnityEngine.Random.Range(0, majorSlash2Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayMajorSlash3()
    {
        AudioClip selectedClip = isInChurch ?
            churchMajorSlash3Sounds[UnityEngine.Random.Range(0, churchMajorSlash3Sounds.Length)] :
            majorSlash3Sounds[UnityEngine.Random.Range(0, majorSlash3Sounds.Length)];

        audioSource.PlayOneShot(selectedClip, attackVolume);
    }

    public void PlayBlockExecution()
    {
        AudioClip selectedClip = isInChurch ?
            churchBlockExecutionSounds[UnityEngine.Random.Range(0, churchBlockExecutionSounds.Length)] :
            blockExecutionSounds[UnityEngine.Random.Range(0, blockExecutionSounds.Length)];

        audioSource.PlayOneShot(selectedClip, blockVolume);
    }

    public void PlayParryExecution()
    {
        AudioClip selectedClip = isInChurch ?
            churchParryExecutionSounds[UnityEngine.Random.Range(0, churchParryExecutionSounds.Length)] :
            parryExecutionSounds[UnityEngine.Random.Range(0, parryExecutionSounds.Length)];

        audioSource.PlayOneShot(selectedClip, parryVolume);
    }

    public void PlayRoll()
    {
        AudioClip selectedClip = isInChurch ?
            churchRollSounds[UnityEngine.Random.Range(0, churchRollSounds.Length)] :
            rollSounds[UnityEngine.Random.Range(0, rollSounds.Length)];

        audioSource.PlayOneShot(selectedClip, rollVolume);
    }

    public void resetCombo()
    {
        hasPlayedComboSound = false;
    }

    // When the first attack is executed, store the correct note
    public void PlayComboFirstAttack(out string playedNote)
    {
        if (firstAttackClips.Count == 0)
        {
            playedNote = "";
            return;
        }

        int index = UnityEngine.Random.Range(0, firstAttackClips.Count);
        AudioClip clip = firstAttackClips[index];
        audioSource.PlayOneShot(clip, comboVolume);

        playedNote = ExtractNoteFromClipName(clip.name); // Extract the note for the next attack
        lastFirstNote = playedNote; // Store for future use
    }

    public void PlayComboSecondAttack(string prevNote, out string playedNote)
    {
        playedNote = "";

        // Find the second attack group based on the note from the first attack
        var match = secondAttackClips.Find(x => x.note == prevNote);
        if (match != null && match.clips.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, match.clips.Count);
            AudioClip clip = match.clips[index];
            audioSource.PlayOneShot(clip, comboVolume);

            playedNote = ExtractNoteFromClipName(clip.name); // Same as previous code
            lastSecondNote = playedNote;
        }
        else
        {
            // Fallback: random from all available second attack clips
            List<AudioClip> allClips = new List<AudioClip>();
            foreach (var group in secondAttackClips)
            {
                allClips.AddRange(group.clips);
            }

            if (allClips.Count > 0)
            {
                AudioClip fallbackClip = allClips[UnityEngine.Random.Range(0, allClips.Count)];
                audioSource.PlayOneShot(fallbackClip, comboVolume);
                playedNote = ExtractNoteFromClipName(fallbackClip.name);
                lastSecondNote = playedNote;
            }
            else
            {
                Debug.LogWarning("No fallback second attack clips available.");
            }
        }
    }

    public void PlayComboThirdAttack(string firstNote, string secondNote, GameGlobals.MusicScale scale)
    {
        if (string.IsNullOrEmpty(secondNote) || string.IsNullOrEmpty(firstNote))
        {
            PlayFallbackThirdAttack();
            return;
        }

        // Find the group for the third attack based on the 1st note
        var group = thirdAttackGroups.Find(g => g.groupLabel == firstNote);
        if (group == null)
        {
            Debug.LogWarning($"No third attack group found for first note '{firstNote}'.");
            return;
        }

        // Find the attack group for the 2nd note
        var attackGroup = group.attackGroups.Find(a => a.attackType == secondNote);
        if (attackGroup == null || attackGroup.attacks.Count == 0)
        {
            Debug.LogWarning($"No third attack group found for second note '{secondNote}'.");
            return;
        }

        // Now find a ThirdAttack with the correct scale label
        string scaleLabel = scale == GameGlobals.MusicScale.MAJOR ? "MAJOR" : "MINOR";
        var matchingAttack = attackGroup.attacks.Find(a => a.label == scaleLabel);
        if (matchingAttack == null || matchingAttack.clips.Count == 0)
        {
            Debug.LogWarning($"No third attack clip found for scale '{scaleLabel}' under second note '{secondNote}'.");
            return;
        }

        PlayRandom(matchingAttack.clips);
    }

    private void PlayFallbackThirdAttack()
    {
        List<AudioClip> allClips = new List<AudioClip>();

        foreach (var group in thirdAttackGroups)
        {
            foreach (var attackGroup in group.attackGroups)
            {
                foreach (var attack in attackGroup.attacks)
                {
                    allClips.AddRange(attack.clips);
                }
            }
        }

        if (allClips.Count > 0)
        {
            PlayRandom(allClips);
        }
        else
        {
            Debug.LogWarning("No fallback third attack clips available.");
        }
    }

    public void ResetCombo()
    {
        lastSecondNote = "";
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
    }

    private void PlayRandom(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Count)], comboVolume);
    }

    private void OnEnable()
    {
        CustomEvents.OnDamageDelt += HandleDamageDelt;
    }

    private void OnDisable()
    {
        CustomEvents.OnDamageDelt -= HandleDamageDelt;
    }

    private void HandleDamageDelt(int damage, GameGlobals.MusicScale scale, bool sameScale, HealthManager enemy, int comboLength)
    {
        if (hasPlayedComboSound)
            return;

        string note = scale.ToString(); // Get the current scale (MAJOR or MINOR)

        switch (comboLength)
        {
            case 1:
                string firstPlayedNote;
                PlayComboFirstAttack(out firstPlayedNote);
                lastFirstNote = firstPlayedNote;
                break;

            case 2:
                string secondPlayedNote;
                PlayComboSecondAttack(lastFirstNote, out secondPlayedNote);
                lastSecondNote = secondPlayedNote;
                break;

            case 3:
                PlayComboThirdAttack(lastFirstNote, lastSecondNote, scale);
                break;

            default:
                Debug.LogWarning($"Unhandled combo length: {comboLength}");
                return;
        }

        hasPlayedComboSound = true; // Prevent from playing again this frame
    }
}
