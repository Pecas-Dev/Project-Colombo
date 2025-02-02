using UnityEngine;


namespace ProjectColombo.Combat
{
    public class HealthManager : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health for this entity.")]
        [SerializeField] int maxHealth = 100;


        [Tooltip("Maximum health for this entity.")]
        [SerializeField, ReadOnlyInspector] int currentHealth;


        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public delegate void OnHealthChanged(int currentHealth, int maxHealth);
        public event OnHealthChanged HealthChanged;

        public delegate void OnDeath();
        public event OnDeath Died;

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

        void Die()
        {
            Died?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
