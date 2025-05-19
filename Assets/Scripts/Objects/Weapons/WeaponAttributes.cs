using UnityEngine;
using System.Collections;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Camera;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.GameManagement;
using ProjectColombo.LevelManagement;
using UnityEngine.VFX;
using System.Collections.Generic;
using ProjectColombo.Inventory;
using UnityEngine.InputSystem;

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

        public float stunArea = 2f;

        [SerializeField, ReadOnlyInspector] string ownerTag;
        [HideInInspector] public bool onCooldown;
        [HideInInspector] public bool isAttacking;
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;

        bool doHitstop = true;
        List<GameObject> hitObjects = new();

        ParticleSystem myParticles;
        Collider myCollider;
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;
        PlayerInventory myPlayerInventory;

        public VisualEffect majorVFX;
        public VisualEffect minorVFX;
        public VisualEffect majorParryVFX;
        public VisualEffect minorParryVFX;
        public VisualEffect successfullMajorParryVFX;
        public VisualEffect successfullMinorParryVFX;


        private void Start()
        {
            myGlobalStats = GameManager.Instance.GetComponent<GlobalStats>();
            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            myPlayerInventory = GameManager.Instance.GetComponent<PlayerInventory>();
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
            hitObjects.Clear();
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
                    mainModule.startColor = (Color)(GameGlobals.majorColor);
                }
                else if (currentScale == GameGlobals.MusicScale.MINOR)
                {
                    mainModule.startColor = (Color)(GameGlobals.minorColor);
                }

                myParticles.Play();
            }
        }

        public void PlayParryVFX()
        {
            if (currentScale == GameGlobals.MusicScale.MAJOR)
            {
                majorParryVFX.Play();
            }
            else if (currentScale == GameGlobals.MusicScale.MINOR)
            {
                minorParryVFX.Play();
            }
        }

        public void PlayVFX()
        {
            if (currentScale == GameGlobals.MusicScale.MAJOR)
            {
                majorVFX.Play();
            }
            else if (currentScale == GameGlobals.MusicScale.MINOR)
            {
                minorVFX.Play();
            }
            else
            {
                Debug.Log("no scale for VFX");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hitObjects.Contains(other.gameObject)) return;
            hitObjects.Add(other.gameObject);


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
                        Rumble(0.1f, 0.5f, 0.1f); // Light buzz
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

                    int rand = Random.Range(0, 101);
                    int currentCritChance = Mathf.RoundToInt(myPlayerInventory.currentLuck / 2f);

                    if (rand < currentCritChance)
                    {
                        Debug.Log("crit");
                        damage = Mathf.RoundToInt(damage * 2f);
                    }

                    Debug.Log("Damage delt: " + damage);
                    int comboLength = GetComponentInParent<PlayerStateMachine>().currentComboString.Length;
                    CustomEvents.DamageDelt(damage, currentScale, otherHealth, comboLength);
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

                if (otherHealth.GetIgnoreDamage())
                {
                    Debug.Log("immortal currently");
                    return;
                }

                int rand = Random.Range(0, 101);
                int currentEvadeChance = Mathf.RoundToInt(myPlayerInventory.currentLuck / 5f);

                if (rand < currentEvadeChance)
                {
                    Debug.Log("evaded damage");
                    return;
                }
                

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (doHitstop)
                    {
                        StopTime();
                        ScreenShake();
                        Rumble(1.0f, 0.5f, 0.5f); // Big rumble
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
                            StunEnemiesInArea();
                            sameScale = false;
                        }
                        else
                        {
                            //Debug.Log("..but not the opposite scale");
                        }

                        otherAttributes.GetComponent<PlayerStateMachine>().myWeaponAttributes.PlayStunVFX(otherAttributes.currentScale);

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

                        otherStateMachine.SetStaggered();
                        CustomEvents.FailedParry(damage, currentScale, otherHealth, sameScale);
                    }
                    else
                    {
                        otherStateMachine.SetStaggered();
                        CustomEvents.DamageReceived(damage, currentScale, otherHealth);
                    }

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
            FindFirstObjectByType<ScreenShakeManager>().Shake(0.4f);
        }


        public void StopTime()
        {
            StartCoroutine(DoHitStop());
        }

        IEnumerator DoHitStop()
        {
            float pauseDuration = 0.2f; // Adjust the duration of the freeze

            Time.timeScale = 0.1f; // Slow down time instead of freezing completely
            yield return new WaitForSecondsRealtime(pauseDuration);
            Time.timeScale = 1f; // Resume normal time
        }

        private void StunEnemiesInArea()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject e in enemies)
            {
                if (Vector3.Magnitude(transform.position - e.transform.position) < stunArea)
                {
                    e.GetComponent<MommottiStateMachine>().SetStaggered();
                }
            }
        }

        public void PlayStunVFX(GameGlobals.MusicScale whichScale)
        {
            if (whichScale == GameGlobals.MusicScale.MAJOR)
            {
                successfullMajorParryVFX.Play();
            }
            else if (whichScale == GameGlobals.MusicScale.MINOR)
            {
                successfullMinorParryVFX.Play();
            }
        }

        public void Rumble(float big, float small, float duration)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            // Clamp values between 0 and 1
            big = Mathf.Clamp01(big);
            small = Mathf.Clamp01(small);

            // Set motor speeds
            gamepad.SetMotorSpeeds(big, small);

            // Stop after duration
            StartCoroutine(StopRumbleAfter(duration));
        }

        private IEnumerator StopRumbleAfter(float duration)
        {
            yield return new WaitForSeconds(duration);

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }
    }
}
