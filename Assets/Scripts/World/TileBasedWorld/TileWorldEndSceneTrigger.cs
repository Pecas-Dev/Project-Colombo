using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.LevelManagement
{
    public class TileWorldEndSceneTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

                if (nextScene >= 0 && nextScene < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextScene);
                }
                else
                { 
                    Debug.LogError("Invalid scene index: " + nextScene);
                }
            }
        }
    }
}