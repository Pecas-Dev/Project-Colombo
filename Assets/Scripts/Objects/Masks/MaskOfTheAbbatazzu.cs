using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfTheAbbatazzu : BaseMask
    {
        [Header("General Buffs")]
        public int gainHealthPointsPerKill = 20;
        public float extraDamageDecreasePercent = 12f;
        public float extraDamageIncreasePerHealthPercent = 0.5f;
        public float attackSpeedDecreasePercent = 16f;
        float attackSpeedDelta;
        public float staminaRegenDecreasePercent = 15f;
        float staminaRegenDelta;
        public int extraHealthPointsOnPickup = 300;
        public float damageReceiveDecreasePercent = 6f;
        public float extraDamageReceiveIncreasePerHealth = 0.32f;
        public float shopDiscountPercent = 15f;
        public float healthPunishmentForPurchasePercent = 20f;

        [Header("Echo Misson")]
        public int maxHealthToObtain = 1000;
        [ReadOnlyInspector] public int currentMaxHealthObtained = 0;

        [Header("Upgraded Buffs after Echo")]
        public int gainHealthPointsPerKillEcho = 30;
        public float shopDiscountPercentEcho = 20f;
        public float damageReceiveDecreasePercentEcho = 9f;
        public float extraDamageReceiveIncreasePerHealthEcho = 0.38f;
        public int healthPointsIncreaseEcho = 150;

        //Gain +30 Max Health Points each kill
        //Shop items cost 20% less, but when you buy an item you receive 20% of the cost as damage.
        //Received damage is decreased by 9 (+0.38% of Max Health) %
        //+150 Max Health Points (just adds 150 Max HPs)

        [Header("Ability Stats")]
        public float abilityCooldown = 100f;
        public float abilityDuration = 3f;
        bool ignoreDamage;

        PlayerStateMachine myPlayerStateMachine;

        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

            CustomEvents.OnEnemyDeath += OnEnemyDeath;
            CustomEvents.OnDamageDelt += OnDamageDelt;
            CustomEvents.OnDamageReceived += OnDamageReceived;
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
            CustomEvents.OnShopOpen += OnShopOpen;
            CustomEvents.OnItemPurchase += OnItemPurchase;
            CustomEvents.OnMaxHealthGained += OnMaxHealthGained;


            attackSpeedDelta = myPlayerStateMachine.myWeaponAttributes.cooldown * attackSpeedDecreasePercent / 100f;
            Debug.Log("changed attack speed from: " + myPlayerStateMachine.myWeaponAttributes.cooldown + " by: " + attackSpeedDelta);
            myPlayerStateMachine.myWeaponAttributes.cooldown += attackSpeedDelta;

            staminaRegenDelta = myPlayerStateMachine.myStamina.regenTime * staminaRegenDecreasePercent / 100;
            Debug.Log("changed stamina regen speed from: " + myPlayerStateMachine.myStamina.regenTime + " by: " + staminaRegenDelta);
            myPlayerStateMachine.myStamina.regenTime += staminaRegenDelta;
        }

        private void OnMaxHealthGained(int value)
        {
            currentMaxHealthObtained += value;

            if (currentMaxHealthObtained >= maxHealthToObtain && !echoUnlocked)
            {
                UnlockEcho();
            }
        }

        private void OnItemPurchase(int damageAmount)
        {
            int value = (int)(damageAmount * healthPunishmentForPurchasePercent / 100f);
            Debug.Log("damage for buying item: " + value);
            myPlayerStateMachine.myHealthManager.TakeDamage(value);
        }

        private void OnShopOpen()
        {
            //TODO discount shop items
        }

        private void OnCoinsCollected(int obj)
        {
            Debug.Log("extra health for coins pickup: " + extraHealthPointsOnPickup);
            myPlayerStateMachine.myHealthManager.AddHealthPoints(extraHealthPointsOnPickup);
        }

        private void OnDamageReceived(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (ignoreDamage)
            {
                Debug.Log("ignore damaged of: " + damage);
                healthmanager.Heal(damage);
                return;
            }

            int maxHealth = myPlayerStateMachine.myHealthManager.MaxHealth;
            int extra = (int)(maxHealth * extraDamageReceiveIncreasePerHealth / 100f);

            int value = (int)(damage * damageReceiveDecreasePercent / 100f);
            Debug.Log("extra damage received: " + -value + " and: " + extra);
            healthmanager.TakeDamage(-value + extra);
        }

        private void OnDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int maxHealth = myPlayerStateMachine.myHealthManager.MaxHealth;
            int extra = (int)(maxHealth * extraDamageIncreasePerHealthPercent / 100f);

            int value = (int)(damage * extraDamageDecreasePercent / 100f);
            Debug.Log("extra damage delt: " + -value + " and: " + extra);
            healthmanager.TakeDamage(-value + extra);
        }

        private void OnEnemyDeath(GameGlobals.MusicScale scale)
        {
            Debug.Log("gained health for kill: " + gainHealthPointsPerKill);
            myPlayerStateMachine.myHealthManager.AddHealthPoints(gainHealthPointsPerKill);
        }

        public override void Remove()
        {
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
            CustomEvents.OnDamageDelt -= OnDamageDelt;
            CustomEvents.OnDamageReceived -= OnDamageReceived;
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
            CustomEvents.OnShopOpen -= OnShopOpen;
            CustomEvents.OnItemPurchase -= OnItemPurchase;
            CustomEvents.OnMaxHealthGained -= OnMaxHealthGained;


            myPlayerStateMachine.myWeaponAttributes.cooldown -= attackSpeedDelta;
            myPlayerStateMachine.myStamina.regenTime -= staminaRegenDelta;
        }

        public override void UseAbility()
        {
            currentAbilityCooldown = abilityDuration;

            StartCoroutine(Ability());
        }

        IEnumerator Ability()
        {
            ignoreDamage = true;
            yield return new WaitForSeconds(abilityDuration);
            Debug.Log("end ability");
            ignoreDamage = false;
        }

        public override void UnlockEcho()
        {
            Remove();

            echoUnlocked = true;

            gainHealthPointsPerKill = gainHealthPointsPerKillEcho;
            shopDiscountPercent = shopDiscountPercentEcho;
            damageReceiveDecreasePercent = damageReceiveDecreasePercentEcho;
            extraDamageReceiveIncreasePerHealth = extraDamageReceiveIncreasePerHealthEcho;

            myPlayerStateMachine.myHealthManager.AddHealthPoints(healthPointsIncreaseEcho);
          
            Equip();
        }
    }
}




//Effects:
//Gain + 20 Max Health Points each kill
//Deal -12 (+0.5% of Max Health)% Major/Minor scale damage.
//Attack speed is reduced by 16%
//Stamina regeneration decreased by 15%
//Shop items cost 15% less, but when you buy an item you receive 20% of the cost as damage.
//+300 Max Health points on pickup/selection
//Received damage is decreased by 6 (+0.32% of Max Health) %


//ECHO OF THE MASK:
//Obtain + 1000 Max Health Points


//Awakened Stats:
//Gain +30 Max Health Points each kill
//Shop items cost 20% less, but when you buy an item you receive 20% of the cost as damage.
//Received damage is decreased by 9 (+0.38% of Max Health) %
//+150 Max Health Points (just adds 150 Max HPs)

//Special Ability: Protection from Above
//For the next 3 seconds you can’t receive damage (100 seconds cooldown).

