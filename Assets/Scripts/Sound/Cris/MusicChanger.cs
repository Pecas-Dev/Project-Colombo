using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ProjectColombo.GameManagement.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float mainMenuVolume = 1f;
    [Range(0f, 1f)] public float explorationVolume = 0.5f;
    [Range(0f, 1f)] public float churchEntranceVolume = 0.5f;
    [Range(0f, 1f)] public float[] bossBattleLayerVolumes = new float[4] { 1f, 1f, 1f, 1f };
    [Range(0f, 1f)] public float endingSongVolume = 0.5f;
    [Range(0f, 1f)] public float[] battleLayerVolumes = new float[4] { 1f, 1f, 1f, 1f };
    [Range(0f, 1f)] public float loseSceneVolume = 0.5f;
    [Range(0f, 1f)] public float creditsSceneVolume = 0.5f;

    [Header("General Music Settings")]
    public float fadeSpeed = 1f;

    [Header("Music Clips")]
    public AudioClip menuMusicClip;
    public AudioClip tutorialMusicClip;
    public AudioClip levelExplorationClip;
    public AudioClip[] levelBattleClips;
    public AudioClip bossExplorationClip;
    public AudioClip[] bossBattleClips;
    public AudioClip endingSongClip;
    public AudioClip loseSceneClip;
    public AudioClip creditsSceneClip;

    private enum MusicCategory { None, MainMenu, Exploration, Battle, ChurchEntrance, BossBattle, Ending, Lose, Credits }
    private MusicCategory currentMusicCategory = MusicCategory.None;

    private AudioSource explorationMusic;
    private AudioSource[] battleMusicLayers = new AudioSource[4];
    private bool canPlayBattleMusic = true;
    private float musicIntensity = 0f;
    private float battleBlend = 0f;
    private int currentComboLevel = 0;
    private string currentScene = "";
    private bool isInGameplay = false;
    private Coroutine battleBlendCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        explorationMusic = gameObject.AddComponent<AudioSource>();
        explorationMusic.loop = true;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            battleMusicLayers[i] = gameObject.AddComponent<AudioSource>();
            battleMusicLayers[i].loop = true;
            battleMusicLayers[i].volume = 0f;
        }
    }

    private void Start()
    {
        CustomEvents.OnChamberActivated += HandleChamberActivated;
        CustomEvents.OnChamberFinished += HandleChamberFinished;
        CustomEvents.OnComboMeterLevelIncrease += HandleComboChange;
        CustomEvents.OnComboMeterLevelDecrease += HandleComboChange;
        CustomEvents.OnBossFightStarted += HandleBossFightStarted;

        StartCoroutine(PreloadAllAudioClips());
    }

    private IEnumerator PreloadAllAudioClips()
    {
        List<AudioClip> clipsToPreload = new List<AudioClip>
    {
        menuMusicClip,
        tutorialMusicClip,
        levelExplorationClip,
        bossExplorationClip,
        endingSongClip
    };

        clipsToPreload.AddRange(levelBattleClips);
        clipsToPreload.AddRange(bossBattleClips);
        clipsToPreload.Add(loseSceneClip);
        clipsToPreload.Add(creditsSceneClip);

        foreach (var clip in clipsToPreload)
        {
            if (clip == null || clip.loadState == AudioDataLoadState.Loaded)
                continue;

            clip.LoadAudioData(); // Asynchronous
            float timer = 0f;
            while (clip.loadState == AudioDataLoadState.Loading && timer < 2f)
            {
                timer += Time.unscaledDeltaTime;
                yield return null; // Wait until clip finishes loading
            }
        }
    }

    private void OnDestroy()
    {
        CustomEvents.OnChamberActivated -= HandleChamberActivated;
        CustomEvents.OnChamberFinished -= HandleChamberFinished;
        CustomEvents.OnComboMeterLevelIncrease -= HandleComboChange;
        CustomEvents.OnComboMeterLevelDecrease -= HandleComboChange;
        CustomEvents.OnBossFightStarted -= HandleBossFightStarted;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (explorationMusic.clip != null && explorationMusic.isPlaying)
        {
            UpdateExplorationVolume();
            UpdateBattleVolume();
            UpdateBattleMusicLayers();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        isInGameplay = true;
        battleBlend = 0f;
        musicIntensity = 0f;
        canPlayBattleMusic = false;

        switch (currentScene)
        {
            case "00_MainMenu":
                PlayMusic(menuMusicClip, MusicCategory.MainMenu);
                isInGameplay = false;
                break;
            case "01_Tutorial":
            case "02_MaskSelection":
                PlayMusic(tutorialMusicClip, MusicCategory.Exploration);
                break;
            case "03_LevelTwo":
                PlayGameplayMusic(levelExplorationClip, levelBattleClips);
                StartCoroutine(EnableBattleMusicAfterDelay(3f));
                break;
            case "04_Church":
                PlayGameplayMusic(bossExplorationClip, bossBattleClips);
                StartCoroutine(EnableBattleMusicAfterDelay(3f));
                break;
            case "06_LooseScene":
                PlayMusic(loseSceneClip, MusicCategory.Lose);
                explorationMusic.loop = false;
                isInGameplay = false;
                break;
            case "07_Credits":
                PlayMusic(creditsSceneClip, MusicCategory.Credits);
                explorationMusic.loop = false;
                isInGameplay = false;
                break;
            default:
                StopAllMusic();
                isInGameplay = false;
                break;
        }
    }

    private void PlayMusic(AudioClip clip, MusicCategory category)
    {
        StopAllMusic();
        if (clip == null) return;
        currentMusicCategory = category;
        explorationMusic.clip = clip;
        explorationMusic.loop = true;
        explorationMusic.volume = GetInitialExplorationVolume();
        explorationMusic.Play();
    }

    private void PlayGameplayMusic(AudioClip explorationClip, AudioClip[] battleClips)
    {
        StopAllMusic();
        if (explorationClip == null) return;

        explorationMusic.clip = explorationClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleClips != null && i < battleClips.Length && battleClips[i] != null)
            {
                battleMusicLayers[i].clip = battleClips[i];
                battleMusicLayers[i].volume = 0f;
            }
        }
        currentMusicCategory = (currentScene == "04_Church") ? MusicCategory.ChurchEntrance : MusicCategory.Exploration;
    }

    private void StopAllMusic()
    {
        explorationMusic.Stop();
        foreach (var layer in battleMusicLayers) layer.Stop();
    }

    private IEnumerator EnableBattleMusicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canPlayBattleMusic = true;
    }

    private float GetInitialExplorationVolume()
    {
        float volume = masterVolume;
        switch (currentMusicCategory)
        {
            case MusicCategory.MainMenu: volume *= mainMenuVolume; break;
            case MusicCategory.Exploration: volume *= explorationVolume; break;
            case MusicCategory.ChurchEntrance: volume *= churchEntranceVolume; break;
            case MusicCategory.Ending: volume *= endingSongVolume; break;
            case MusicCategory.Lose: volume *= loseSceneVolume; break;
            case MusicCategory.Credits: volume *= creditsSceneVolume; break;
        }
        return (1f - battleBlend) * volume;
    }

    private float GetBattleLayerVolume(int index)
    {
        float baseVolume = masterVolume * battleBlend * Mathf.Clamp01(musicIntensity);

        if (currentMusicCategory == MusicCategory.BossBattle)
        {
            return baseVolume * bossBattleLayerVolumes[index] * 0.65f;
        }
        else
        {
            return baseVolume * battleLayerVolumes[index] * explorationVolume;
        }
    }

    private void UpdateExplorationVolume()
    {
        float targetVolume = GetInitialExplorationVolume();
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }

    private void UpdateBattleVolume()
    {
        if (!isInGameplay) return;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip != null)
            {
                float targetVol = GetBattleLayerVolume(i);
                battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, targetVol, Time.deltaTime * fadeSpeed);
            }
        }
    }

    private void UpdateBattleMusicLayers()
    {
        float baseVolume = battleBlend * Mathf.Clamp01(musicIntensity) * masterVolume;
        float categoryMultiplier = (currentMusicCategory == MusicCategory.BossBattle) ? 1f : explorationVolume;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip == null) continue;
            bool isActive = (i == currentComboLevel);
            float targetVol = (battleBlend > 0f && isActive) ? baseVolume * battleLayerVolumes[i] * categoryMultiplier : 0f;

            if (battleBlend > 0f && !battleMusicLayers[i].isPlaying)
                battleMusicLayers[i].Play();

            battleMusicLayers[i].volume = Mathf.MoveTowards(battleMusicLayers[i].volume, targetVol, Time.deltaTime * fadeSpeed);
        }
    }

    private void HandleChamberActivated()
    {
        if (currentScene == "01_Tutorial" || !canPlayBattleMusic) return;

        musicIntensity = 0.5f;
        currentMusicCategory = (currentScene == "04_Church") ? MusicCategory.BossBattle : MusicCategory.Battle;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip != null)
            {
                battleMusicLayers[i].Stop();
                battleMusicLayers[i].Play();
                battleMusicLayers[i].volume = 0f;
            }
        }

        SetBattleBlend(1f, 1f);
    }

    private void HandleChamberFinished()
    {
        if (currentScene == "01_Tutorial") return;

        StartCoroutine(FadeOutAndStopBattleMusic(2f));
        PlayMusic((currentScene == "04_Church") ? bossExplorationClip : levelExplorationClip,
                  (currentScene == "04_Church") ? MusicCategory.ChurchEntrance : MusicCategory.Exploration);
    }

    public void SetBattleBlend(float target, float duration)
    {
        if (battleBlendCoroutine != null)
            StopCoroutine(battleBlendCoroutine);

        battleBlendCoroutine = StartCoroutine(FadeBattleBlend(target, duration));
    }

    private IEnumerator FadeBattleBlend(float target, float duration)
    {
        float start = battleBlend;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            battleBlend = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }
        battleBlend = target;
    }

    private IEnumerator FadeOutAndStopBattleMusic(float duration)
    {
        float startBlend = battleBlend;
        float time = 0f;
        float[] startVolumes = new float[battleMusicLayers.Length];
        for (int i = 0; i < battleMusicLayers.Length; i++)
            startVolumes[i] = battleMusicLayers[i].volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            battleBlend = Mathf.Lerp(startBlend, 0f, t);
            for (int i = 0; i < battleMusicLayers.Length; i++)
                if (battleMusicLayers[i].clip != null)
                    battleMusicLayers[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
            yield return null;
        }

        battleBlend = 0f;
        foreach (var layer in battleMusicLayers)
        {
            layer.Stop();
            layer.volume = 0f;
        }
    }

    private void HandleComboChange(int level)
    {
        currentComboLevel = Mathf.Clamp(level, 0, 3);
        UpdateBattleMusicLayers();
    }

    public void UpdateVolumeSettings(float newMaster, float newMainMenu, float newExploration,
                                     float newChurchEntrance, float newEnding,
                                     float[] newBattleLayerVolumes, float[] newBossBattleLayerVolumes)
    {
        masterVolume = newMaster;
        mainMenuVolume = newMainMenu;
        explorationVolume = newExploration;
        churchEntranceVolume = newChurchEntrance;
        endingSongVolume = newEnding;

        for (int i = 0; i < battleLayerVolumes.Length && i < newBattleLayerVolumes.Length; i++)
            battleLayerVolumes[i] = newBattleLayerVolumes[i];

        for (int i = 0; i < bossBattleLayerVolumes.Length && i < newBossBattleLayerVolumes.Length; i++)
            bossBattleLayerVolumes[i] = newBossBattleLayerVolumes[i];
    }

    private void HandleBossFightStarted()
    {
        if (currentScene != "04_Church") return;

        currentMusicCategory = MusicCategory.BossBattle;
        musicIntensity = 1f;
        battleBlend = 1f;
        canPlayBattleMusic = true;

        if (explorationMusic.isPlaying)
        {
            StartCoroutine(CrossfadeExplorationToBattle(1f));
        }
        else
        {
            for (int i = 0; i < battleMusicLayers.Length; i++)
            {
                if (i < bossBattleClips.Length && bossBattleClips[i] != null)
                {
                    battleMusicLayers[i].clip = bossBattleClips[i];
                    battleMusicLayers[i].volume = (i == currentComboLevel) ? GetBattleLayerVolume(i) : 0f;
                    battleMusicLayers[i].Play();
                }
                else
                {
                    battleMusicLayers[i].Stop();
                }
            }
        }

        Debug.Log("Boss fight started - Playing music");
        Debug.Log($"Battle clip[0]: {bossBattleClips[0]?.name}");
        Debug.Log($"Current Combo Level: {currentComboLevel}");
    }

    private IEnumerator CrossfadeExplorationToBattle(float duration)
    {
        float startVolume = explorationMusic.volume;
        float time = 0f;

        currentMusicCategory = MusicCategory.BossBattle;
        musicIntensity = 1f;
        battleBlend = 1f;
        canPlayBattleMusic = true;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (i < bossBattleClips.Length && bossBattleClips[i] != null)
            {
                battleMusicLayers[i].clip = bossBattleClips[i];
                battleMusicLayers[i].volume = 0f;
                battleMusicLayers[i].Play();
            }
            else
            {
                battleMusicLayers[i].Stop();
            }
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            explorationMusic.volume = Mathf.Lerp(startVolume, 0f, t);

            for (int i = 0; i < battleMusicLayers.Length; i++)
            {
                if (battleMusicLayers[i].clip != null)
                {
                    float targetVol = (i == currentComboLevel) ? GetBattleLayerVolume(i) : 0f;
                    battleMusicLayers[i].volume = Mathf.Lerp(0f, targetVol, t);
                }
            }

            yield return null;
        }

        explorationMusic.Stop();
        explorationMusic.volume = 0f;
    }
}