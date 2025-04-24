using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfTheZanni : BaseMask
    {
        //general
        public float majorDamageIncreasePercent = 6;
        public float minorDamageDecreasePercent = 8;
        public float receivedDamageIncreasePercent = 5;
        public float failedParryDamageIncreasePercent = 20;
        public float staminaRegenSpeedIncreasePercent = 25;
        float regenSpeedDelta;
        public float chanceOfDoubleStamina = 50;
        public int luckPointsIncrease = 20;

        //ability
        public float defaultAbilityCooldown = 40f;
        public float cooldownDecreasePerLuck = 0.3f;
        public float abilityArea = 10f;

        PlayerStateMachine myPlayerStateMachine;

        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

            Debug.Log("added luck points: " + luckPointsIncrease);
            myPlayerStateMachine.myEntityAttributes.currentLuck += luckPointsIncrease;

            regenSpeedDelta = myPlayerStateMachine.myStamina.regenSpeed / 100f * staminaRegenSpeedIncreasePercent;
            Debug.Log("lowered stamina regen speed from: " + myPlayerStateMachine.myStamina.regenSpeed + " by: " + regenSpeedDelta);
            myPlayerStateMachine.myStamina.regenSpeed += regenSpeedDelta;

            CustomEvents.OnDamageDelt += AddDamageDelt;
            CustomEvents.OnDamageReceived += AddDamageReceive;
            CustomEvents.OnParryFailed += AddFailedParry;
            CustomEvents.OnStaminaUsed += AddStaminaUsed;
        }

        private void AddStaminaUsed()
        {
            if (myPlayerStateMachine.currentComboString.Length >= 2 || myPlayerStateMachine.currentState == PlayerStateMachine.PlayerState.Roll)
            {
                Debug.Log("chance to double stamina for roll and final attack");
                int rand = Random.Range(0, 101);

                if (rand > chanceOfDoubleStamina)
                {
                    Debug.Log("double stamina");
                    myPlayerStateMachine.myStamina.TryConsumeStamina(1);
                }
            }
        }

        private void AddFailedParry(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager, bool sameScale)
        {
            int value = (int)(damage * failedParryDamageIncreasePercent / 100f);
            Debug.Log("increase failed parry punishment: " + value);
            healthmanager.TakeDamage(value);
        }

        private void AddDamageReceive(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            int value = (int)(damage * receivedDamageIncreasePercent / 100f);
            Debug.Log("increase receive damage: " + value);
            healthmanager.TakeDamage(value);
        }

        private void AddDamageDelt(int damage, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                int value = (int)(damage * majorDamageIncreasePercent / 100f);
                Debug.Log("increase major damage by: " + value);
                healthmanager.TakeDamage(value);
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                int value = (int)(damage * minorDamageDecreasePercent / 100f);
                Debug.Log("decrease minor damage by: " + value);
                healthmanager.TakeDamage(-value);
            }
        }

        public override void Remove()
        {
            myPlayerStateMachine.myEntityAttributes.currentLuck -= luckPointsIncrease;
            myPlayerStateMachine.myStamina.regenSpeed -= regenSpeedDelta;

            CustomEvents.OnDamageDelt -= AddDamageDelt;
            CustomEvents.OnDamageReceived -= AddDamageReceive;
            CustomEvents.OnParryFailed -= AddFailedParry;
            CustomEvents.OnStaminaUsed -= AddStaminaUsed;
        }

        public override void UseAbility()
        {
            currentAbilityCooldown = defaultAbilityCooldown - (cooldownDecreasePerLuck * myPlayerStateMachine.myEntityAttributes.currentLuck);

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemies");

            foreach (GameObject e in enemies)
            {
                float distance = (e.transform.position - transform.position).magnitude;

                if (distance < abilityArea)
                {
                    Debug.Log("stunned enemy");
                    e.GetComponent<MommottiStateMachine>().SetStaggered();
                }
            }

            Debug.Log("end ability");
        }
    }
}



//+6 % Major scale damage(increases by 0.22% flat by each Luck point)
//-8 % Minor scale damage
//+5% Received damage from all sources
//+20% Received damage from failed parry
//50% chance of spending 2 stamina points on Last-Combo Attack and Roll (gets reduced by 0.6% flat by each Luck point)


//Special Ability:
//On casting the ability, you unleash a contagious laugh that will stun all the enemy in a certain area around you [40 seconds cooldown - reduced by 0.3 seconds for each Luck point]
