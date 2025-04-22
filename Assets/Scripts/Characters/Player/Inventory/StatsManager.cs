using UnityEngine;

namespace ProjectColombo.Inventory
{
    public class StatsManager : MonoBehaviour
    {
        public int defaultLuck = 0;
        [HideInInspector] public int currentLuck;               //-> set somewhere

        public int defaultDoubleGoldChance = 0;                 //-> in event on currency pickup
        [HideInInspector] public int currentDoubleGoldChance;

        public int defaultSpeedIncrease = 0;                    //-> entity attrib
        [HideInInspector] public int currentSpeedIncrease;


        //DEFENSE
        public int defaultHealthBoost = 0;                      //-> health manager
        [HideInInspector] public int currentHealthBoost;

        public int defaultBlockStrengthIncrease = 0;            //-> weapon attrib
        [HideInInspector] public int currentBlockStrenghtIncrease;

        public int defaultResistance = 0;                       //-> weapon attrib
        [HideInInspector] public int currentResistance;

        public int defaultDamageAfterFailedParryResistance = 0;     //-> weapon attrib
        [HideInInspector] public int currentDamageAfterFailedParryResistance;

        public int defaultEvadeChance = 0;                      //-> in event on damage received
        [HideInInspector] public int currentEvadeChance;


        //STAMINA
        public int defaultExtraStamina = 0;                     //-> stamina
        [HideInInspector] public int currentExtraStamina;

        public int defaultStaminaRegenBoost = 0;
        [HideInInspector] public int currentStaminaRegenBoost;

        public int defaultChanceForDoubleStamina = 0;           //-> in event on stamina used
        [HideInInspector] public int currentChanceForDoubleStamina;


        //ATTACK
        public int defaultAttackSpeedBoost = 0;                 //-> weapon attrib
        [HideInInspector] public int currentAttackSpeedBoost;

        public int defaultDamageBoost = 0;
        [HideInInspector] public int currentDamageBoost;

        public int defaultMajorDamageBoost = 0;
        [HideInInspector] public int currentMajorDamageBoost;

        public int defaultMinorDamageBoost = 0;
        [HideInInspector] public int currentMinorDamageBoost;
    }
}