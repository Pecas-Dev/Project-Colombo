using UnityEngine;
using ProjectColombo.Core;

namespace ProjectColombo.Combat
{
    public class Fight : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] float attackRange = 2.0f;
        [SerializeField] float attackDamage = 20.0f;
        [SerializeField] float attackCooldown = 1.5f;

        private Health health;
        private float timeSinceLastAttack = Mathf.Infinity;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        public void Attack()
        {
            if (timeSinceLastAttack < attackCooldown) return;

            // Find the nearest enemy within attack range
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hit in hits)
            {
                Health targetHealth = hit.GetComponent<Health>();
                if (targetHealth != null && !targetHealth.GetIsDead() && hit.gameObject != this.gameObject)
                {
                    // Face the target
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(direction);

                    // Trigger attack animation
                    GetComponent<Animator>().SetTrigger("attack");
                    timeSinceLastAttack = 0f;
                    return;
                }
            }
        }

        // Animation Event
        private void Hit()
        {
            // Deal damage to the target
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackRange * 0.5f, attackRange * 0.5f);
            foreach (var hit in hits)
            {
                Health targetHealth = hit.GetComponent<Health>();
                if (targetHealth != null && !targetHealth.GetIsDead() && hit.gameObject != this.gameObject)
                {
                    targetHealth.TakeDamage(attackDamage);
                    break; // Only hit one target
                }
            }
        }
    }
}
