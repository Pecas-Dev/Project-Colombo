using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectColombo.GameManagement.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Volume Settings")]

    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float mainMenuVolume = 1f;

    [Range(0f, 1f)]
    public float explorationVolume = 0.5f;

    [Range(0f, 1f)]
    public float churchEntranceVolume = 0.5f;

    [Range(0f, 1f)]
    public float churchFightVolume = 0.5f;

    [Range(0f, 1f)]
    public float endingSongVolume = 0.5f;

    [Range(0f, 1f)]
    public float[] battleLayerVolumes = new float[4] { 1f, 1f, 1f, 1f };

    private enum MusicCategory
    {
        None,
        MainMenu,
        Exploration,
        Battle,
        ChurchEntrance,
        ChurchFight,
        Ending
    }

    private MusicCategory currentMusicCategory = MusicCategory.None;

    [Header("General Music Settings")]
    public float fadeSpeed = 1f;

    [Header("Menu Music")]
    public AudioClip menuMusicClip;

    [Header("Exploration Sets")]
    public AudioClip levelExplorationClip;
    public AudioClip[] levelBattleClips;

    [Header("Church Music Set")]
    public AudioClip churchExplorationClip;
    public AudioClip[] churchBattleClips;

    [Header("Ending Music")]
    public AudioClip endingSongClip;

    private AudioSource explorationMusic;
    private AudioSource[] battleMusicLayers;

    private float musicIntensity = 0f;
    private float battleBlend = 0f;
    private int currentComboLevel = 0;
    private bool isFadingOut = false;

    private string currentScene = "";

    private bool isInGameplay = false;

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
        // Only skip if no music playing at all
        if (explorationMusic.clip == null || !explorationMusic.isPlaying)
            return;

        // For menu music and ending music, update volume always
        if (currentMusicCategory == MusicCategory.MainMenu || currentMusicCategory == MusicCategory.Ending)
        {
            float volMultiplier = 1f;

            if (currentMusicCategory == MusicCategory.MainMenu)
                volMultiplier = mainMenuVolume;
            else if (currentMusicCategory == MusicCategory.Ending)
                volMultiplier = endingSongVolume;

            float targetVolume = volMultiplier * masterVolume;
            explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetVolume, Time.deltaTime * fadeSpeed);

            return;
        }

        if (!isInGameplay) return;

        float explorationVolMultiplier = explorationVolume;

        switch (currentMusicCategory)
        {
            case MusicCategory.MainMenu:
                explorationVolMultiplier = mainMenuVolume;
                break;
            case MusicCategory.Exploration:
                explorationVolMultiplier = explorationVolume;
                break;
            case MusicCategory.ChurchEntrance:
                explorationVolMultiplier = churchEntranceVolume;
                break;
            case MusicCategory.Ending:
                explorationVolMultiplier = endingSongVolume;
                break;
            case MusicCategory.ChurchFight:
                explorationVolMultiplier = churchFightVolume;
                break;
                // Battle handled separately below
        }

        float targetExplorationVolume = (explorationVolMultiplier * (1f - battleBlend)) * masterVolume;
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetExplorationVolume, Time.deltaTime * fadeSpeed);

        float intensityLayer0 = Mathf.Clamp01(musicIntensity);

        float battleVolMultiplier = battleLayerVolumes[0];
        if (currentMusicCategory == MusicCategory.ChurchFight)
            battleVolMultiplier *= churchFightVolume;
        else if (currentMusicCategory == MusicCategory.Battle)
            battleVolMultiplier *= explorationVolume;

        float targetBattleVolume = battleBlend * intensityLayer0 * battleVolMultiplier * masterVolume;
        battleMusicLayers[0].volume = Mathf.Lerp(battleMusicLayers[0].volume, targetBattleVolume, Time.deltaTime * fadeSpeed);

        // Uncomment and implement if you want more battle layers controlled similarly
        /*
        for (int i = 1; i < battleMusicLayers.Length; i++)
        {
            float target = battleBlend * musicIntensity * battleLayerVolumes[i] * masterVolume;
            battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, target, Time.deltaTime * fadeSpeed);
        }
        */
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        Debug.Log("Loaded scene: " + currentScene);

        // Determine if we should treat these scenes as part of a shared exploration set
        bool isExplorationSharedScene = scene.name == "01_MaskSelection" || scene.name == "02_Tutorial";

        // Do not stop music if it's already the correct clip and it's a shared exploration scene
        if (!isExplorationSharedScene)
            StopAllCoroutines();

        switch (currentScene)
        {
            case "00_MainMenu":
                PlayMenuMusic();
                isInGameplay = false;
                break;

            case "01_Tutorial":
            case "02_MaskSelection":
                PlayGameplayMusic(levelExplorationClip, levelBattleClips, preserveIfAlreadyPlaying: true);
                isInGameplay = true;
                break;

            case "03_LevelOne":
            case "04_LevelTwo":
                PlayGameplayMusic(levelExplorationClip, levelBattleClips);
                isInGameplay = true;
                break;

            case "05_Church":
                PlayGameplayMusic(churchExplorationClip, churchBattleClips);
                isInGameplay = true;
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
        if (explorationMusic.clip == menuMusicClip && explorationMusic.isPlaying)
            return;

        StopAllMusic();

        if (menuMusicClip == null) return;

        explorationMusic.clip = menuMusicClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        currentMusicCategory = MusicCategory.MainMenu;
    }

    private void PlayGameplayMusic(AudioClip explorationClip, AudioClip[] battleClips, bool preserveIfAlreadyPlaying = false)
    {
        if (preserveIfAlreadyPlaying && explorationMusic.clip == explorationClip && explorationMusic.isPlaying)
            return;

        StopAllMusic();

        if (explorationClip == null)
        {
            Debug.LogWarning("Exploration clip is null!");
            return;
        }

        if (battleClips == null || battleClips.Length == 0 || battleClips[0] == null)
        {
            Debug.LogWarning("No battle clip (layer 0) provided!");
            return;
        }

        double startTime = AudioSettings.dspTime + 0.1f;

        explorationMusic.clip = explorationClip;
        explorationMusic.volume = 0f;
        explorationMusic.PlayScheduled(startTime);

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            if (i < battleClips.Length && battleClips[i] != null)
            {
                battleMusicLayers[i].clip = battleClips[i];
                battleMusicLayers[i].volume = 0f;
                battleMusicLayers[i].PlayScheduled(startTime);
            }
        }

        if (currentScene == "05_Church")
            currentMusicCategory = MusicCategory.ChurchEntrance;
        else
            currentMusicCategory = MusicCategory.Exploration;
    }

    private void PlayEndingSong()
    {
        StopAllMusic();

        if (endingSongClip == null) return;

        explorationMusic.clip = endingSongClip;
        explorationMusic.volume = 0f;
        explorationMusic.Play();

        currentMusicCategory = MusicCategory.Ending;
    }

    private void StopAllMusic()
    {
        explorationMusic.Stop();
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            battleMusicLayers[i].Stop();
        }
    }

    private void HandleChamberActivated()
    {
        battleBlend = 1f;
        musicIntensity = 0.5f;

        if (currentScene == "05_Church")
            currentMusicCategory = MusicCategory.ChurchFight;
        else
            currentMusicCategory = MusicCategory.Battle;
    }

    private void HandleChamberFinished()
    {
        battleBlend = 0f;

        if (currentScene == "05_Church")
            currentMusicCategory = MusicCategory.ChurchEntrance;
        else
            currentMusicCategory = MusicCategory.Exploration;
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

    private void UpdateBattleMusicLayers()
    {
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            bool shouldBeActive = i <= currentComboLevel;
            float intensity = Mathf.Clamp01(musicIntensity);
            float volume = battleBlend * intensity * battleLayerVolumes[i] * masterVolume;

            // Only enable layers if they have a valid clip
            if (battleMusicLayers[i].clip != null)
            {
                if (shouldBeActive && !battleMusicLayers[i].isPlaying)
                {
                    battleMusicLayers[i].Play();
                }
                else if (!shouldBeActive && battleMusicLayers[i].isPlaying)
                {
                    battleMusicLayers[i].Pause(); // Or .Stop() if you want a hard cut
                }

                battleMusicLayers[i].volume = shouldBeActive ? volume : 0f;
            }
        }
    }

    public IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = explorationMusic.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float newVolume = Mathf.Lerp(startVolume, 0f, t);
            explorationMusic.volume = newVolume;
            yield return null;
        }

        explorationMusic.Stop();
        explorationMusic.volume = startVolume; // Reset if needed later
    }

    public void FadeOutAndLoadScene(string sceneName, float duration)
    {
        if (isFadingOut) return; // Prevent multiple calls
        isFadingOut = true;
        StartCoroutine(FadeAndLoad(sceneName, duration));
    }

    private IEnumerator FadeAndLoad(string sceneName, float duration)
    {
        yield return StartCoroutine(FadeOutMusic(duration));
        SceneManager.LoadScene(sceneName);
        isFadingOut = false; // Reset if you need to reuse this logic later
    }
}