using UnityEngine;
using ProjectColombo.Enemies.DropSystem;


namespace ProjectColombo.Combat
{
    public class HealthManager : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health for this entity.")]
        [SerializeField] int maxHealth = 100;


        [Tooltip("Maximum health for this entity.")]
        [ReadOnlyInspector] public int currentHealth;


        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public delegate void OnHealthChanged(int currentHealth, int maxHealth);
        public event OnHealthChanged HealthChanged;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damageAmount)
        {
            if (currentHealth <= 0)
            {
                return;
            }

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(currentHealth, 0); 

            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int healAmount)
        {
            if (currentHealth <= 0)
            {
                return; 
            }

            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth); 

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void AddHealthPercentage(int percentage)
        {
            maxHealth += (int)(percentage/100f * maxHealth);
            currentHealth += (int)(percentage/100f * maxHealth);
        }

        void Die()
        {
            if (TryGetComponent<DropSystem>(out _))
            {
                GetComponent<DropSystem>().DropItem();
            }

            if (gameObject.CompareTag("Destroyable"))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
