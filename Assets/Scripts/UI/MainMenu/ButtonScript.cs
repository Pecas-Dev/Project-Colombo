using ProjectColombo.GameManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ProjectColombo.UI
{
    public class ButtonScript : MonoBehaviour
    {
        public void ChangeScene(int targetScene)
        {
            if (targetScene >= 0 && targetScene < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.LogError("Invalid scene index: " + targetScene);
            }
        }

        public void ResumeGame()
        {
            GameManager.Instance.ResumeGame();
        }
    }
}