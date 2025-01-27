using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ChamberRandomizer : MonoBehaviour
    {
        public List<GameObject> chamberVariants;
        public int numberOfChambers;
        public bool allowMultipleOfSameChamber = false;
        public List<int> selectedChambersIndex;
        public GameObject wakeUpExit;

        int currentActiveChamber = 0;
        List<GameObject> chamberOrder;

        private void Start()
        {
            chamberOrder = new List<GameObject>();

            if (chamberVariants.Count < numberOfChambers && !allowMultipleOfSameChamber)
            {
                Debug.Log("not enough chamber variants! Turned on multiple of same kind!");
                allowMultipleOfSameChamber = true;
            }

            while (chamberOrder.Count < numberOfChambers)
            {
                int rand = Random.Range(0, chamberVariants.Count);

                // Add only if duplicates are allowed or if it's not already in the list
                if (allowMultipleOfSameChamber || !selectedChambersIndex.Contains(rand))
                {
                    GameObject selectedChamber = GameObject.Instantiate(chamberVariants[rand]);
                    selectedChambersIndex.Add(rand);
                    selectedChamber.GetComponent<ChamberBehaviour>().myRandomizer = this;
                    chamberOrder.Add(selectedChamber);
                }
            }

            chamberOrder[0].GetComponent<ChamberBehaviour>().SetPosition(wakeUpExit.transform.position, wakeUpExit.transform.rotation);

            for (int i = 1; i < chamberOrder.Count; i++)
            {
                Vector3 lastExitPosition = chamberOrder[i - 1].GetComponent<ChamberBehaviour>().exit.transform.position;
                Quaternion lastExitRotation = chamberOrder[i - 1].GetComponent<ChamberBehaviour>().exit.transform.rotation;
                chamberOrder[i].GetComponent<ChamberBehaviour>().SetPosition(lastExitPosition, lastExitRotation);
            }

            wakeUpExit.SetActive(false);
            currentActiveChamber = -1; //because it needs to activate the first one ;)
            ActivateNextChamber();
        }

        public void ActivateNextChamber()
        {
            currentActiveChamber++;

            if (currentActiveChamber >= numberOfChambers)
            {
                Debug.Log("last chamber completed");
                //next scene
            }
            else
            {
                chamberOrder[currentActiveChamber].GetComponent<ChamberBehaviour>().Activate();
            }
        }
    }
}