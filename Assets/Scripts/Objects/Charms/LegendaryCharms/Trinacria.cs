using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
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
        public float damageIncreasePercent = 5;
        public float attackSpeedIncreasePercent = 3;
        float attackSpeedDelta;
        public float moveSpeedIncreasePercent = 3;
        float moveSpeedDeltaGeneral;
        public float incomingDamageDecreasePercent = 5;
        public float staminaRegenIncreasePercent = 6;
        float staminaRegenDeltaGeneral;
        public float decreaseBlockDamagePercent = 15;
        public float failedParryDamageIncreasePercent = 10;


        //ability
        public float abilityCooldown = 10;
        public float firstDuration = 2;
        public float secondDuration = 4;
        public float damageDecreaseAbility = 10;
        public float moveSpeedDecreaseAbility = 3;
        float moveSpeedDeltaAbility;
        public float incomingDamageIncreasePercentAbility = 8;
        float timer = 0;
        bool abilityReady = false;
        bool abilityActive = false;
        bool firstStepAbility = false;

        PlayerStateMachine myPlayerStateMachine;

        private void Update()
        {
            if (abilityReady)
            {
                if (GameManager.Instance.gameInput.UseItemPressed)
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
                    Debug.Log("ability ready");
                    abilityReady = true;
                }
            }

            if (myPlayerStateMachine.gameInputSO.UseItemPressed)
            {
                GameManager.Instance.gameInput.ResetUseItemPressed();
            }
        }

        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();
            GameManager.Instance.gameInput.EnableInput(GameInputSystem.InputActionType.UseItem);

            CustomEvents.OnDamageDelt += DamageIncrease;
            CustomEvents.OnDamageReceived += IncomingDamageDecrease;
            CustomEvents.OnDamageBlocked += DecreaseBlockedDamage;
            CustomEvents.OnParryFailed += IncreaseFailParryDamage;

            attackSpeedDelta = attackSpeedIncreasePercent / 100f;
            Debug.Log("increased weapon cooldown from: " + myPlayerStateMachine.myEntityAttributes.attackSpeed + ", by: " + attackSpeedDelta);
            myPlayerStateMachine.myEntityAttributes.attackSpeed -= attackSpeedDelta;

            moveSpeedDeltaGeneral = myPlayerStateMachine.myEntityAttributes.moveSpeed / 100f * moveSpeedIncreasePercent;
            Debug.Log("increased speed from: " + myPlayerStateMachine.myEntityAttributes.moveSpeed + ", by: " + moveSpeedDeltaGeneral);
            myPlayerStateMachine.myEntityAttributes.moveSpeed += moveSpeedDeltaGeneral;

            staminaRegenDeltaGeneral = myPlayerStateMachine.myStamina.regenTime / 100f * staminaRegenIncreasePercent;
            Debug.Log("increased speed from: " + myPlayerStateMachine.myStamina.regenTime + ", by: " + staminaRegenDeltaGeneral);
            myPlayerStateMachine.myStamina.regenTime -= staminaRegenDeltaGeneral;
        }

        private void IncreaseFailParryDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager, bool sameScale)
        {
            int delta = (int)(damage / 100f * failedParryDamageIncreasePercent);
            Debug.Log("increased failed parry damage from: " + damage + ", by: " + delta);
            healthmanager.TakeDamage(delta);
        }

        private void DecreaseBlockedDamage(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * decreaseBlockDamagePercent);
            Debug.Log("decreased block damage from: " + damage + ", by: " + delta);
            healthmanager.TakeDamage(-delta);
        }

        private void IncomingDamageDecrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * incomingDamageDecreasePercent);

            if (firstStepAbility)
            {
                delta = damage;
                Debug.Log("firstStepability");
            }
            else if (abilityActive)
            {
                delta = -(int)(damage /100f * incomingDamageIncreasePercentAbility);
                Debug.Log("second step ability");
            }

            Debug.Log("decreased damage from: " + damage + ", by: " + delta);

            healthmanager.TakeDamage(-delta);
        }

        private void DamageIncrease(int damage, GameGlobals.MusicScale scale, HealthManager healthmanager)
        {
            int delta = (int)(damage / 100f * damageIncreasePercent);

            if (!firstStepAbility && abilityActive)
            {
                Debug.Log("second step ability");
                delta = (int)(damage / 100f * damageDecreaseAbility);
            }

            Debug.Log("increased damage from: " + damage + ", by: " + delta);
            healthmanager.TakeDamage(delta);
        }

        public override void Remove()
        {
            CustomEvents.OnDamageDelt -= DamageIncrease;
            CustomEvents.OnDamageReceived -= IncomingDamageDecrease;
            CustomEvents.OnDamageBlocked -= DecreaseBlockedDamage;
            CustomEvents.OnParryFailed -= IncreaseFailParryDamage;

            myPlayerStateMachine.myEntityAttributes.attackSpeed += attackSpeedDelta;
            myPlayerStateMachine.myEntityAttributes.moveSpeed -= moveSpeedDeltaGeneral;
            myPlayerStateMachine.myStamina.regenTime += staminaRegenDeltaGeneral;
        }

        IEnumerator Ability()
        {
            Debug.Log("Start ability");
            ApplyAbilityStats();

            yield return new WaitForSeconds(firstDuration);

            Debug.Log("second step starts");
            MiddleStepAbility();

            yield return new WaitForSeconds(secondDuration);

            Debug.Log("ability over");
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

            moveSpeedDeltaAbility = myPlayerStateMachine.myEntityAttributes.moveSpeed / 100f * moveSpeedDecreaseAbility;
            Debug.Log("increased speed from: " + myPlayerStateMachine.myEntityAttributes.moveSpeed + ", by: " + moveSpeedDeltaAbility);
            myPlayerStateMachine.myEntityAttributes.moveSpeed -= moveSpeedDeltaAbility;
        }

        void RemoveAbilityStats()
        {
            abilityActive = false;

            myPlayerStateMachine.myEntityAttributes.moveSpeed += moveSpeedDeltaAbility;
            Debug.Log("decreased speed from: " + myPlayerStateMachine.myEntityAttributes.moveSpeed + ", by: " + moveSpeedDeltaAbility);
        }
    }
}