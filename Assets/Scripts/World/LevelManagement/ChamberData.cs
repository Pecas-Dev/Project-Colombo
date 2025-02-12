using NUnit.Framework.Internal;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ChamberData : MonoBehaviour
    {
        public List<GameObject> entrances;
        public List<GameObject> exits;
        public List<GameObject> spawners;
        public GridManager chamberGrid;
        bool isActive;
        float timer = 0;
        float checkDefeatedEnemiesIntervall = 1f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            //hide arrows in entrances and exits
            foreach (GameObject e in entrances)
            {
                e.GetComponent<MeshRenderer>().enabled = false;
                e.GetComponent<BoxCollider>().enabled = false;
            }

            foreach (GameObject e in exits)
            {
                e.GetComponent<MeshRenderer>().enabled = false;
            }


            //set all spawners inactive until the chamber is reached
            foreach (GameObject s in spawners)
            {
                if (chamberGrid != null) 
                { 
                    s.GetComponent<MommottiSpawnPoint>().myGridManager = chamberGrid; 
                }

                s.SetActive(false);
            }

            timer = 0;
        }

        void Update()
        {
            if (isActive)
            {
                timer += Time.deltaTime;

                if (timer >= checkDefeatedEnemiesIntervall)
                {
                    GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                    if (activeEnemies.Length == 0)
                    {
                        isActive = false;
                        DeactivateChamber();
                    }

                    timer = 0;
                }
            }
        }

        public void SetPosition(Transform lastChamberExit)
        {
            //get random entrace
            int rand = Random.Range(0, entrances.Count);
            GameObject entrance = entrances[rand];

            //set rotation
            Quaternion entranceRotation = entrance.transform.rotation;
            Quaternion rotationAdjustment = lastChamberExit.rotation * Quaternion.Inverse(entranceRotation);
            transform.rotation = rotationAdjustment * transform.rotation;

            //set position
            Vector3 differenceToEntrance = transform.position - entrance.transform.position;
            transform.position = lastChamberExit.position + differenceToEntrance;
        }

        public void ActivateSpawners()
        {
            isActive = true;

            foreach (GameObject s in spawners)
            {
                s.SetActive(true);
            }
        }

        public void LockEntrance()
        {
            foreach (GameObject e in entrances)
            {
                e.GetComponent<BoxCollider>().enabled = true; 
            }
        }

        public void DeactivateChamber()
        {
            foreach (GameObject e in exits)
            {
                e.SetActive(false);
                e.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
}