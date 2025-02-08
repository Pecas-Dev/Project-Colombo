using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using System.ComponentModel;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public int damage;
        public float knockback;
        public float cooldown;
        float currentTimer;
        public float reach;
        [SerializeField, ReadOnlyInspector] string ownerTag;
        [HideInInspector] public bool onCooldown;
        [HideInInspector] public bool isAttacking;
        [HideInInspector] public Animator myAnimator;
        ParticleSystem myParticles;

        private void Start()
        {
            myAnimator = GetComponent<Animator>();
            isAttacking = false;
            currentTimer = 0;
            ownerTag = GetComponentInParent<HealthManager>().tag;
            myParticles = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
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

        public void Telegraphing()
        {
            if (myParticles != null)
            {
                myParticles.Stop();
                myParticles.Clear();
                myParticles.Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
            attackDirection.y = 0.2f; //could be increased to make the hit entity jump a bit

            if ((ownerTag == "Enemy" && other.tag == "Player"))
            {
                Debug.Log("hit player");
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
            else if (ownerTag == "Player" && other.tag == "Enemy")
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