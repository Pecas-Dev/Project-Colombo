using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using System.Collections;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfTheAbbatazzu : BaseMask
    {
        public int gainHealthPointsPerKill = 20;
        public float extraDamageDecreasePercent = 12f;
        public float extraDamageIncreasePerHealthPercent = 0.5f;
        public float attackSpeedDecreasePercent = 16f;
        float attackSpeedDelta;
        public float staminaRegenDecreasePercent = 15f;
        float staminaRegenDelta;
        public int extraHealthPointsOnPickup = 300;
        public float damageReceiveDecreasePercent = 6f;
        public float extraDamageIncreasePerHealth = 0.32f;
        public float shopDiscountPercent = 15f;
        public float healthPunishmentForPurchasePercent = 20f;

        PlayerStateMachine myPlayerStateMachine;

        public float abilityCooldown = 100f;
        public float abilityDuration = 3f;
        bool ignoreDamage;


        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

            CustomEvents.OnEnemyDeath += OnEnemyDeath;
            CustomEvents.OnDamageDelt += OnDamageDelt;
            CustomEvents.OnDamageReceived += OnDamageReceived;
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
            CustomEvents.OnShopOpen += OnShopOpen;
            CustomEvents.OnItemPurchase += OnItemPurchase;


            attackSpeedDelta = myPlayerStateMachine.myWeaponAttributes.cooldown * attackSpeedDecreasePercent / 100f;
            myPlayerStateMachine.myWeaponAttributes.cooldown += attackSpeedDelta;

            staminaRegenDelta = myPlayerStateMachine.myStamina.regenSpeed * staminaRegenDecreasePercent / 100;
            myPlayerStateMachine.myStamina.regenSpeed += staminaRegenDelta;
        }

        private void OnItemPurchase(int damageAmount)
        {
            myPlayerStateMachine.myHealthManager.TakeDamage((int)(damageAmount * healthPunishmentForPurchasePercent / 100f));
        }

        private void OnShopOpen()
        {
            //TODO discount shop items
        }

        private void OnCoinsCollected(int obj)
        {
            myPlayerStateMachine.myHealthManager.AddHealthPoints(extraHealthPointsOnPickup);
        }

        private void OnDamageReceived(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (ignoreDamage)
            {
                healthmanager.Heal(damage);
                return;
            }

            int maxHealth = myPlayerStateMachine.myHealthManager.MaxHealth;
            int extra = (int)(maxHealth * extraDamageIncreasePerHealth);

            int value = (int)(damage * damageReceiveDecreasePercent / 100f);
            healthmanager.TakeDamage(-value + extra);
        }

        private void OnDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int maxHealth = myPlayerStateMachine.myHealthManager.MaxHealth;
            int extra = (int)(maxHealth * extraDamageIncreasePerHealthPercent);

            int value = (int)(damage * extraDamageDecreasePercent / 100f);
            healthmanager.TakeDamage(-value + extra);
        }

        private void OnEnemyDeath()
        {
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


            myPlayerStateMachine.myWeaponAttributes.cooldown -= attackSpeedDelta;
            myPlayerStateMachine.myStamina.regenSpeed -= staminaRegenDelta;
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
            ignoreDamage = false;
        }
    }
}




//Mask of the Abbatazzu: [insert Mask description]
//[insert Mask image][insert Mask lore]


//Effects:
//Gain + 20 Max Health Points each kill
//Deal -12 (+0.5% of additional Max Health)% Major/Minor scale damage.
//Attack speed is reduced by 16%
//Stamina regeneration decreased by 15%
//Shop items cost 15% less, but when you buy an item you receive 20% of the cost as damage.
//+300 Max Health points on pickup/selection
//Received damage is decreased by 6 (+0.32% of Max Health) %


//Special Ability:
//For the next 3 seconds you can’t receive damage (100 seconds cooldown).
