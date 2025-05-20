using ProjectColombo.LevelManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ProjectColombo.Tutorial
{
    public class TutorialDialogFunctions : MonoBehaviour
    {
        public GameObject chamberSlot;
        public List<GameObject> setActiveSlot;
        public List<GameObject> setInactiveSlot;
        public GameInputSystem.InputActionType[] input;

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
            foreach (var g in setActiveSlot)
            {
                g.SetActive(true);
            }
        }

        public void SetInactive()
        {
            foreach (var g in setInactiveSlot)
            {
                g.SetActive(false);
            }
        }

        public void EnableInput()
        {
            TutorialManager tm = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();

            tm.allowedInputs = input;
        }
    }
}
