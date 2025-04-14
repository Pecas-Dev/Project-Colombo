using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ModularHouseRandomizer : MonoBehaviour
    {
        public List<GameObject> baseModuls;
        public float baseModuleHeight = 5;
        public List<GameObject> firstLayerModuls;
        public float firstLayerHeight = 5;
        public List<GameObject> secondLayerModuls;
        public float secondLayerHeight = 5;

        private void Start()
        {
            PlaceRandomFromList(baseModuls, 0);
            PlaceRandomFromList(firstLayerModuls, baseModuleHeight);
            PlaceRandomFromList(secondLayerModuls, baseModuleHeight + firstLayerHeight);
        }

        void PlaceRandomFromList(List<GameObject> list, float heightOffset)
        {
            int rand = Random.Range(0, list.Count);
            Vector3 pos = new(transform.position.x, transform.position.y + heightOffset, transform.position.z);
            Quaternion rot = transform.rotation;

            Instantiate(list[rand], pos, rot);
        }
    }
}