using UnityEngine;

namespace ProjectColombo.Core
{
    public class Health : MonoBehaviour
    {
        [Header("Values")]
        [SerializeField] float health = 100.0f;

        bool isDead = false;

        public void TakeDamage(float damage)
        {
            health = Mathf.Max(health - damage, 0);

            if (health <= 0.0f)
            {
                Die();
            }
        }

        void Die()
        {
            if(isDead == false)
            {
                isDead = true;
                GetComponent<Animator>().SetTrigger("death");
                GetComponent<ActionSchedueler>().CancelCurrentAction();
            }
        }

        public bool GetIsDead()
        {
            return isDead;
        }

        public float GetCurrentHealth()
        {
            return health;
        }
    }
}

