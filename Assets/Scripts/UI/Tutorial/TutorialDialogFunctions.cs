using ProjectColombo.LevelManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogFunctions : MonoBehaviour
    {
        public GameObject chamberSlot;
        public GameObject setActiveSlot;

        public void ActivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().ActivateChamber();
        }

        public void DeactivateChamber()
        {
            chamberSlot.GetComponent<TileWorldChamber>().DeactivateChamber();
        }

        public void EndTutorial()
        {
            SceneManager.LoadScene(3);
        }

        public void SetActive()
        {
            setActiveSlot.SetActive(true);
        }
    }
}