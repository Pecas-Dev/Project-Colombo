using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GatesSFX_Script : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip gateOpeningClip;
    public AudioClip gateOpenedClip;
    public AudioClip gateClosingClip;
    public AudioClip gateClosedClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float gateOpeningVolume = 1f;
    [Range(0f, 1f)] public float gateOpenedVolume = 1f;
    [Range(0f, 1f)] public float gateClosingVolume = 1f;
    [Range(0f, 1f)] public float gateClosedVolume = 1f;

    private AudioSource audioSource;
    private bool gameStarted = false;
    private float startTimer = 0f;
    private float startDelay = 5f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    void Start()
    {
        startTimer = 0f;
        gameStarted = false;
    }

    void Update()
    {
        if (!gameStarted)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startDelay)
            {
                gameStarted = true;
            }
        }
    }

    public void PlayGateOpening()
    {
        PlaySound(gateOpeningClip, gateOpeningVolume, true);
    }

    public void PlayGateOpened()
    {
        PlaySound(gateOpenedClip, gateOpenedVolume, false);
    }

    public void PlayGateClosing()
    {
        PlaySound(gateClosingClip, gateClosingVolume, true);
    }

    public void PlayGateClosed()
    {
        PlaySound(gateClosedClip, gateClosedVolume, false);
    }

    private void PlaySound(AudioClip clip, float volume, bool loop)
    {
        if (!gameStarted || clip == null)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play();
    }
}
