using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfCoviello : BaseMask
    {
        //general
        public float majorScaleDamageIncreasePercent = 12;
        public float minorScaleDamageIncreasePercent = 12;
        public float extraDamageForMissingHealthPercent = 1.2f;
        public int extraDamageForHowManyPoints = 50;
        public float extraDamageReceivePercent = 12;
        public float extraDamageReceiveForMissingHealthPercent = 0.32f;
        public int extraDamageReceiveForHowManyPoints = 100;
        public float extraDamageIncraseMissingParryForPoints = 0.125f;
        public int extraMissingParryForHowManyPoints = 200;
        public int extraHealthPoints = 100;


        //ability
        public float abilityCooldown = 120;
        public float abilityDuration = 6;
        int staminaCounter;
        int abilityExtraDamageCounter = 0;
        public float extraDamageForStaminaPercent = 5;
        public int extraDamageDuration = 8;
        public int removeHealthPerAttack = 20;
        public float attackSpeedIncreasePercent = 10;
        float attackSpeedIncreaseValue;
        public float movementSpeedIncreasePercent = 8;
        float movementSpeedIncreaseValue;

        bool abilityActive = false;
        PlayerStateMachine myPlayerStateMachine;

        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

            CustomEvents.OnDamageDelt += AddDamageDelt;
            CustomEvents.OnDamageReceived += AddDamageReceive;
            CustomEvents.OnParryFailed += AddFailedParry;

            Debug.Log("added health: " + extraHealthPoints);
            myPlayerStateMachine.myHealthManager.AddHealthPoints(extraHealthPoints);
        }

        private void AddFailedParry(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, bool sameScale)
        {
            int currentHealth = myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentHealth / extraMissingParryForHowManyPoints) * extraDamageIncraseMissingParryForPoints);

            int value = (int)(damage * extraDamageReceivePercent / 100f);
            Debug.Log("extra failed parry damage: " + (value + extra));
            healthmanager.TakeDamage(value + extra);
        }

        private void AddDamageReceive(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int currentHealth = myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentHealth / extraDamageReceiveForHowManyPoints) * extraDamageReceiveForMissingHealthPercent);

            int value = (int)(damage * extraDamageReceivePercent / 100f);
            Debug.Log("extra receive damage: " + (value + extra));
            healthmanager.TakeDamage(value + extra);
        }

        private void AddDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int currentMissingHealth = myPlayerStateMachine.myHealthManager.MaxHealth - myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentMissingHealth / extraDamageForHowManyPoints) * extraDamageForMissingHealthPercent);

            int abilityExtra = (int)(abilityExtraDamageCounter * damage * extraDamageForStaminaPercent / 100f);
            healthmanager.TakeDamage(abilityExtra);

            if (abilityActive)
            {
                Debug.Log("lost health for attacking: " + removeHealthPerAttack);
                myPlayerStateMachine.myHealthManager.TakeDamage(removeHealthPerAttack);
            }

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                int value = (int)(damage * majorScaleDamageIncreasePercent / 100f);
                Debug.Log("extra major damage: " + (value + extra));
                healthmanager.TakeDamage(value + extra);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                int value = (int)(damage * minorScaleDamageIncreasePercent / 100f);
                Debug.Log("extra minor damage: " + (value + extra));
                healthmanager.TakeDamage(value + extra);
            }
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= AddDamageDelt;
            CustomEvents.OnDamageReceived -= AddDamageReceive;
            CustomEvents.OnParryFailed -= AddFailedParry;
            CustomEvents.OnStaminaUsed -= OnStaminaUsed;

            myPlayerStateMachine.myHealthManager.AddHealthPoints(-extraHealthPoints);
        }

        public override void UseAbility()
        {
            currentAbilityCooldown = abilityCooldown;

            movementSpeedIncreaseValue = myPlayerStateMachine.myEntityAttributes.moveSpeed * movementSpeedIncreasePercent / 100f;
            attackSpeedIncreaseValue = myPlayerStateMachine.myWeaponAttributes.cooldown * attackSpeedIncreasePercent / 100f;

            StartCoroutine(Ability());
        }

        private void OnStaminaUsed()
        {
            staminaCounter++;
            
            if (staminaCounter %2 == 0)
            {
                StartCoroutine(AbilityExtraDamage());
            }
        }

        IEnumerator Ability()
        {
            abilityActive = true;
            Debug.Log("increased speed from: " + myPlayerStateMachine.myEntityAttributes.moveSpeed + " by: " + movementSpeedIncreaseValue);
            myPlayerStateMachine.myEntityAttributes.moveSpeed += movementSpeedIncreaseValue;
            Debug.Log("increased attack speed from: " + myPlayerStateMachine.myWeaponAttributes.cooldown + " by: " + attackSpeedIncreaseValue);
            myPlayerStateMachine.myWeaponAttributes.cooldown -= attackSpeedIncreaseValue;
            staminaCounter = 0;

            CustomEvents.OnStaminaUsed += OnStaminaUsed;

            yield return new WaitForSeconds(abilityDuration);

            Debug.Log("end ability");
            abilityActive = false;
            myPlayerStateMachine.myEntityAttributes.moveSpeed -= movementSpeedIncreaseValue;
            myPlayerStateMachine.myWeaponAttributes.cooldown += attackSpeedIncreaseValue;

            CustomEvents.OnStaminaUsed -= OnStaminaUsed;
        }

        IEnumerator AbilityExtraDamage()
        {
            Debug.Log("start extra damage");
            abilityExtraDamageCounter++;

            yield return new WaitForSeconds(extraDamageDuration);

            abilityExtraDamageCounter--;
            Debug.Log("end extra damage. Remaining: " + abilityExtraDamageCounter);
        }
    }
}


//+12 % Major scale damage(+1.2% flat each 50 missing Health points)
//+12 % Minor scale damage(+1.2% flat each 50 missing Health points)
//+12 % Received damage from all sources (increases by 0.32% for any 100 Health Points)
//Increased damage by failing an opposite scale parry is now increased by 0.125% for any 200 Health Points
//+100 Health Points


//Special Ability:
//For next 6 seconds:
//every 2 stamina points used, you get +5% damage for 8 seconds (stacks up to 25%)
//each attack consumes 20 Health Points 
//you gain +10% attack speed 
//you gain +8% movement speed.


