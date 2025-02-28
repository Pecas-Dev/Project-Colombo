using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using System.ComponentModel;
using ProjectColombo.StateMachine.Player;
using UnityEngine.UIElements;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public int minDamage;
        public int maxDamage;
        public int additionalDamageOnHeavyAttack;
        public int additionalDamageMissedParry;
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
                    myAnimator.ResetTrigger("Interrupt");
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
            attackDirection.y = 0.0f; //could be increased to make the hit entity jump a bit

            if (ownerTag == "Enemy" && other.CompareTag("Player"))
            {
                PlayerStateMachine otherStateMachine = other.GetComponent<PlayerStateMachine>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0) 
                {
                    if (otherStateMachine.isInvunerable) return; //if player rolling

                    if (otherStateMachine.isParrying) //if player parrying successfully
                    {
                        GetComponentInParent<MommottiStateMachine>().Impact(attackDirection, 0); // stagger enemy if parry successfull
                        return;
                    }

                    if (heavyAttack) //knockback only on heavy attacks and additional damage
                    {
                        otherStateMachine.Impact(attackDirection, knockback);
                        damage += additionalDamageOnHeavyAttack;
                    }
                    else
                    {
                        otherStateMachine.Impact(attackDirection, 0); //no knockback but flinch
                    }

                    if (otherStateMachine.tryParrying) //penalty for parrying to early
                    {
                        damage += additionalDamageMissedParry;
                    }

                    otherHealth.TakeDamage(damage);
                    return;
                }
            }
            else if (ownerTag == "Player" && other.tag == "Enemy")
            {
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    //TODO: pause a quick second
                    //if stagger add screenshake ?!?

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