using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectColombo.LevelManagement
{
    public class HouseRandomizer : MonoBehaviour
    {
        public List<GameObject> buildingVariants;
        public float chanceOfInvert = 0.5f;
        private void Start()
        {
            int rand = Random.Range(0, buildingVariants.Count);
            float invert = Random.Range(0f, 1f);
            Vector3 scale = transform.parent.localScale;

            if (invert < chanceOfInvert)
            {
                scale.y *= -1;
            }

            GameObject instance = Instantiate(buildingVariants[rand], transform.position, transform.rotation, transform.parent);
            MakeColorFull(instance);
            transform.parent.localScale = scale;

            Destroy(this.gameObject);
        }

        void MakeColorFull(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            float rand = Random.Range(0f, 1f);

            foreach (Material m in renderer.materials)
            {
                if (m.HasFloat("_PlasterColor"));
                {
                    m.SetFloat("_PlasterColor", rand);
                }
            }
        }
    }
}