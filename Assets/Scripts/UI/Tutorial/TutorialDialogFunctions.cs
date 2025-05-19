using ProjectColombo.LevelManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogFunctions : MonoBehaviour
    {
        public GameObject chamberSlot;
        public GameObject mommottiSlot;


        public void ActivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().ActivateChamber();
        }

        public void DeactivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().DeactivateChamber();
        }

        public void EnableMommotti()
        {
            mommottiSlot.SetActive(true);
        }

        public void EndTutorial()
        {
            SceneManager.LoadScene(3);
        }
    }
}