using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
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
                StartCoroutine(LoadNextSceneDelayed());
            }
        }

        private IEnumerator LoadNextSceneDelayed()
        {
            yield return new WaitForEndOfFrame(); // even one frame might help
            CustomEvents.LevelChanged(); //set event

            yield return new WaitForSeconds(0.2f); //let events handle first and then load next


            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.PlayCloseTransition();
            }

            yield return new WaitForSeconds(1.5f);

            int nextScene = GameObject.Find("WorldGeneration").GetComponent<LevelStats>().nextSceneNumber;

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