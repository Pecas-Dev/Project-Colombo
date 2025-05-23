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
    public AudioClip churchExplorationClip;
    public AudioClip[] churchBattleClips;

    [Header("Ending Music")]
    public AudioClip endingSongClip;

    private enum MusicCategory { None, MainMenu, Exploration, Battle, ChurchEntrance, ChurchFight, Ending }
    private MusicCategory currentMusicCategory = MusicCategory.None;

    private AudioSource explorationMusic;
    private AudioSource[] battleMusicLayers;
    private float musicIntensity = 0f;
    private float battleBlend = 0f;
    private int currentComboLevel = 0;
    private bool isFadingOut = false;
    private string currentScene = "";
    private bool isInGameplay = false;

    #region Unity Methods

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
        CustomEvents.OnComboMeterLevelIncrease += HandleComboIncrease;
        CustomEvents.OnComboMeterLevelDecrease += HandleComboDecrease;
    }

    private void OnDestroy()
    {
        CustomEvents.OnChamberActivated -= HandleChamberActivated;
        CustomEvents.OnChamberFinished -= HandleChamberFinished;
        CustomEvents.OnComboMeterLevelIncrease -= HandleComboIncrease;
        CustomEvents.OnComboMeterLevelDecrease -= HandleComboDecrease;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (explorationMusic.clip == null || !explorationMusic.isPlaying) return;

        UpdateExplorationVolume();
        UpdateBattleVolume();

        UpdateBattleMusicLayers(); // smooth fade per frame
    }

    #endregion

    #region Volume & Mixing

    private void UpdateExplorationVolume()
    {
        float volumeMultiplier = masterVolume;

        switch (currentMusicCategory)
        {
            case MusicCategory.MainMenu: volumeMultiplier *= mainMenuVolume; break;
            case MusicCategory.Exploration: volumeMultiplier *= explorationVolume; break;
            case MusicCategory.ChurchEntrance: volumeMultiplier *= churchEntranceVolume; break;
            case MusicCategory.ChurchFight: volumeMultiplier *= churchFightVolume; break;
            case MusicCategory.Ending: volumeMultiplier *= endingSongVolume; break;
        }

        float targetVolume = (1f - battleBlend) * volumeMultiplier;
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }

    private void UpdateBattleVolume()
    {
        if (!isInGameplay || battleMusicLayers[0].clip == null) return;

        float baseVolume = masterVolume * battleBlend * Mathf.Clamp01(musicIntensity);
        float categoryMultiplier = currentMusicCategory == MusicCategory.ChurchFight ? churchFightVolume : explorationVolume;

        float target = baseVolume * battleLayerVolumes[0] * categoryMultiplier;
        battleMusicLayers[0].volume = Mathf.Lerp(battleMusicLayers[0].volume, target, Time.deltaTime * fadeSpeed);

        // Uncomment for additional layers if needed
     
        for (int i = 1; i < battleMusicLayers.Length; i++)
        {
            float targetVol = baseVolume * battleLayerVolumes[i] * categoryMultiplier;
            battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, targetVol, Time.deltaTime * fadeSpeed);
        }
    }

    #endregion

    #region Scene and Music Logic

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        Debug.Log("Loaded scene: " + currentScene);

        bool sharedExploration = currentScene == "01_MaskSelection" || currentScene == "02_Tutorial";

        if (!sharedExploration)
            StopAllCoroutines();

        isInGameplay = true;

        switch (currentScene)
        {
            case "00_MainMenu":
                PlayMenuMusic();
                isInGameplay = false;
                break;
            case "01_Tutorial":
            case "02_MaskSelection":
                PlayTutorialMusic();
                break;
            case "03_LevelTwo":
            case "04_LevelThree":
                PlayGameplayMusic(levelExplorationClip, levelBattleClips);
                break;
            case "05_Church":
                PlayGameplayMusic(churchExplorationClip, churchBattleClips);
                break;
            case "06_WinScene":
            case "07_LooseScene":
                StopAllMusic();
                isInGameplay = false;
                break;
            default:
                StopAllMusic();
                isInGameplay = false;
                break;
        }

        battleBlend = 0f;
        musicIntensity = 0f;
    }

    private void PlayMenuMusic()
    {
        if (explorationMusic.clip == menuMusicClip && explorationMusic.isPlaying) return;

        StopAllMusic();
        if (menuMusicClip == null) return;

        explorationMusic.clip = menuMusicClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        currentMusicCategory = MusicCategory.MainMenu;
    }

    private void PlayTutorialMusic()
    {
        if (explorationMusic.clip == tutorialMusicClip && explorationMusic.isPlaying) return;

        StopAllMusic();
        if (tutorialMusicClip == null) return;

        explorationMusic.clip = tutorialMusicClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        currentMusicCategory = MusicCategory.Exploration;
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

        // Assign and play battle music layers
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleClips != null && i < battleClips.Length && battleClips[i] != null)
            {
                battleMusicLayers[i].clip = battleClips[i];
                battleMusicLayers[i].volume = 0f;
                //battleMusicLayers[i].Play();
            }
        }

        currentMusicCategory = currentScene == "05_Church" ? MusicCategory.ChurchEntrance : MusicCategory.Exploration;
    }

    private void StopAllMusic()
    {
        explorationMusic.Stop();
        foreach (var layer in battleMusicLayers) layer.Stop();
    }

    public IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = explorationMusic.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            explorationMusic.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        explorationMusic.Stop();
        explorationMusic.volume = startVolume;
    }

    #endregion

    #region Event Handlers

    private void HandleChamberActivated()
    {
        if (currentScene == "01_Tutorial") return;

        musicIntensity = 0.5f;
        currentMusicCategory = currentScene == "05_Church" ? MusicCategory.ChurchFight : MusicCategory.Battle;

        // Start all clips from the beginning
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip != null)
            {
                battleMusicLayers[i].Stop();
                battleMusicLayers[i].Play();
                battleMusicLayers[i].volume = 0f;
            }
        }

        // Begin fading in
        SetBattleBlend(1f, 1f);
    }

    private void HandleChamberFinished()
    {
        if (currentScene == "01_Tutorial") return;

        StartCoroutine(FadeOutAndStopBattleMusic(2f));

        if (currentScene == "05_Church")
            PlayExplorationMusic(churchExplorationClip, MusicCategory.ChurchEntrance);
        else
            PlayExplorationMusic(levelExplorationClip, MusicCategory.Exploration);
    }

    private Coroutine battleBlendCoroutine;

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

        while (time < duration)
        {
            time += Time.deltaTime;
            battleBlend = Mathf.Lerp(startBlend, 0f, time / duration);
            yield return null;
        }

        // Stop and reset battle music layers
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            battleMusicLayers[i].Stop();
            battleMusicLayers[i].volume = 0f;
        }
    }

    public void SetBattleIntensity(float intensity)
    {
        musicIntensity = Mathf.Clamp01(intensity);
    }

    private void HandleComboIncrease(int newLevel)
    {
        currentComboLevel = Mathf.Clamp(newLevel, 0, 3);
        UpdateBattleMusicLayers();
    }

    private void HandleComboDecrease(int newLevel)
    {
        currentComboLevel = Mathf.Clamp(newLevel, 0, 3);
        UpdateBattleMusicLayers();
    }
    private void PlayExplorationMusic(AudioClip clip, MusicCategory category)
    {
        if (explorationMusic.clip == clip && explorationMusic.isPlaying)
            return;

        explorationMusic.Stop();
        explorationMusic.clip = clip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();
        currentMusicCategory = category;
    }


    private float[] targetBattleVolumes = new float[4];

    private void UpdateBattleMusicLayers()
    {
        float baseVolume = battleBlend * Mathf.Clamp01(musicIntensity) * masterVolume;
        float categoryMultiplier = currentMusicCategory == MusicCategory.ChurchFight ? churchFightVolume : explorationVolume;

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip == null) continue;

            // Layers fade in/out smoothly - all layers keep playing
            bool isActiveLayer = (i == currentComboLevel);
            float targetVol = isActiveLayer ? baseVolume * battleLayerVolumes[i] * categoryMultiplier : 0f;

            if (!battleMusicLayers[i].isPlaying)
                battleMusicLayers[i].Play();

            // Smoothly move volume towards target
            battleMusicLayers[i].volume = Mathf.MoveTowards(battleMusicLayers[i].volume, targetVol, Time.deltaTime * fadeSpeed);
        }
    }

    private void UpdateBattleLayerVolumesSmoothly()
    {
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (battleMusicLayers[i].clip == null) continue;

            float currentVol = battleMusicLayers[i].volume;
            float targetVol = targetBattleVolumes[i];
            battleMusicLayers[i].volume = Mathf.MoveTowards(currentVol, targetVol, Time.deltaTime * fadeSpeed);
        }
    }

    #endregion
}