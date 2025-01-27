using System.Collections.Generic;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ChamberBehaviour : MonoBehaviour
    {
        public List<GameObject> spawner;
        public GameObject entrance;
        public GameObject exit;
        public GridManager chamberGrid;

        [HideInInspector] public ChamberRandomizer myRandomizer;
        public GameObject[] activeEnemies;
        bool active = false;
        float checkDefeatedEnemiesIntervall = 1f;
        float timer = 0;

        private void Awake()
        {
            entrance.SetActive(false);
            exit.SetActive(true);

            foreach (GameObject s in spawner)
            {
                s.GetComponent<MommottiSpawnPoint>().myGridManager = chamberGrid;
                s.SetActive(false);
            }
        }

        private void Update()
        {
            if (active)
            {
                timer += Time.deltaTime;

                if (timer >= checkDefeatedEnemiesIntervall)
                {
                    activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                    if (activeEnemies.Length == 0)
                    {
                        active = false;
                        exit.SetActive(false);
                        myRandomizer.ActivateNextChamber();
                    }

                    timer = 0;
                }
            }
        }

        public void SetPosition(Vector3 exitPositionOfLastChamber, Quaternion exitRotationOfLastChamber)
        {
            Quaternion entranceRotation = entrance.transform.rotation;
            Quaternion rotationAdjustment = exitRotationOfLastChamber * Quaternion.Inverse(entranceRotation);

            transform.rotation = rotationAdjustment * transform.rotation;

            Vector3 differenceToEntrance = transform.position - entrance.transform.position;
            transform.position = exitPositionOfLastChamber + differenceToEntrance;
        }

        public void Activate()
        {
            active = true;

            foreach (GameObject s in spawner)
            {
                s.SetActive(true);
            }
        }
    }
}