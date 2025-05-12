using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectColombo.GameManagement.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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

    private AudioSource explorationMusic;
    private AudioSource[] battleMusicLayers;

    private float musicIntensity = 0f;
    private float battleBlend = 0f;

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
    }

    private void OnDestroy()
    {
        CustomEvents.OnChamberActivated -= HandleChamberActivated;
        CustomEvents.OnChamberFinished -= HandleChamberFinished;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!isInGameplay) return;

        float targetExplorationVolume = 1f - battleBlend;
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetExplorationVolume, Time.deltaTime * fadeSpeed);

        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            float intensityPerLayer = Mathf.Clamp01(musicIntensity - i * 0.25f);
            float targetVolume = battleBlend * intensityPerLayer;
            battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, targetVolume, Time.deltaTime * fadeSpeed);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        StopAllCoroutines();

        switch (currentScene)
        {
            case "00_MainMenu":
            case "01_MaskSelection":
                PlayMenuMusic();
                isInGameplay = false;
                break;

            case "02_LevelOne":
            case "03_LevelTwo":
            case "04_LevelThree":
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
        StopAllMusic();

        if (menuMusicClip == null) return;

        explorationMusic.clip = menuMusicClip;
        explorationMusic.volume = 1f;
        explorationMusic.Play();
    }

    private void PlayGameplayMusic(AudioClip explorationClip, AudioClip[] battleClips)
    {
        StopAllMusic();

        if (explorationClip == null || battleClips.Length < 4) return;

        explorationMusic.clip = explorationClip;
        explorationMusic.volume = 1f;
        explorationMusic.Play();

        for (int i = 0; i < 4; i++)
        {
            battleMusicLayers[i].clip = battleClips[i];
            battleMusicLayers[i].volume = 0f;
            battleMusicLayers[i].Play();
        }
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
    }

    private void HandleChamberFinished()
    {
        battleBlend = 0f;
    }

    public void SetBattleIntensity(float intensity)
    {
        musicIntensity = Mathf.Clamp01(intensity);
    }
}
