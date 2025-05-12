using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Camera;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using ProjectColombo.LevelManagement;

namespace ProjectColombo.Combat
{
    public class WeaponAttributes : MonoBehaviour
    {
        public Sprite weaponSprite;
        public int defaultMajorDamage;
        public int defaultMinorDamage;

        float majorDamageMultiplier;
        float minorDamageMultiplier;

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
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;

        bool doHitstop = true;

        ParticleSystem myParticles;
        Collider myCollider;
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;


        private void Start()
        {
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            myCollider = GetComponent<Collider>();
            DisableWeaponHitbox();
            GetCurrentStats();
            isAttacking = false;
            currentTimer = 0;
            ownerTag = GetComponentInParent<HealthManager>().tag;
            myParticles = GetComponent<ParticleSystem>();
        }



        void GetCurrentStats()
        {
            if (GetComponentInParent<EntityAttributes>().CompareTag("Player"))
            {
                Debug.Log("set weapon stats");
                majorDamageMultiplier = myGlobalStats.currentMajorDamageMultiplyer;
                minorDamageMultiplier = myGlobalStats.currentMinorDamageMultiplyer;
                correctAttackScaleBonusPercentage = myGlobalStats.currentCorrectAttackScalePercent;
                blockDamageReductionPercentage = myGlobalStats.currentBlockReductionPercent;
                missedParryPaneltyPercentage = myGlobalStats.currentMissedParryPaneltyPercent;
            }
            else if (GetComponentInParent<EntityAttributes>().CompareTag("Enemy"))
            {
                myLevelStats.ResetStats();
                defaultMinorDamage = myLevelStats.currentMommottiDamage;
                defaultMajorDamage = myLevelStats.currentMommottiDamage;
                majorDamageMultiplier = 1;
                minorDamageMultiplier = 1;


                blockDamageReductionPercentage = myGlobalStats.currentBlockReductionPercent;
                missedParryPaneltyPercentage = myGlobalStats.currentMissedParryPaneltyPercent;
            }
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

        public void EnableWeaponHitbox()
        {
            myCollider.enabled = true;
            doHitstop = true;
        }

        public void DisableWeaponHitbox()
        {
            myCollider.enabled = false;
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
            int damage = currentScale == GameGlobals.MusicScale.MAJOR ? (int)(defaultMajorDamage * majorDamageMultiplier) : (int)(defaultMinorDamage * minorDamageMultiplier);
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

                if (otherHealth.GetIgnoreDamage()) return;

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (doHitstop)
                    {
                        StopTime();
                        ScreenShake();
                        doHitstop = false;
                    }

                    if (currentScale != otherAttributes.currentScale)
                    {
                        //Debug.Log("..with the opposite scale");
                        damage = AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        //Debug.Log("..but not the opposite scale");
                    }

                    CustomEvents.DamageDelt(damage, currentScale, otherHealth);
                    otherHealth.TakeDamage(damage);
                    otherStateMachine.ApplyKnockback(attackDirection, knockback, currentScale);
                }
            }
            else if (ownerTag == "Enemy" && other.CompareTag("Player"))
            {
                //Debug.Log("Enemy hit Player" + damage);
                PlayerStateMachine otherStateMachine = other.GetComponent<PlayerStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherHealth.GetIgnoreDamage()) return;

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (doHitstop)
                    {
                        StopTime();
                        ScreenShake();
                        doHitstop = false;
                    }

                    if (otherStateMachine.isBlocking)
                    {
                        Debug.Log("Player blocked incoming attack");
                        damage = AddTemporaryDamagePercentage(damage, -blockDamageReductionPercentage);
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
                            damage = AddTemporaryDamagePercentage(damage, missedParryPaneltyPercentage);
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

        public void AddDamagePercentage(int percentage, GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                majorDamageMultiplier += percentage / 100f;
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                minorDamageMultiplier += percentage / 100f;
            }
            else
            {
                Debug.Log("no scale set in add damage");
            }
        }

        private int AddTemporaryDamagePercentage(int damage, float percentage)
        {
            return damage + (int)(percentage / 100 * damage);
        }

        private void ScreenShake()
        {
            FindFirstObjectByType<ScreenShakeManager>().Shake(0.2f);
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
