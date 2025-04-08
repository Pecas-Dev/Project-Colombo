using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.LevelManagement
{
    public class TileWorldEndSceneTrigger : MonoBehaviour
    {
        public int nextScene = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
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