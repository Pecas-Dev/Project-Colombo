using UnityEngine;
using ProjectColombo.GameManagement.Events;

public class MusicChanger : MonoBehaviour
{
    public static MusicChanger Instance;

    public AudioSource explorationMusic;
    public AudioSource[] battleMusicLayers; // 4 variations of Battle Theme

    [Range(0f, 1f)] public float musicIntensity = 0f; // Controls battle mix
    [Range(0f, 1f)] public float battleBlend = 0f; // Blends the musics: 0 = exploration, 1 = battle

    public float fadeSpeed = 1f; // Fade speed for transitions

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        explorationMusic.volume = 1f;
        explorationMusic.Play();

        foreach (var layer in battleMusicLayers)
        {
            layer.volume = 0f;
            layer.Play();
        }

        battleBlend = 0f;
        musicIntensity = 0f;

        // Subscribe to chamber events
        CustomEvents.OnChamberActivated += HandleChamberActivated;
        CustomEvents.OnChamberFinished += HandleChamberFinished;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        CustomEvents.OnChamberActivated -= HandleChamberActivated;
        CustomEvents.OnChamberFinished -= HandleChamberFinished;
    }

    // Update is called once per frame
    void Update()
    {
        // Fade exploration music out/in based on battleBlend
        float targetExplorationVolume = 1f - battleBlend;
        explorationMusic.volume = Mathf.Lerp(explorationMusic.volume, targetExplorationVolume, Time.deltaTime * fadeSpeed);

        // Fade in battle layers based on intensity and blend
        for (int i = 0; i < battleMusicLayers.Length; i++)
        {
            float intensityPerLayer = Mathf.Clamp01(musicIntensity - i * 0.25f);
            float targetVolume = battleBlend * intensityPerLayer;
            battleMusicLayers[i].volume = Mathf.Lerp(battleMusicLayers[i].volume, targetVolume, Time.deltaTime * fadeSpeed);
        }
    }

    // Call this to enter battle mode
    private void HandleChamberActivated()
    {
        // Start battle music transition
        battleBlend = 1f;
        musicIntensity = 0.5f; // Optional default intensity
    }

    private void HandleChamberFinished()
    {
        // Return to exploration music
        battleBlend = 0f;
    }
}
