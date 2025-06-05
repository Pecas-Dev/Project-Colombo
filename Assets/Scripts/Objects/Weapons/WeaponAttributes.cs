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
using ProjectColombo.Tutorial;

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

        [ReadOnlyInspector] public string ownerTag;
        [HideInInspector] public bool onCooldown;
        [HideInInspector] public bool isAttacking;
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;

        bool doHitstop = true;
        bool isSlowMo = false;
        List<GameObject> hitObjects = new();

        ParticleSystem myParticles;
        Collider myCollider;
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;
        PlayerInventory myPlayerInventory;

        public VisualEffect majorVFXOne;
        public VisualEffect majorVFXTwo;
        public VisualEffect majorVFXThree;
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
            isAttacking = false;
            currentTimer = 0;

            if (GetComponentInParent<HealthManager>() != null)
            {
                ownerTag = GetComponentInParent<HealthManager>().tag;
            }

            GetCurrentStats();

            if (ownerTag == "Boss" || ownerTag == "")
            {
                ownerTag = "Enemy";
                majorDamageMultiplier = 1;
                minorDamageMultiplier = 1;
                blockDamageReductionPercentage = myGlobalStats.currentBlockReductionPercent;
                missedParryPaneltyPercentage = myGlobalStats.currentMissedParryPaneltyPercent;
            }

            myParticles = GetComponent<ParticleSystem>();
        }



        void GetCurrentStats()
        {
            if (ownerTag == "Player")
            {
                Debug.Log("set weapon stats");
                majorDamageMultiplier = myGlobalStats.currentMajorDamageMultiplyer;
                minorDamageMultiplier = myGlobalStats.currentMinorDamageMultiplyer;
                correctAttackScaleBonusPercentage = myGlobalStats.currentCorrectAttackScalePercent;
                blockDamageReductionPercentage = myGlobalStats.currentBlockReductionPercent;
                missedParryPaneltyPercentage = myGlobalStats.currentMissedParryPaneltyPercent;
            }
            else if (ownerTag == "Enemy")
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
            hitObjects.Clear();
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
                var sm = GetComponentInParent<PlayerStateMachine>();

                if (sm == null)
                {
                    majorVFXOne.Play();
                    return;
                }

                int comboLenght = sm.currentComboString.Length;

                if (comboLenght == 1)
                {
                    majorVFXOne.Play();
                    return;

                }
                else if (comboLenght == 2)
                {
                    majorVFXTwo.Play();
                    return;
                }
                else
                {
                    majorVFXThree.Play();
                    return;
                }

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

            if (ownerTag == "Player" && other.CompareTag("Destroyable"))
            {
                //Debug.Log("Player hit Destroyable");
                HealthManager otherHealth = other.GetComponent<HealthManager>();

                if (otherHealth != null)
                {
                    otherHealth.TakeDamage(damage);
                }
            }
            else if (ownerTag == "Player" && (other.CompareTag("Enemy") || other.CompareTag("Boss")))
            {
                MommottiStateMachine otherStateMachine = other.GetComponent<MommottiStateMachine>();
                EntityAttributes otherAttributes = other.GetComponent<EntityAttributes>();
                HealthManager otherHealth = other.GetComponent<HealthManager>();
                bool sameScale = false;

                if (otherHealth.GetIgnoreDamage()) return;

                if (otherStateMachine != null && otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (doHitstop)
                    {
                        ScreenShake();
                    }

                    if (currentScale != otherAttributes.currentScale)
                    {
                        if (doHitstop)
                        {
                            StopTime();
                            Rumble(0.1f, 0.5f, 0.1f); // Light buzz
                        }

                        //Debug.Log("..with the opposite scale");
                        sameScale = false;
                        damage = AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        sameScale = true;
                        //Debug.Log("..but not the opposite scale");
                    }

                    int rand = Random.Range(0, 101);
                    int currentCritChance = Mathf.RoundToInt(myPlayerInventory.currentLuck / 2f);

                    if (rand < currentCritChance)
                    {
                        Debug.Log("crit");
                        damage = Mathf.RoundToInt(damage * 2f);
                    }

                    Debug.Log("regular enemy Damage delt: " + damage);
                    int comboLength = GetComponentInParent<PlayerStateMachine>().currentComboString.Length;
                    CustomEvents.DamageDelt(damage, currentScale, sameScale, otherHealth, comboLength);
                    otherHealth.TakeDamage(damage);

                    Vector3 attackDirection = (other.transform.position - transform.parent.position).normalized; //get direction from user to target
                    attackDirection.y = 0.0f; //could be increased to make the hit entity jump a bit

                    otherStateMachine.ApplyKnockback(attackDirection, knockback, currentScale);

                    if (doHitstop)
                    {
                        doHitstop = false;
                    }
                }
                else if (otherHealth != null && otherHealth.CurrentHealth > 0)
                {
                    if (doHitstop)
                    {
                        StopTime();
                        ScreenShake();
                        //Rumble(0.1f, 0.5f, 0.1f); // Light buzz
                    }

                    if (currentScale != otherAttributes.currentScale)
                    {
                        if (doHitstop)
                        {
                            Rumble(0.1f, 0.5f, 0.2f); // Light buzz
                        }

                        //Debug.Log("..with the opposite scale");
                        sameScale = false;
                        damage = AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        sameScale = true;
                        //Debug.Log("..but not the opposite scale");
                    }

                    int rand = Random.Range(0, 101);
                    int currentCritChance = Mathf.RoundToInt(myPlayerInventory.currentLuck / 2f);

                    if (rand < currentCritChance)
                    {
                        damage = Mathf.RoundToInt(damage * 2f);
                    }

                    int comboLength = GetComponentInParent<PlayerStateMachine>().currentComboString.Length;
                    CustomEvents.DamageDelt(damage, currentScale, sameScale, otherHealth, comboLength);
                    otherHealth.TakeDamage(damage);
                    doHitstop = false;
                }
                else if (otherHealth != null) //for tutorial dummy
                {
                    if (doHitstop)
                    {
                        StopTime();
                        ScreenShake();
                        Rumble(0.1f, 0.5f, 0.2f); // Light buzz
                        doHitstop = false;
                    }

                    if (currentScale != otherAttributes.currentScale && otherAttributes.currentScale != GameGlobals.MusicScale.NONE)
                    {
                        //Debug.Log("..with the opposite scale");
                        sameScale = false;
                        damage = AddTemporaryDamagePercentage(damage, correctAttackScaleBonusPercentage);
                    }
                    else
                    {
                        sameScale = true;
                        //Debug.Log("..but not the opposite scale");
                    }

                    Debug.Log("tutorial hit damage = " + damage);
                    CustomEvents.DamageDelt(damage, currentScale, sameScale, otherHealth, 0);
                    otherHealth.TakeDamage(damage);
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
                    }

                    if (otherStateMachine.isBlocking)
                    {
                        Debug.Log("Player blocked incoming attack");
                        damage = AddTemporaryDamagePercentage(damage, -blockDamageReductionPercentage);
                        CustomEvents.DamageBlocked(damage, currentScale, otherHealth);

                        if (doHitstop)
                        {
                            Rumble(0.1f, 0.5f, 0.2f); // Light buzz
                        }
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

                        if (doHitstop)
                        {
                            Rumble(0.1f, 0.5f, 0.2f); // Light buzz
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

                        if (doHitstop)
                        {
                            Rumble(1.0f, 0.5f, 0.5f); // Big buzz
                        }

                        otherStateMachine.SetStaggered();
                        CustomEvents.FailedParry(damage, currentScale, otherHealth, sameScale);
                    }
                    else
                    {
                        if (doHitstop)
                        {
                            Rumble(1.0f, 0.5f, 0.5f); // Big buzz
                        }

                        if (otherStateMachine.isInvunerable)
                        {
                            otherStateMachine.SetStaggered();
                        }

                        otherStateMachine.SetStaggered();
                        CustomEvents.DamageReceived(damage, currentScale, otherHealth);
                    }

                    otherHealth.TakeDamage(damage);

                    if (doHitstop)
                    {
                        doHitstop = false;
                    }
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

            isSlowMo = true;
            Time.timeScale = 0.1f; // Slow down time instead of freezing completely
            yield return new WaitForSecondsRealtime(pauseDuration);

            isSlowMo = false;
            Time.timeScale = 1f; // Resume normal time
        }

        private void StunEnemiesInArea()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject e in enemies)
            {
                if (!e.TryGetComponent<MommottiStateMachine>(out _))
                {
                    e.GetComponent<TutorialDummyBehavior>().SetStaggered();
                    return;
                }

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
            yield return new WaitForSecondsRealtime(duration);

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }

        public void ClearCollider()
        {
            hitObjects = new();
        }

        void OnDisable()
        {
            if (isSlowMo)
                Time.timeScale = 1f; // Ensure game doesn't stay stuck in slow motion

            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            gamepad.SetMotorSpeeds(0f, 0f);
        }


        private void OnDestroy()
        {
            if (isSlowMo)
                Time.timeScale = 1f;

            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}
