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
        public int majorScaleDamageIncreasePercent = 12;
        public int minorScaleDamageIncreasePercent = 12;
        public float extraDamageForMissingHealthPercent = 1.2f;
        public int extraDamageForHowManyPoints = 50;
        public int extraDamageReceivePercent = 12;
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
        public int attackSpeedIncreasePercent = 10;
        float attackSpeedIncreaseValue;
        public int movementSpeedIncreasePercent = 8;
        float movementSpeedIncreaseValue;

        PlayerStateMachine myPlayerStateMachine;

        public override void Equip()
        {
            myPlayerStateMachine = GetComponentInParent<PlayerStateMachine>();

            CustomEvents.OnDamageDelt += AddDamageDelt;
            CustomEvents.OnDamageReceived += AddDamageReceive;
            CustomEvents.OnParryFailed += AddFailedParry;

            myPlayerStateMachine.myHealthManager.AddHealthPoints(100);
        }

        private void AddFailedParry(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, bool sameScale)
        {
            int currentHealth = myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentHealth / extraMissingParryForHowManyPoints) * extraDamageIncraseMissingParryForPoints);

            int value = (int)(damage * extraDamageReceivePercent / 100f);
            healthmanager.TakeDamage(value + extra);
        }

        private void AddDamageReceive(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int currentHealth = myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentHealth / extraDamageReceiveForHowManyPoints) * extraDamageReceiveForMissingHealthPercent);

            int value = (int)(damage * extraDamageReceivePercent / 100f);
            healthmanager.TakeDamage(value + extra);
        }

        private void AddDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int currentMissingHealth = myPlayerStateMachine.myHealthManager.MaxHealth - myPlayerStateMachine.myHealthManager.currentHealth;
            int extra = (int)(Mathf.FloorToInt(currentMissingHealth / extraDamageForHowManyPoints) * extraDamageForMissingHealthPercent);

            int abilityExtra = (int)(damage * extraDamageForStaminaPercent / 100f);
            healthmanager.TakeDamage(abilityExtra);

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                int value = (int)(damage * majorScaleDamageIncreasePercent / 100f);
                healthmanager.TakeDamage(value + extra);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                int value = (int)(damage * minorScaleDamageIncreasePercent / 100f);
                healthmanager.TakeDamage(value + extra);
            }
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= AddDamageDelt;
            CustomEvents.OnDamageReceived -= AddDamageReceive;
            CustomEvents.OnParryFailed -= AddFailedParry;

            myPlayerStateMachine.myHealthManager.AddHealthPoints(-100);
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

            if (myPlayerStateMachine.currentState == PlayerStateMachine.PlayerState.Attack)
            {
                myPlayerStateMachine.myHealthManager.TakeDamage(removeHealthPerAttack);
            }
        }

        IEnumerator Ability()
        {
            myPlayerStateMachine.myEntityAttributes.moveSpeed += movementSpeedIncreaseValue;
            myPlayerStateMachine.myWeaponAttributes.cooldown -= attackSpeedIncreaseValue;
            staminaCounter = 0;

            CustomEvents.OnStaminaUsed += OnStaminaUsed;

            yield return new WaitForSeconds(abilityDuration);

            myPlayerStateMachine.myEntityAttributes.moveSpeed -= movementSpeedIncreaseValue;
            myPlayerStateMachine.myWeaponAttributes.cooldown += attackSpeedIncreaseValue;

            CustomEvents.OnStaminaUsed -= OnStaminaUsed;
        }

        IEnumerator AbilityExtraDamage()
        {
            abilityExtraDamageCounter++;

            yield return new WaitForSeconds(extraDamageDuration);

            abilityExtraDamageCounter--;
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


