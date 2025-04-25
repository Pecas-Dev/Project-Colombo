using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Camera;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        int defaultDamage;

        float correctAttackScaleBonusPercentage;
        float blockDamageReductionPercentage;
        float missedParryPaneltyPercentage;

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
        [HideInInspector] public Animator myAnimator;
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;
        ParticleSystem myParticles;


        GlobalStats myGlobalStats;


        private void Start()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            GetCurrentStats();
            myAnimator = GetComponent<Animator>();
            isAttacking = false;
            currentTimer = 0;
            ownerTag = GetComponentInParent<HealthManager>().tag;
            myParticles = GetComponent<ParticleSystem>();
        }

        private void SaveCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                myGlobalStats.currentPlayerDamage = defaultDamage;
                myGlobalStats.currentCorrectAttackScalePercent = correctAttackScaleBonusPercentage;
                myGlobalStats.currentBlockReductionPercent = blockDamageReductionPercentage;
                myGlobalStats.currentMissedParryPaneltyPercent = missedParryPaneltyPercentage;
            }
        }

        void GetCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                defaultDamage = myGlobalStats.currentPlayerDamage;
                correctAttackScaleBonusPercentage = myGlobalStats.currentCorrectAttackScalePercent;
                blockDamageReductionPercentage = myGlobalStats.currentBlockReductionPercent;
                missedParryPaneltyPercentage = myGlobalStats.currentMissedParryPaneltyPercent;
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                defaultDamage = myGlobalStats.currentMommottiDamage;
            }
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

                if (currentScale == GameGlobals.MusicScale.MAJOR)
                {
                    mainModule.startColor = Color.green;
                }
                else if (currentScale == GameGlobals.MusicScale.MINOR)
                {
                    mainModule.startColor = Color.blue;
                }
                else
                {
                    mainModule.startColor = Color.red;
                }

                    myParticles.Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            int damage = defaultDamage;
            Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
            attackDirection.y = 0.0f; //could be increased to make the hit entity jump a bit

            if (ownerTag == "Player" && other.CompareTag("Destroyable"))
            {
                //Debug.Log("Player hit Destroyable");
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherHealth != null)
                {
                    otherHealth.TakeDamage(damage);
                }
            }
            else if (ownerTag == "Player" && other.CompareTag("Enemy"))
            {
                //Debug.Log("Player hit Enemy");
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    StopTime();
                    ScreenShake();

                    if (currentScale != otherAttributes.currentScale)
                    {
                        //Debug.Log("..with the opposite scale");
                        AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        //Debug.Log("..but not the opposite scale");
                    }

                    CustomEvents.DamageDelt(damage, currentScale, otherHealth);
                    otherHealth.TakeDamage(damage);
                    otherStateMachine.ApplyKnockback(attackDirection, knockback);
                }
            }
            else if (ownerTag == "Enemy" && other.CompareTag("Player"))
            {
                //Debug.Log("Enemy hit Player");
                PlayerStateMachine otherStateMachine = other.GetComponent<PlayerStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    StopTime();
                    ScreenShake();

                    if (otherStateMachine.isBlocking)
                    {
                        //Debug.Log("Player blocked incoming attack");

                        AddTemporaryDamagePercentage(damage, -blockDamageReductionPercentage);
                        CustomEvents.DamageBlocked(damage, currentScale, otherHealth);
                    }
                    else if (otherStateMachine.isParrying)
                    {
                        //Debug.Log("Player parried");
                        bool sameScale = true;
                        damage = 0;

                        if (currentScale != otherAttributes.currentScale)
                        {
                            //Debug.Log("..with opposite scale -> stagger enemy");
                            GetComponentInParent<MommottiStateMachine>().SetStaggered();
                            sameScale = false;
                        }
                        else
                        {
                            //Debug.Log("..but not the opposite scale");
                        }

                        CustomEvents.SuccessfullParry(currentScale, sameScale);
                    }
                    else if (otherStateMachine.tryParrying)
                    {
                        //Debug.Log("Player missed parry");
                        bool sameScale = true;

                        if (currentScale != otherAttributes.currentScale)
                        {
                            //Debug.Log("..with opposite scale -> extra damage");
                            AddTemporaryDamagePercentage(damage, missedParryPaneltyPercentage);
                            sameScale = false;
                        }

                        CustomEvents.FailedParry(damage, currentScale, otherHealth, sameScale);
                    }
                    else
                    {
                        CustomEvents.DamageReceived(damage, currentScale, otherHealth);
                    }

                    otherStateMachine.SetStaggered();
                    otherHealth.TakeDamage(damage);
                }
            }
        }

        public void AddDamagePercentage(int percentage)
        {
            defaultDamage += (int)(percentage / 100 * defaultDamage);
        }

        private void AddTemporaryDamagePercentage(int damage, float percentage)
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
