using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class WineChalice : BaseCharm
    {
        //general
        public float damageIncreasePercent = 16;
        public float attackSpeedIncreasePercent = 5;
        float weaponCooldownDeltaGeneral;
        public float incomingDamageIncreasePercent = 10;


        //ability
        public float abilityCooldown = 120;
        public float abilityDuration = 6;
        public float attackSpeedDecreasePercent = 10;
        float weaponCooldownDeltaAbility;
        public float moveSpeedDecreasePercent = 8;
        float moveSpeedDeltaAbility;
        public float staminaRegenDecreasePercent = 15;
        float staminaRegenDeltaAbility;
        public float damageIncreasePercentAbility = 18;
        public int healAmountOfDamage = 6;
        float timer = 0;
        bool abilityReady = false;
        bool abilityActive = false;



        private void Update()
        {
            if (abilityReady)
            {
                if (GetComponentInParent<PlayerStateMachine>().gameInputSO.UseItemPressed)
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
            CustomEvents.OnDamageReceived += IncomingDamageIncrease;

            weaponCooldownDeltaGeneral = GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown / 100f * attackSpeedIncreasePercent;
            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown -= weaponCooldownDeltaGeneral;
        }

        private void IncomingDamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * incomingDamageIncreasePercent);
            healthmanager.TakeDamage(delta);
        }

        private void DamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * damageIncreasePercent);

            if (abilityActive)
            {
                delta += (int)(damage / 100f * damageIncreasePercentAbility);

                int health = (int)(healAmountOfDamage * (damage + delta));
                GetComponentInParent<HealthManager>().Heal(health);
            }

            healthmanager.TakeDamage(delta);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= DamageIncrease;
            CustomEvents.OnDamageReceived -= IncomingDamageIncrease;

            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown += weaponCooldownDeltaGeneral;
        }

        IEnumerator Ability()
        {
            ApplyAbilityStats();

            yield return new WaitForSeconds(abilityDuration);

            RemoveAbilityStats();
        }

        void ApplyAbilityStats()
        {
            abilityActive = true;

            weaponCooldownDeltaAbility = GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown / 100f * attackSpeedDecreasePercent;
            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown -= weaponCooldownDeltaAbility;

            moveSpeedDeltaAbility = GetComponentInParent<EntityAttributes>().moveSpeed / 100f * moveSpeedDecreasePercent;
            GetComponentInParent<EntityAttributes>().moveSpeed -= moveSpeedDeltaAbility;

            staminaRegenDeltaAbility = GetComponentInParent<Stamina>().regenSpeed / 100f * staminaRegenDecreasePercent;
            GetComponentInParent<Stamina>().regenSpeed -= staminaRegenDeltaAbility;
        }

        void RemoveAbilityStats()
        {
            abilityActive = false;

            GetComponentInParent<PlayerStateMachine>().myWeaponAttributes.cooldown += weaponCooldownDeltaAbility;
            GetComponentInParent<EntityAttributes>().moveSpeed += moveSpeedDeltaAbility;
            GetComponentInParent<Stamina>().regenSpeed += staminaRegenDeltaAbility;
        }
    }
}