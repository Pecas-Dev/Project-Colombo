using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.UI.Pausescreen;
using System.Collections;
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
                GameManager.Instance.pauseMenuUI.GetComponent<PauseMenuUI>().ResetSelection();
                StartCoroutine(LoadNextSceneDelayed());
            }
        }

        private IEnumerator LoadNextSceneDelayed()
        {
            CustomEvents.LevelChanged(); //set event

            yield return new WaitForSeconds(0.2f); //let events handle first and then load next

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