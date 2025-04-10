using ProjectColombo.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectColombo.Enemies.UI
{
    public class EnemyHealthbar : MonoBehaviour
    {
        public Vector3 offset;
        HealthManager myHealthManager;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            myHealthManager = GetComponentInParent<HealthManager>();
            GetComponent<RectTransform>().position = GetComponentInParent<EntityAttributes>().gameObject.transform.position + offset;
        }

        public void UpdateHealthbar(float current, float max)
        {
            if (current <= 0)
            {
                Destroy(this.gameObject);
            }

            float value = current / max;
            GetComponent<Slider>().value = value;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHealthbar(myHealthManager.currentHealth, myHealthManager.MaxHealth);
            GetComponent<RectTransform>().position = GetComponentInParent<EntityAttributes>().gameObject.transform.position + offset;
            transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }
    }
}