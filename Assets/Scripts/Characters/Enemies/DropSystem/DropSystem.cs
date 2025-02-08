using ProjectColombo.Inventory.Collectable;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Enemies.DropSystem
{
    public class DropSystem : MonoBehaviour
    {
        public List<GameObject> possibleDrops;
        //public int chanceToDrop;
        List<int> dropChancePerItem;

        private void Start()
        {
            dropChancePerItem = new();

            foreach (GameObject drop in possibleDrops)
            {
                dropChancePerItem.Add(drop.GetComponent<BaseCollectable>().myData.dropChance);
            }
        }

        public void DropItem()
        {
            int rand = Random.Range(0, 100);

            for (int i = 0; i < dropChancePerItem.Count; i++)
            {
                if (rand < dropChancePerItem[i])
                {
                    Instantiate(possibleDrops[i], new Vector3(transform.position.x, 1.5f, transform.position.z), transform.rotation);
                    break;
                }

                rand -= dropChancePerItem[i];
            }
        }
    }
}