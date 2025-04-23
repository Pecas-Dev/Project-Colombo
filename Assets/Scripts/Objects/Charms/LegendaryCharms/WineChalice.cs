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
                if (GameObject.Find("Player").GetComponent<PlayerStateMachine>().gameInputSO.UseItemPressed)
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

            weaponCooldownDeltaGeneral = GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown / 100f * attackSpeedIncreasePercent;
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown -= weaponCooldownDeltaGeneral;
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
                GameObject.Find("Player").GetComponent<HealthManager>().Heal(health);
            }

            healthmanager.TakeDamage(delta);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= DamageIncrease;
            CustomEvents.OnDamageReceived -= IncomingDamageIncrease;

            GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown += weaponCooldownDeltaGeneral;
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

            weaponCooldownDeltaAbility = GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown / 100f * attackSpeedDecreasePercent;
            GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown -= weaponCooldownDeltaAbility;

            moveSpeedDeltaAbility = GameObject.Find("Player").GetComponent<EntityAttributes>().moveSpeed / 100f * moveSpeedDecreasePercent;
            GameObject.Find("Player").GetComponent<EntityAttributes>().moveSpeed -= moveSpeedDeltaAbility;

            staminaRegenDeltaAbility = GameObject.Find("Player").GetComponent<Stamina>().regenSpeed / 100f * staminaRegenDecreasePercent;
            GameObject.Find("Player").GetComponent<Stamina>().regenSpeed -= staminaRegenDeltaAbility;
        }

        void RemoveAbilityStats()
        {
            abilityActive = false;

            GameObject.Find("Player").GetComponent<PlayerStateMachine>().myWeaponAttributes.cooldown += weaponCooldownDeltaAbility;
            GameObject.Find("Player").GetComponent<EntityAttributes>().moveSpeed += moveSpeedDeltaAbility;
            GameObject.Find("Player").GetComponent<Stamina>().regenSpeed += staminaRegenDeltaAbility;
        }
    }
}