using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public int damage;
        public float knockback;
        public float cooldown;
        float currentTimer;
        public float reach;
        //public Collider hitbox;
        [HideInInspector] public bool onCooldown;
        [HideInInspector] public bool isAttacking;
        [HideInInspector] public Animator myAnimator;

        private void Start()
        {
            myAnimator = GetComponent<Animator>();
            isAttacking = false;
            currentTimer = 0;
            //hitbox.enabled = cooldownDone;
        }

        private void Update()
        {
            //hitbox.enabled = cooldownDone;

            if (onCooldown)
            {
                currentTimer += Time.deltaTime;

                if (currentTimer >= cooldown)
                {
                    onCooldown = false;
                    currentTimer = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
            attackDirection.y = 0.2f; //could be increased to make the hit entity jump a bit

            if ((transform.parent.tag == "Enemy" && other.tag == "Player"))
            {
                HealthManager targetHealth = other.GetComponent<HealthManager>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);

                    Rigidbody targetRigidbody = other.GetComponent<Rigidbody>();

                    if (targetRigidbody != null)
                    {
                        targetRigidbody.AddForce(attackDirection * knockback, ForceMode.Impulse);
                    }
                }
            }
            else if (transform.parent.tag == "Player" && other.tag == "Enemy")
            {
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    otherStateMachine.Impact(attackDirection, knockback);
                    otherHealth.TakeDamage(damage);
                }
            }
        }
    }
}