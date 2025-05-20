using ProjectColombo.LevelManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogFunctions : MonoBehaviour
    {
        public GameObject chamberSlot;
        public GameObject setActiveSlot;
        public GameInputSystem.InputActionType input;

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

        public void EnableInput()
        {
            TutorialManager tm = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();

            GameInputSystem.InputActionType[] current = tm.allowedInputs;

            // Check if the input is already enabled to avoid duplicates
            if (current.Contains(input))
                return;

            // Create a new array with one extra slot
            GameInputSystem.InputActionType[] updated = new GameInputSystem.InputActionType[current.Length + 1];
            current.CopyTo(updated, 0);
            updated[updated.Length - 1] = input;

            tm.allowedInputs = updated;
        }

    }
}
