using ProjectColombo.Combat;
using ProjectColombo.Enemies;
using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectColombo.Tutorial
{
    public class TutorialDummyBehavior : MonoBehaviour
    {
        EntityAttributes myEntityAttributes;
        HealthManager myHealthManager;
        Animator myAnimator;
        WeaponAttributes myWeaponAttributes;

        public int maxHealth;
        public int minHealth;
        public bool healWhenLow;
        int lastFrameHealth;

        public bool canAttack;
        public float attackIntervall = 3f;
        float attackTimer = 0;


        public GameGlobals.MusicScale myScale;

        private void Start()
        {
            canAttack = false;
            myEntityAttributes = GetComponent<EntityAttributes>();
            myEntityAttributes.SetScale(myScale);
            GetComponent<TextureSwapScale>().SetMaterials(myScale);

            myWeaponAttributes = GetComponentInChildren<WeaponAttributes>();

            myHealthManager = GetComponent<HealthManager>();
            myHealthManager.AddHealthPoints(maxHealth);
            lastFrameHealth = maxHealth;

            myAnimator = GetComponent<Animator>();
            CustomEvents.OnSuccessfullParry += OnSuccessfullParry;
        }

        private void OnSuccessfullParry(GameGlobals.MusicScale scale, bool sameScale)
        {
            if (!sameScale)
            {
                canAttack = false;
                InterruptAttack();
            }

            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
        }

        private void OnDisable()
        {
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
        }

        private void OnDestroy()
        {
            CustomEvents.OnSuccessfullParry -= OnSuccessfullParry;
        }

        private void Update()
        {
            myAnimator.ResetTrigger("Impact");
            myAnimator.ResetTrigger("Attack");

            if (!canAttack)
            {

                if (lastFrameHealth > myHealthManager.currentHealth)
                {
                    myAnimator.SetTrigger("Impact");
                    TutorialEvents.DummyHit();
                }

                lastFrameHealth = myHealthManager.currentHealth;

                if (healWhenLow && myHealthManager.currentHealth < minHealth)
                {
                    while (myHealthManager.currentHealth < myHealthManager.MaxHealth)
                    {
                        myHealthManager.Heal(maxHealth);
                    }
                }
            }
            else
            {
                attackTimer += Time.deltaTime;

                if (attackTimer >= attackIntervall)
                {
                    attackTimer = 0;
                    myAnimator.SetTrigger("Attack");
                }
            }
        }

        public void SwitchScale(GameGlobals.MusicScale newScale)
        {
            myEntityAttributes.SetScale(newScale);
            GetComponent<TextureSwapScale>().SetMaterials(newScale);

            myHealthManager.Heal(maxHealth);
            lastFrameHealth = maxHealth;
        }

        public void StartAttack()
        {
            myWeaponAttributes.EnableWeaponHitbox();
        }

        public void InterruptAttack()
        {
            myWeaponAttributes.isAttacking = false;
            myWeaponAttributes.DisableWeaponHitbox();
        }

        public void Telegraphing()
        {
            myWeaponAttributes.Telegraphing();
        }

        public void SetChaseState()
        {

        }

        public void SetAttacker()
        {
            canAttack = true;
            myHealthManager.ignoreDamage = true;

            while (myHealthManager.currentHealth < myHealthManager.MaxHealth)
            {
                myHealthManager.Heal(maxHealth);
            }
        }

        public void StopAttacker()
        {
            canAttack = false;
            myHealthManager.ignoreDamage = false;

            while (myHealthManager.currentHealth < myHealthManager.MaxHealth)
            {
                myHealthManager.Heal(maxHealth);
            }
        }

        public void SetStaggered()
        {
            InterruptAttack();
            myAnimator.SetTrigger("Impact");
        }
    }
}