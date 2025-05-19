using ProjectColombo.LevelManagement;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public List<GameObject> chambers;
        public TutorialSpawner startSpawner;

        private void Start()
        {
            foreach (var chamber in chambers)
            {
                chamber.GetComponent<TileWorldChamber>().Initialize(new Vector2(0,0));
            }

            startSpawner.SpawnEnemy();
        }
    }
}