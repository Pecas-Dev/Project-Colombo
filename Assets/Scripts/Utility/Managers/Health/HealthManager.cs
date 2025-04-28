using UnityEngine;
using ProjectColombo.Enemies.DropSystem;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.LevelManagement;


namespace ProjectColombo.Combat
{
    public class HealthManager : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;
        int maxHealth = 100;

        [ReadOnlyInspector] public int currentHealth;


        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public delegate void OnHealthChanged(int currentHealth, int maxHealth);
        public event OnHealthChanged HealthChanged;

        void Awake()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            GetCurrentStats();
        }

        private void OnDestroy()
        {
            CustomEvents.OnLevelChange -= SaveCurrentStats;
        }

        private void SaveCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                myGlobalStats.currentPlayerMaxHealth = maxHealth;
                myGlobalStats.currentPlayerHealth = currentHealth;
            }
        }

        void GetCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                maxHealth = myGlobalStats.currentPlayerMaxHealth;
                currentHealth = myGlobalStats.currentPlayerHealth;
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                maxHealth = myLevelStats.currentMommottiMaxHealth;
                currentHealth = maxHealth;
            }
            else if (gameObject.CompareTag("Destroyable"))
            {
                maxHealth = currentHealth = 1;
            }
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
            int delta = (int)(percentage / 100f * maxHealth);
            maxHealth += delta;
            currentHealth += delta;
        }

        public void AddHealthPoints(int points)
        {
            maxHealth += points;
            currentHealth += points;
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
