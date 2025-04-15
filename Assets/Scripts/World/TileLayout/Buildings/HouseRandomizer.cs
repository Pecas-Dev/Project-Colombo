using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectColombo.LevelManagement
{
    public class HouseRandomizer : MonoBehaviour
    {
        public List<GameObject> buildingVariants;

        private void Start()
        {
            int rand = Random.Range(0, buildingVariants.Count);

            Instantiate(buildingVariants[rand], transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
    }
}