using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ChangeSceneOnVideoEnd : MonoBehaviour
{
    public VideoPlayer cutScene;
    GameObject transitionCanvas;

    private void Start()
    {
        cutScene.loopPointReached += OnVideoEnd;

        transitionCanvas = GameObject.Find("TransitionCanvas");

    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (cutScene != null)
        {
            cutScene.loopPointReached -= OnVideoEnd;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(End());
    }

    IEnumerator End()
    {
        transitionCanvas.GetComponentInChildren<Animator>().Play("Close");

        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(7);

    }
}
