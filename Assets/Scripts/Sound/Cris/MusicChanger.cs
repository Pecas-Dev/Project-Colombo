using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectColombo.GameManagement.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float mainMenuVolume = 1f;
    [Range(0f, 1f)] public float explorationVolume = 0.5f;
    [Range(0f, 1f)] public float churchEntranceVolume = 0.5f;
    [Range(0f, 1f)] public float churchFightVolume = 0.5f;
    [Range(0f, 1f)] public float endingSongVolume = 0.5f;
    [Range(0f, 1f)] public float[] battleLayerVolumes = new float[4] { 1f, 1f, 1f, 1f };

    [Header("General Music Settings")]
    public float fadeSpeed = 1f;

    [Header("Menu Music")]
    public AudioClip menuMusicClip;

    [Header("Tutorial Music")]
    public AudioClip tutorialMusicClip;

    [Header("Exploration Sets")]
    public AudioClip levelExplorationClip;
    public AudioClip[] levelBattleClips;

    [Header("Church Music Set")]
    public AudioClip bossExplorationClip;

    [Header("Boss Music Set")]
    public AudioClip[] bossBattleClips;

    [Header("Ending Music")]
    public AudioClip endingSongClip;

    private enum MusicCategory { None, MainMenu, Exploration, Battle, ChurchEntrance, BossBattle, Ending }
    private MusicCategory currentMusicCategory = MusicCategory.None;

    private AudioSource explorationMusic;
    private AudioSource[] battleMusicLayers;
    private bool canPlayBattleMusic = true;
    private float musicIntensity = 0f;
    private float battleBlend = 0f;
    private int currentComboLevel = 0;
    private string currentScene = "";
    private bool isInGameplay = false;
    private Coroutine battleBlendCoroutine;

    public bool hasBossFightBegun = false;
    public bool hasBossDied = false;

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

        battleMusicLayers = new AudioSource[4];
        for (int i = 0; i < 4; i++)
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
    }

    private void OnDestroy()
    {
        CustomEvents.OnChamberActivated -= HandleChamberActivated;
        CustomEvents.OnChamberFinished -= HandleChamberFinished;
        CustomEvents.OnComboMeterLevelIncrease -= HandleComboChange;
        CustomEvents.OnComboMeterLevelDecrease -= HandleComboChange;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (hasBossDied)
        {
            StopAllMusic();
            return;
        }

        if (hasBossFightBegun && currentMusicCategory != MusicCategory.Battle)
        {
            return;
        }

        if (explorationMusic.clip == null || !explorationMusic.isPlaying)
            return;

        UpdateExplorationVolume();
        UpdateBattleVolume();
        UpdateBattleMusicLayers();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        isInGameplay = true;

        battleBlend = 0f;
        musicIntensity = 0f;
        canPlayBattleMusic = false;

        if (hasBossDied)
        {
            StopAllMusic();
            return;
        }

        if (hasBossFightBegun && currentMusicCategory != MusicCategory.BossBattle)
        {
            PlayGameplayMusic(null, bossBattleClips, true); // Keep exploration if any
            currentMusicCategory = MusicCategory.BossBattle;
            SetBattleBlend(1f, 1f);
            return;
        }

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
            case "05_WinScene":
            case "06_LooseScene":
                StopAllMusic();
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
        explorationMusic.volume = GetInitialExplorationVolume();
        explorationMusic.Play();
    }

    private void PlayGameplayMusic(AudioClip explorationClip, AudioClip[] battleClips, bool preserve = false)
    {
        if (preserve && explorationMusic.clip == explorationClip)
        {
            if (!explorationMusic.isPlaying)
                explorationMusic.Play();
            return;
        }

        StopAllMusic();

        if (explorationClip == null)
        {
            Debug.LogWarning("Exploration clip is null!");
            return;
        }

        // Assign and play exploration music
        explorationMusic.clip = explorationClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        // Assign battle music layers but DO NOT play them yet
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleClips != null && i < battleClips.Length && battleClips[i] != null)
            {
                battleMusicLayers[i].clip = battleClips[i];
                battleMusicLayers[i].volume = 0f;
                // DO NOT call Play() here
            }
        }

        currentMusicCategory = currentScene == "04_Church" ? MusicCategory.ChurchEntrance : MusicCategory.Exploration;
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
        float volumeMultiplier = masterVolume;

        switch (currentMusicCategory)
        {
            case MusicCategory.MainMenu: volumeMultiplier *= mainMenuVolume; break;
            case MusicCategory.Exploration: volumeMultiplier *= explorationVolume; break;
            case MusicCategory.ChurchEntrance: volumeMultiplier *= churchEntranceVolume; break;
            case MusicCategory.BossBattle: volumeMultiplier *= churchFightVolume; break;
            case MusicCategory.Ending: volumeMultiplier *= endingSongVolume; break;
        }

        return (1f - battleBlend) * volumeMultiplier;
    }

    private float GetBattleLayerVolume(int index)
    {
        float baseVolume = masterVolume * battleBlend * Mathf.Clamp01(musicIntensity);
        float categoryMultiplier =
            currentMusicCategory == MusicCategory.BossBattle || currentMusicCategory == MusicCategory.BossBattle
             ? churchFightVolume : explorationVolume;
        return baseVolume * battleLayerVolumes[index] * categoryMultiplier;
    }

    private void UpdateExplorationVolume()
    {
        float targetVolume = GetInitialExplorationVolume();
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }

    private void UpdateBattleVolume()
    {
        if (!isInGameplay || battleMusicLayers[0].clip == null) return;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            float targetVol = GetBattleLayerVolume(i);
            battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, targetVol, Time.deltaTime * fadeSpeed);
        }
    }

    private void UpdateBattleMusicLayers()
    {
        float baseVolume = battleBlend * Mathf.Clamp01(musicIntensity) * masterVolume;
        float categoryMultiplier = currentMusicCategory == MusicCategory.BossBattle ? churchFightVolume : explorationVolume;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip == null) continue;

            bool isActiveLayer = (i == currentComboLevel);
            float targetVol = (battleBlend > 0f && isActiveLayer) ? baseVolume * battleLayerVolumes[i] * categoryMultiplier : 0f;

            if (battleBlend > 0f && !battleMusicLayers[i].isPlaying)
                battleMusicLayers[i].Play(); // Only play if we're blending into battle

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

        AudioClip clip = (currentScene == "04_Church") ? bossExplorationClip : levelExplorationClip;
        MusicCategory category = (currentScene == "04_Church") ? MusicCategory.ChurchEntrance : MusicCategory.Exploration;
        PlayMusic(clip, category);
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

        // Cache initial volumes
        float[] startVolumes = new float[battleMusicLayers.Length];
        for (int i = 0; i < battleMusicLayers.Length; i++)
            startVolumes[i] = battleMusicLayers[i].volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            battleBlend = Mathf.Lerp(startBlend, 0f, t);

            for (int i = 0; i < battleMusicLayers.Length; i++)
            {
                if (battleMusicLayers[i].clip != null)
                {
                    battleMusicLayers[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
                }
            }

            yield return null;
        }

        // Fully faded out, now stop and reset
        battleBlend = 0f;
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            battleMusicLayers[i].Stop();
            battleMusicLayers[i].volume = 0f;
        }
    }

    private void HandleComboChange(int newLevel)
    {
        currentComboLevel = Mathf.Clamp(newLevel, 0, 3);
        UpdateBattleMusicLayers();
    }

    private void HandleBossComboChange(int level)
    {
        currentComboLevel = Mathf.Clamp(level, 0, bossBattleClips.Length - 1);
        UpdateBattleMusicLayers();
    }

    public void UpdateVolumeSettings(
        float newMaster,
        float newMainMenu,
        float newExploration,
        float newChurchEntrance,
        float newChurchFight,
        float newEnding,
        float[] newBattleLayerVolumes)
    {
        masterVolume = newMaster;
        mainMenuVolume = newMainMenu;
        explorationVolume = newExploration;
        churchEntranceVolume = newChurchEntrance;
        churchFightVolume = newChurchFight;
        endingSongVolume = newEnding;

        for (int i = 0; i < battleLayerVolumes.Length && i < newBattleLayerVolumes.Length; i++)
            battleLayerVolumes[i] = newBattleLayerVolumes[i];
    }
}
