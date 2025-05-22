using UnityEngine;

namespace ProjectColombo.Combat
{
    public class RagefullImpactSphere : MonoBehaviour
    {
        public int damage;
        public float speed;
        public float timeTillDelete;
        float timer;
        public Collider myCollider;
        bool hit = false;

        private void Start()
        {
            myCollider.enabled = true;
        }
        private void Update()
        {
            if (!hit) myCollider.enabled = true;

            timer += Time.deltaTime;

            if (timer >= timeTillDelete)
            {
                Destroy(this.gameObject);
            }

            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                hit = true;
                myCollider.enabled = false;
                other.GetComponent<HealthManager>().TakeDamage(damage);
                Destroy(this.gameObject, 1f);
            }
        }
    }
}