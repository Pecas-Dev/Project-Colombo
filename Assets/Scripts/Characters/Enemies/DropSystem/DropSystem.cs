using ProjectColombo.Inventory.Collectable;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Enemies.DropSystem
{
    public class DropSystem : MonoBehaviour
    {
        public List<GameObject> possibleDrops;
        public List<int> dropChances;


        private void Start()
        {
        }

        public void DropItem()
        {
            int rand = Random.Range(0, 100);

            if (dropChances.Count == 0)
            {
                Debug.Log("no drop chance values set");
                return;
            }

            for (int i = 0; i < possibleDrops.Count; i++)
            {
                if (dropChances.Count < i)
                {
                    Debug.Log("no drop chance values set");
                    break;
                }

                if (rand <= dropChances[i])
                {
                    Instantiate(possibleDrops[i], new Vector3(transform.position.x, 1f, transform.position.z), transform.rotation);
                    break;
                }

                rand -= dropChances[i];

                if (rand <= 0)
                {
                    Debug.Log("intentional no drop");
                    break;
                }
            }
        }
    }
}