using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.Camera;

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
        public float distanceToActivateForwardImpulse = 3f;     // player should be close already
        public float maxDistanceAfterImpulse = 1f;              // furthest away
        public float minDistanceAfterImpulse = .5f;             // closest
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
            Debug.Log("weapon hit from: " + ownerTag + "to: " + other.tag);

            int damage = Random.Range(minDamage, maxDamage);
            bool shouldStagger = false;
            float knockbackStrength = 0;
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
                        StopTime(); //hitstop if successfull parry
                        GetComponentInParent<MommottiStateMachine>().Impact(attackDirection, 0); // stagger enemy if parry successfull
                        return;
                    }

                    if (heavyAttack) //knockback only on heavy attacks and additional damage
                    {
                        shouldStagger = true;
                        knockbackStrength = knockback;
                        damage += additionalDamageOnHeavyAttack;
                        ScreenShake();
                    }
                    else
                    {
                        shouldStagger = true;
                        knockbackStrength = 0;
                    }

                    if (otherStateMachine.tryParrying) //penalty for parrying to early
                    {
                        damage += additionalDamageMissedParry;
                    }


                    otherHealth.TakeDamage(damage);

                    if (shouldStagger)
                    {
                        otherStateMachine.Impact(attackDirection, knockbackStrength);
                    }
                    return;
                }
            }
            else if (ownerTag == "Player" && other.tag == "Enemy")
            {
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();


                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    StopTime();
                    ScreenShake();

                    otherHealth.TakeDamage(damage);
                    otherStateMachine.Impact(attackDirection, knockback);
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

        private void ScreenShake()
        {
            FindFirstObjectByType<ScreenShakeManager>().Shake(0.5f);
        }


        public void StopTime()
        {
            StartCoroutine(DoHitStop());
        }

        IEnumerator DoHitStop()
        {
            float pauseDuration = 0.1f; // Adjust the duration of the freeze

            Time.timeScale = 0.1f; // Slow down time instead of freezing completely
            yield return new WaitForSecondsRealtime(pauseDuration);
            Time.timeScale = 1f; // Resume normal time
        }
    }

}
