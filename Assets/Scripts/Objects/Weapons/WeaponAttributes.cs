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

        public int correctAttackScaleBonusPercentage = 10;
        public int blockDamageReductionPercentage = 10;
        public int missedParryPaneltyPercentage = 10;

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
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;
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

            if (ownerTag == "Player" && other.CompareTag("Destroyable"))
            {
                Debug.Log("Player hit Destroyable");
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherHealth != null)
                {
                    otherHealth.TakeDamage(damage);
                }
            }
            else if (ownerTag == "Player" && other.CompareTag("Enemy"))
            {
                Debug.Log("Player hit Enemy");
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    StopTime();
                    ScreenShake();

                    if (currentScale != otherAttributes.currentScale)
                    {
                        Debug.Log("..with the opposite scale");
                        AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        Debug.Log("..but not the opposite scale");
                    }

                    otherHealth.TakeDamage(damage);
                    otherStateMachine.Impact(attackDirection, knockback);
                }
            }
            else if (ownerTag == "Enemy" && other.CompareTag("Player"))
            {
                Debug.Log("Enemy hit Player");
                PlayerStateMachine otherStateMachine = other.GetComponent<PlayerStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    StopTime();
                    ScreenShake();

                    if (otherStateMachine.isBlocking)
                    {
                        Debug.Log("Player blocked incoming attack");

                        AddTemporaryDamagePercentage(damage, -blockDamageReductionPercentage);
                    }
                    else if (otherStateMachine.isParrying)
                    {
                        Debug.Log("Player parried");

                        damage = 0;

                        if (currentScale != otherAttributes.currentScale)
                        {
                            Debug.Log("..with opposite scale -> stagger enemy");
                            GetComponentInParent<MommottiStateMachine>().Impact(attackDirection, 0); //no knockback but stagger
                        }
                        else
                        {
                            Debug.Log("..but not the opposite scale");
                        }
                    }
                    else if (otherStateMachine.tryParrying)
                    {
                        Debug.Log("Player missed parry");

                        if (currentScale != otherAttributes.currentScale)
                        {
                            Debug.Log("..with opposite scale -> extra damage");
                            AddTemporaryDamagePercentage(damage, missedParryPaneltyPercentage);
                        }
                    }

                    otherHealth.TakeDamage(damage);
                }
            }
        }

        public void AddDamagePercentage(int percentage)
        {
            minDamage += (int)(percentage / 100 * minDamage);
            maxDamage += (int)(percentage / 100 * maxDamage);
        }

        private void AddTemporaryDamagePercentage(int damage, int percentage)
        {
            damage += (int)(percentage / 100 * damage);
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
