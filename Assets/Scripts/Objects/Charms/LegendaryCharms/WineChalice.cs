using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
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
        float attackSpeedDelta;
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

        PlayerStateMachine myPlayerStateMachine;

        private void Update()
        {
            if (abilityReady)
            {
                if (myPlayerStateMachine.gameInputSO.UseItemPressed)
                {
                    GameManager.Instance.gameInput.ResetUseItemPressed();
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
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();
            GameManager.Instance.gameInput.EnableInput(GameInputSystem.InputActionType.UseItem);
            CustomEvents.OnDamageDelt += DamageIncrease;
            CustomEvents.OnDamageReceived += IncomingDamageIncrease;

            attackSpeedDelta = attackSpeedIncreasePercent / 100;
            Debug.Log("decreased weapon cooldown from: " + myPlayerStateMachine.myEntityAttributes.attackSpeed + ", by: " + attackSpeedDelta);
            myPlayerStateMachine.myEntityAttributes.attackSpeed -= attackSpeedDelta;
        }

        private void IncomingDamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * incomingDamageIncreasePercent);
            Debug.Log("increased damage from: " + damage + ", by: " + delta);
            healthmanager.TakeDamage(delta);
        }

        private void DamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * damageIncreasePercent);

            if (abilityActive)
            {
                Debug.Log("ability active");
                delta += (int)(damage / 100f * damageIncreasePercentAbility);

                Debug.Log("heal from: " + damage + ", with: " + delta);
                myPlayerStateMachine.myHealthManager.Heal(delta);
            }
            Debug.Log("decreased damage from: " + damage + ", by: " + delta);

            healthmanager.TakeDamage(delta);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= DamageIncrease;
            CustomEvents.OnDamageReceived -= IncomingDamageIncrease;

            if (abilityActive)
            {
                RemoveAbilityStats();
            }

            myPlayerStateMachine.myEntityAttributes.attackSpeed += attackSpeedDelta;
        }

        IEnumerator Ability()
        {
            Debug.Log("start ability");
            ApplyAbilityStats();

            yield return new WaitForSeconds(abilityDuration);

            Debug.Log("ability over");
            RemoveAbilityStats();
        }

        void ApplyAbilityStats()
        {
            abilityActive = true;

            weaponCooldownDeltaAbility = myPlayerStateMachine.myWeaponAttributes.cooldown / 100f * attackSpeedDecreasePercent;
            Debug.Log("decreased weapon cooldown from: " + myPlayerStateMachine.myWeaponAttributes.cooldown+ ", by: " + weaponCooldownDeltaAbility);
            myPlayerStateMachine.myWeaponAttributes.cooldown -= weaponCooldownDeltaAbility;

            moveSpeedDeltaAbility = myPlayerStateMachine.myEntityAttributes.moveSpeed / 100f * moveSpeedDecreasePercent;
            Debug.Log("decreased speed from: " + myPlayerStateMachine.myEntityAttributes.moveSpeed + ", by: " + moveSpeedDeltaAbility);
            myPlayerStateMachine.myEntityAttributes.moveSpeed -= moveSpeedDeltaAbility;

            staminaRegenDeltaAbility = myPlayerStateMachine.myStamina.regenSpeed / 100f * staminaRegenDecreasePercent;
            Debug.Log("decreased stamina regen from: " + myPlayerStateMachine.myStamina.regenSpeed + ", by: " + staminaRegenDeltaAbility);
            myPlayerStateMachine.myStamina.regenSpeed -= staminaRegenDeltaAbility;
        }

        void RemoveAbilityStats()
        {
            abilityActive = false;

            myPlayerStateMachine.myWeaponAttributes.cooldown += weaponCooldownDeltaAbility;
            myPlayerStateMachine.myEntityAttributes.moveSpeed += moveSpeedDeltaAbility;
            myPlayerStateMachine.myStamina.regenSpeed += staminaRegenDeltaAbility;
        }
    }
}