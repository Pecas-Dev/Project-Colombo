using UnityEngine;
using ProjectColombo.Enemies.DropSystem;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.LevelManagement;
using ProjectColombo.Objects.VFX;
using ProjectColombo.Objects.SFX;


namespace ProjectColombo.Combat
{
    public class HealthManager : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;
        int maxHealth = 100;
        public bool dontDie = false;

        [ReadOnlyInspector] public int currentHealth;
        [ReadOnlyInspector] public bool ignoreDamage = false;
        [ReadOnlyInspector] public bool isDead = false;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public delegate void OnHealthChanged(int currentHealth, int maxHealth);
        public event OnHealthChanged HealthChanged;

        //for damage number display
        int damageThisFrame = 0;
        bool damageAppliedThisFrame = false;


        void Awake()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
             
            if (myGlobalStats.currentPlayerHealth == 0)
            {
                myGlobalStats.ResetStats();
            }

            GameObject worldGen = GameObject.Find("WorldGeneration");
            if (worldGen != null)
            {
                myLevelStats = worldGen.GetComponent<LevelStats>();
            }

            ignoreDamage = false;
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
            else if (gameObject.CompareTag("Boss"))
            {
                maxHealth = myLevelStats.defaultBossMaxHealth;
                currentHealth = maxHealth;
            }
        }

        public bool GetIgnoreDamage()
        {
            return ignoreDamage;
        }

        public void TakeDamage(int damageAmount)
        {
            if (isDead) return;

            damageThisFrame += damageAmount;
            damageAppliedThisFrame = true;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(currentHealth, 0); 

            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (!dontDie && currentHealth <= 0)
            {
                Die();
            }
        }

        private void LateUpdate()
        {
            if (!damageAppliedThisFrame) return;

            Debug.Log("damaged event send");
            CustomEvents.DisplayDamageNumber(this.gameObject, damageThisFrame);
            damageAppliedThisFrame = false;
            damageThisFrame = 0;
        }

        public void Heal(int healAmount)
        {
            if (!dontDie && currentHealth <= 0)
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

            if (tag == "Player")
            {
                CustomEvents.MaxHealthIncreased(delta);
            }
        }

        public void AddHealthPoints(int points)
        {
            maxHealth += points;
            currentHealth += points;

            if (gameObject.CompareTag("Player"))
            {
                CustomEvents.MaxHealthIncreased(points);
            }
        }

        void Die()
        {
            if (isDead) return;
            isDead = true;

            if (TryGetComponent<DropSystem>(out _))
            {
                GetComponent<DropSystem>().DropItem();
            }

            if (TryGetComponent<SpawnVFXOnDestroy>(out _))
            {
                GetComponent<SpawnVFXOnDestroy>().SpawnVFX();
            }

            if (TryGetComponent<VaseBreak_SFX>(out _))
            {
                GetComponent<VaseBreak_SFX>().PlaySFX();
            }

            if (gameObject.CompareTag("Destroyable"))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
