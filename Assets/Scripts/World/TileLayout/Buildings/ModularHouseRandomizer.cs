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

        public int minNumberOfMiddleModules = 0;
        public int maxNumberOfMiddleModules = 3;
        float currentHeightOffset = 0;

        private void Start()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            int rand = Random.Range(minNumberOfMiddleModules, maxNumberOfMiddleModules + 1);

            PlaceRandomFromList(baseModuls, baseModuleHeight);

            for (int i = 0; i < rand; i++)
            {
                PlaceRandomFromList(firstLayerModuls, firstLayerHeight);
            }

            PlaceRandomFromList(secondLayerModuls, currentHeightOffset);
        }

        void PlaceRandomFromList(List<GameObject> list, float heightOffset)
        {
            int rand = Random.Range(0, list.Count);

            GameObject module = Instantiate(list[rand]);
            module.transform.SetParent(transform, false); // false keeps local transform intact
            module.transform.localPosition = new Vector3(0, currentHeightOffset, 0);
            module.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            module.AddComponent<BoxCollider>();
            module.layer = 6;

            currentHeightOffset += heightOffset;
        }
    }
}