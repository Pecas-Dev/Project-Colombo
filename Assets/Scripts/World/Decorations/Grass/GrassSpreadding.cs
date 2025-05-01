using UnityEngine;

namespace ProjectColombo.Objects.Decorations
{
    public class GrassSpreading : MonoBehaviour
    {
        public GameObject grassPrefab;
        public float spreadRadius = 5f;
        public int instancesToSpawn = 100;
        public float minScale = 0.75f;
        public float maxScale = 1.25f;

        private void Start()
        {
            for (int i = 0; i < instancesToSpawn; i++)
            {
                Vector2 randPos = Random.insideUnitCircle * spreadRadius;
                Vector3 pos = new Vector3(randPos.x, 0, randPos.y);
                Quaternion randRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                float randScale = Random.Range(minScale, maxScale);

                GameObject grass = Instantiate(grassPrefab, transform.position + pos, randRot, transform);
                grass.transform.localScale *= randScale;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spreadRadius);
        }
    }
}
