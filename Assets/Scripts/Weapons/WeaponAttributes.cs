using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using System.ComponentModel;
using ProjectColombo.StateMachine.Player;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public int minDamage;
        public int maxDamage;
        public int additionalDamageOnHeavyAttack;
        public float knockback;
        public float cooldown;
        float currentTimer;
        public float reach;
        [SerializeField, ReadOnlyInspector] string ownerTag;
        [HideInInspector] public bool onCooldown;
        [HideInInspector] public bool isAttacking;
        [HideInInspector] public bool heavyAttack;
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

                var mainModule = myParticles.main;
                mainModule.startColor = heavyAttack ? Color.red : Color.green;

                myParticles.Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            int damage = Random.Range(minDamage, maxDamage);
            Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
            attackDirection.y = 0.2f; //could be increased to make the hit entity jump a bit

            if ((ownerTag == "Enemy" && other.tag == "Player"))
            {
                PlayerStateMachine otherStateMachine = other.GetComponent<PlayerStateMachine>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (heavyAttack) //knockback only on heavy attacks
                    {
                        otherStateMachine.Impact(attackDirection, knockback);
                        otherHealth.TakeDamage(additionalDamageOnHeavyAttack);
                    }
                    else
                    {
                        otherStateMachine.Impact(attackDirection, 0); //no knockback but flinch
                    }

                    otherHealth.TakeDamage(damage);
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
            else if (ownerTag == "Player" && other.tag == "Destroyable")
            {
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherHealth != null)
                {
                    otherHealth.TakeDamage(damage);
                }
            }
        }

        public void AddDamagePercentage(int percentage)
        {
            minDamage += (int)(percentage / 100 * minDamage);
            maxDamage += (int)(percentage / 100 * maxDamage);
        }
    }
}