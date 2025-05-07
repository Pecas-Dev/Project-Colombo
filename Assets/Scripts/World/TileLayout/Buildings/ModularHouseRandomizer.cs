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
        float randf = 0;

        private void Start()
        {
            randf = Random.Range(0f, 1f);

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

            GameObject module = Instantiate(list[rand], transform);
            module.transform.SetParent(transform, false); // false keeps local transform intact
            module.transform.localPosition = new Vector3(0, currentHeightOffset, 0);
            module.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            module.AddComponent<BoxCollider>();
            module.layer = 6;
            MakeColorFull(module);

            currentHeightOffset += heightOffset;
        }

        void MakeColorFull(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            foreach (Material m in renderer.materials)
            {
                if (m.HasFloat("_PlasterColor")) ;
                {
                    m.SetFloat("_PlasterColor", randf);
                }
            }
        }
    }
}



//_GradientPick