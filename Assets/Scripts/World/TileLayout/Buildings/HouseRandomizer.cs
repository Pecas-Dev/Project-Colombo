using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectColombo.LevelManagement
{
    public class HouseRandomizer : MonoBehaviour
    {
        public List<Mesh> buildingVariants;

        private void Start()
        {
            int rand = Random.Range(0, buildingVariants.Count);

            GetComponent<MeshFilter>().mesh = buildingVariants[rand];
        }
    }
}