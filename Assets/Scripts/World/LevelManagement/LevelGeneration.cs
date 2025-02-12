using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class LevelGeneration : MonoBehaviour
    {
        public List<GameObject> chamberVariants;
        public List<GameObject> corridorVariants;
        public GameObject startChamberExit;
        public GameObject endChamber;
        List<GameObject> currentOpenCorridors;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startChamberExit.GetComponent<MeshRenderer>().enabled = false;
            startChamberExit.GetComponent<BoxCollider>().enabled = false;

            currentOpenCorridors = new();

            //get random chamber order
            List<int> chamberOrder = new();

            for (int i = 0; i < chamberVariants.Count; i++)
            {
                chamberOrder.Add(i);
            }

            chamberOrder = chamberOrder.OrderBy(x => Random.value).ToList();

            //add corridor to start chamber
            currentOpenCorridors.Add(AddCorridorToExit(startChamberExit));

            while (chamberOrder.Count > 0)
            {
                AddChamberToCorridor(currentOpenCorridors[0], chamberOrder[0]);
                currentOpenCorridors.RemoveAt(0);
                chamberOrder.RemoveAt(0);
            }

            foreach (GameObject corridorEnd in currentOpenCorridors)
            {
                AddEndChamber(corridorEnd);
            }
        }

        private GameObject AddCorridorToExit(GameObject exit)
        {
            int rand = Random.Range(0, corridorVariants.Count);

            GameObject corridor = Instantiate(corridorVariants[rand], transform);
            corridor.GetComponent<ChamberData>().SetPosition(exit.transform);
            corridor.GetComponent<ChamberData>().DeactivateChamber();

            return corridor.GetComponent<ChamberData>().exits[0];
        }

        private void AddChamberToCorridor(GameObject exit, int index)
        {
            GameObject chamber = Instantiate(chamberVariants[index], transform);
            chamber.GetComponent<ChamberData>().SetPosition(exit.transform);

            foreach (GameObject chamberExit in chamber.GetComponent<ChamberData>().exits)
            {
                currentOpenCorridors.Add(AddCorridorToExit(chamberExit));
            }
        }

        private void AddEndChamber(GameObject exit)
        {
            GameObject chamber = Instantiate(endChamber, transform);
            chamber.GetComponent<ChamberData>().SetPosition(exit.transform);
        }
    }
}