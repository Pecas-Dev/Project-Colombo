using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Objects
{
    public class RandomMeshSelection : MonoBehaviour
    {
        public List<GameObject> listOfObjects;

        private void Start()
        {
            int rand = Random.Range(0, listOfObjects.Count);

            Instantiate(listOfObjects[rand], transform.position, transform.rotation, transform.parent);
            Destroy(this.gameObject);
        }
    }
}