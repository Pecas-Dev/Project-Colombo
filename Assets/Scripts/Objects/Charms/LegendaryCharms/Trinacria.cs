using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class Trinacria : BaseCharm
    {
        //general
        public int damageIncreasePercent = 5;
        public int attackSpeedIncreasePercent = 3;
        float weaponCooldownDeltaGeneral;
        public int moveSpeedIncreasePercent = 3;
        float moveSpeedDeltaGeneral;
        public int incomingDamageDecreasePercent = 5;
        public int staminaRegenIncreasePercent = 6;
        float staminaRegenDeltaGeneral;
        public int decreaseBlockDamagePercent = 15;
        public int failedParryDamageIncreasePercent = 10;


        //ability
        public float abilityCooldown = 160;
        public float firstDuration = 2;
        public float secondDuration = 4;
        public int damageDecreaseAbility = 10;
        public int moveSpeedDecreaseAbility = 3;
        float moveSpeedDeltaAbility;
        public int incomingDamageIncreasePercentAbility = 8;
        float timer = 0;
        bool abilityReady = false;
        bool abilityActive = false;
        bool firstStepAbility = false;



        private void Update()
        {
            if (abilityReady)
            {
                if (GetComponentInParent<PlayerStateMachine>().gameInputSO.UseSpecialAbilityPressed)
                {
                    StartCoroutine(Ability());
                    abilityReady = false;
                    timer = 0;
                }
            }
            else
            {
                timer += Time.deltaTime;

                if (timer >= abilityCooldown)
                {
                    abilityReady = true;
                }
            }
        }

        public override void Equip()
        {
            CustomEvents.OnDamageDelt += DamageIncrease;
            CustomEvents.OnDamageReceived += IncomingDamageDecrease;
            CustomEvents.OnDamageBlocked += DecreaseBlockedDamage;
            CustomEvents.OnParryFailed += IncreaseFailParryDamage;

            weaponCooldownDeltaGeneral = GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown / 100f * attackSpeedIncreasePercent;
            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown -= weaponCooldownDeltaGeneral;

            moveSpeedDeltaGeneral = GetComponentInParent<EntityAttributes>().moveSpeed / 100f * moveSpeedIncreasePercent;
            GetComponentInParent<EntityAttributes>().moveSpeed += moveSpeedDeltaGeneral;

            staminaRegenDeltaGeneral = GetComponentInParent<Stamina>().regenSpeed / 100f * staminaRegenIncreasePercent;
            GetComponentInParent<Stamina>().regenSpeed -= staminaRegenDeltaGeneral;
        }

        private void IncreaseFailParryDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager, bool sameScale)
        {
            int delta = (int)(damage / 100f * failedParryDamageIncreasePercent);
            healthmanager.TakeDamage(delta);
        }

        private void DecreaseBlockedDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * decreaseBlockDamagePercent);
            healthmanager.TakeDamage(-delta);
        }

        private void IncomingDamageDecrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * incomingDamageDecreasePercent);

            if (firstStepAbility)
            {
                delta = damage;
            }
            else if (abilityActive)
            {
                delta = -(int)(damage /100f * incomingDamageIncreasePercentAbility);
            }

            healthmanager.TakeDamage(-delta);
        }

        private void DamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * damageIncreasePercent);

            if (!firstStepAbility && abilityActive)
            {
                delta = (int)(damage / 100f * damageDecreaseAbility);
            }

            healthmanager.TakeDamage(delta);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= DamageIncrease;
            CustomEvents.OnDamageReceived -= IncomingDamageDecrease;

            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown += weaponCooldownDeltaGeneral;
            GetComponentInParent<EntityAttributes>().moveSpeed -= moveSpeedDeltaGeneral;
            GetComponentInParent<Stamina>().regenSpeed += staminaRegenDeltaGeneral;
        }

        IEnumerator Ability()
        {
            ApplyAbilityStats();

            yield return new WaitForSeconds(firstDuration);

            MiddleStepAbility();

            yield return new WaitForSeconds(secondDuration);

            RemoveAbilityStats();
        }

        void ApplyAbilityStats()
        {
            abilityActive = true;
            firstStepAbility = true;
            
        }

        void MiddleStepAbility()
        {
            firstStepAbility = false;

            moveSpeedDeltaAbility = GetComponentInParent<EntityAttributes>().moveSpeed / 100f * moveSpeedDecreaseAbility;
            GetComponentInParent<EntityAttributes>().moveSpeed -= moveSpeedDeltaAbility;
        }

        void RemoveAbilityStats()
        {
            abilityActive = false;

            GetComponentInParent<EntityAttributes>().moveSpeed += moveSpeedDeltaAbility;
        }
    }
}