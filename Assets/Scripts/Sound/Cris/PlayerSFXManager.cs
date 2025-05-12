using System.Collections.Generic;
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

    //      [COMBO SYSTEM]
    [Header("1st Attack Sounds")]
    public List<AudioClip> firstAttackClips;

    [System.Serializable]
    public class SecondAttackGroup
    {
        public string note; // A4, C5, D#5, F5
        public List<AudioClip> clips;
    }

    [Header("2nd Attack Sounds")]
    public List<SecondAttackGroup> secondAttackClips;

    [System.Serializable]
    public class ThirdAttack
    {
        public string label; // A#4 Major, etc.
        public List<AudioClip> clips;
    }

    [System.Serializable]
    public class ThirdComboGroup
    {
        public string groupLabel; // COMBO A4, COMBO C5...

        // This is where we break down the 3rd attack into Major/Minor
        public List<ThirdAttackGroup> attackGroups;
    }

    // Nested structure for Major/Minor sound groupings within the third attack
    [System.Serializable]
    public class ThirdAttackGroup
    {
        public string attackType; // Major/Minor
        public List<ThirdAttack> attacks;
    }

    [Header("3rd Attack Groups")]
    public List<ThirdComboGroup> thirdAttackGroups;

    private string lastSecondNote = "";

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

    public void PlayComboFirstAttack()
    {
        PlayRandom(firstAttackClips);
    }

    public void PlayComboSecondAttack(string note)
    {
        var match = secondAttackClips.Find(x => x.note == note);
        if (match != null && match.clips.Count > 0)
        {
            PlayRandom(match.clips);
            lastSecondNote = note;
        }
        else
        {
            Debug.LogWarning($"Second attack clip for note '{note}' not found.");
        }
    }

    public void PlayComboThirdAttack(bool isMajor)
    {
        if (string.IsNullOrEmpty(lastSecondNote))
        {
            Debug.LogWarning("No second attack note recorded for third combo.");
            return;
        }

        var group = thirdAttackGroups.Find(g => g.groupLabel.EndsWith(lastSecondNote));
        if (group != null)
        {
            // Major or Minor selection based on the `isMajor` flag
            string attackType = isMajor ? "Major" : "Minor";

            // Look for the corresponding attack group
            var attackGroup = group.attackGroups.Find(a => a.attackType == attackType);

            if (attackGroup != null && attackGroup.attacks.Count > 0)
            {
                var attack = attackGroup.attacks[Random.Range(0, attackGroup.attacks.Count)];
                PlayRandom(attack.clips);
            }
            else
            {
                Debug.LogWarning($"No third attack clips found for note '{lastSecondNote}' ({attackType}).");
            }
        }
        else
        {
            Debug.LogWarning($"No third attack group found for note '{lastSecondNote}'.");
        }
    }

    public void ResetCombo()
    {
        lastSecondNote = "";
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    private void PlayRandom(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Count)]);
    }
}
