using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class MaskOfTheZanni : BaseMask
    {
        [Header("General Buffs")]
        public float majorDamageIncreasePercent = 6;
        public float majorDamageIncreasePerLuckPoint = 0.22f;
        public float minorDamageDecreasePercent = 8;
        public float receivedDamageIncreasePercent = 5;
        public float failedParryDamageIncreasePercent = 20;
        public float staminaRegenSpeedIncreasePercent = 25;
        float regenSpeedDelta;
        bool hasUsedDoubleStamina = false;
        public float chanceOfDoubleStamina = 50;
        public int luckPointsIncrease = 20;

        [Header("Echo Misson")]
        public int enemiesToKillWithMajor = 75;
        int currentKilledEnemies = 0;

        [Header("Upgraded Buffs after Echo")]
        public float majorDamageIncreasePercentEcho = 10f;
        public float majorDamageIncreasePerLuckPointEcho = 0.32f;
        public float staminaRegenSpeedIncreasePercentEcho = 30f;
        public int luckPointsIncreaseEcho = 30;

        [Header("Ability Stats")]
        public float defaultAbilityCooldown = 40f;
        public float cooldownDecreasePerLuck = 0.3f;
        public float abilityArea = 10f;

        PlayerStateMachine myPlayerStateMachine;

        public override void Equip()
        {
            myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();

            Debug.Log("added luck points: " + luckPointsIncrease);
            GameManager.Instance.GetComponent<PlayerInventory>().currentLuck += luckPointsIncrease;

            regenSpeedDelta = myPlayerStateMachine.myStamina.regenTime / 100f * staminaRegenSpeedIncreasePercent;
            Debug.Log("lowered stamina regen speed from: " + myPlayerStateMachine.myStamina.regenTime + " by: " + regenSpeedDelta);
            myPlayerStateMachine.myStamina.regenTime += regenSpeedDelta;

            CustomEvents.OnDamageDelt += AddDamageDelt;
            CustomEvents.OnDamageReceived += AddDamageReceive;
            CustomEvents.OnParryFailed += AddFailedParry;
            CustomEvents.OnStaminaUsed += AddStaminaUsed;
            CustomEvents.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale scale)
        {
            if (scale != GameGlobals.MusicScale.MAJOR) return;

            currentKilledEnemies++;

            if (currentKilledEnemies >= enemiesToKillWithMajor)
            {
                UnlockEcho();
            }
        }

        private void AddStaminaUsed()
        {
            if (hasUsedDoubleStamina) return;

            if (myPlayerStateMachine.currentComboString.Length >= 2 || myPlayerStateMachine.currentState == PlayerStateMachine.PlayerState.Roll)
            {
                Debug.Log("chance to double stamina for roll and final attack");
                int rand = Random.Range(0, 101);

                if (rand > chanceOfDoubleStamina)
                {
                    Debug.Log("double stamina");
                    hasUsedDoubleStamina = true;
                    myPlayerStateMachine.myStamina.TryConsumeStamina(1);
                }
            }
        }


        private void LateUpdate()
        {
            hasUsedDoubleStamina = false;
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

                int extra = (int)(damage * myPlayerStateMachine.myPlayerInventory.currentLuck * majorDamageIncreasePerLuckPoint);

                healthmanager.TakeDamage(value + extra);
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
            GameManager.Instance.GetComponent<PlayerInventory>().currentLuck -= luckPointsIncrease;
            myPlayerStateMachine.myStamina.regenTime -= regenSpeedDelta;

            CustomEvents.OnDamageDelt -= AddDamageDelt;
            CustomEvents.OnDamageReceived -= AddDamageReceive;
            CustomEvents.OnParryFailed -= AddFailedParry;
            CustomEvents.OnStaminaUsed -= AddStaminaUsed;
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
        }

        public override void UseAbility()
        {
            currentAbilityCooldown = defaultAbilityCooldown - (cooldownDecreasePerLuck * GameManager.Instance.GetComponent<PlayerInventory>().currentLuck);

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Debug.Log("enemies found: " + enemies.Length);

            foreach (GameObject e in enemies)
            {
                float distance = Vector3.Distance(e.transform.position, myPlayerStateMachine.transform.position);

                if (distance < abilityArea)
                {
                    Debug.Log("stunned enemy");
                    e.GetComponent<MommottiStateMachine>().SetStaggered();
                }

            }

            Debug.Log("end ability");
        }

        public override void UnlockEcho()
        {
            Remove();

            echoUnlocked = true;
            majorDamageIncreasePercent = majorDamageIncreasePercentEcho;
            majorDamageIncreasePerLuckPoint = majorDamageIncreasePerLuckPointEcho;
            staminaRegenSpeedIncreasePercent = staminaRegenSpeedIncreasePercentEcho;
            luckPointsIncrease = luckPointsIncreaseEcho;

            CustomEvents.OnEnemyDeath -= OnEnemyDeath;

            Equip();
        }
    }
}



//Effects:
//+6% Major scale damage (increases by 0.22% flat by each Luck point)
//-8% Minor scale damage
//+5% Received damage from all sources
//+20% Received damage from failed parry
//+20% Stamina regeneration speed
//50% chance of spending 2 stamina points on Last-Combo Attack and Roll (gets reduced by 0.6% flat by each Luck point)
//+20 Luck

//ECHO OF THE MASK:
//Kill 75 enemies with Major Attacks

//Awakened Stats:
//+10% Major scale damage (increases by 0.32% flat by each Luck point)
//+30% Stamina regeneration speed
//+30 Luck 

//Special Ability: Contagious Laughter
//On casting the ability, you unleash a contagious laugh that will stun all the enemy in a certain area around you [60 seconds cooldown - reduced by 0.3 seconds for each Luck point]