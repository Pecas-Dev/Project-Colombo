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
            transform.position = offset;
        }

        public void UpdateHealthbar(float current, float max)
        {
            float value = current / max;
            GetComponent<Slider>().value = value;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHealthbar(myHealthManager.currentHealth, myHealthManager.MaxHealth);
            transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }
    }
}