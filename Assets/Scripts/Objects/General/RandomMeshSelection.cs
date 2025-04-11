using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.Objects
{
    public class RandomMeshSelection : MonoBehaviour
    {
        public List<Mesh> listOfMeshes;

        private void Start()
        {
            int rand = Random.Range(0, listOfMeshes.Count);

            GetComponent<MeshFilter>().mesh = listOfMeshes[rand];
        }
    }
}