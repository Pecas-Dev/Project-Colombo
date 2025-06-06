using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ChangeSceneOnVideoEnd : MonoBehaviour
{
    public VideoPlayer cutScene;

    private void Start()
    {
        if (cutScene != null)
        {
            cutScene.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("VideoPlayer not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (cutScene != null)
        {
            cutScene.loopPointReached -= OnVideoEnd;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // Load the scene when the video ends
        SceneManager.LoadScene(7);
    }
}
