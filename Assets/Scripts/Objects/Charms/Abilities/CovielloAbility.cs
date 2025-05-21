using ProjectColombo.GameManagement.Events;
using ProjectColombo.Objects.Charms;
using System.Collections;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class CovielloAbility : BaseAbility
    {
        [Header("Coviello")]
        int staminaCounter;
        int abilityExtraDamageCounter = 0;
        public float extraDamageForStaminaPercent = 5;
        public int extraDamageDuration = 8;

        public GameObject attribs;
        BaseAttributes[] myAttributes;

        public int removeHealthPerAttack = 20;


        public override void UseAbility()
        {
            myAttributes = attribs.GetComponents<BaseAttributes>();

            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Enable();
            }

            staminaCounter = 0;

            CustomEvents.OnStaminaUsed += OnStaminaUsed;
            CustomEvents.OnDamageDelt += OnDamageDelt;
        }

        private void OnDamageDelt(int amount, GameGlobals.MusicScale arg2, bool arg3, Combat.HealthManager healthmanager, int arg5)
        {
            int value = (int)(amount * extraDamageForStaminaPercent / 100f);
            int multiplied = abilityExtraDamageCounter * value;
            healthmanager.TakeDamage(multiplied);
        }

        public override void EndAbility()
        {
            foreach (BaseAttributes attrib in myAttributes)
            {
                attrib.Disable();
            }

            CustomEvents.OnStaminaUsed -= OnStaminaUsed;
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }

        private void OnStaminaUsed()
        {
            staminaCounter++;

            if (staminaCounter % 2 == 0)
            {
                StartCoroutine(AbilityExtraDamage());
            }
        }


        IEnumerator AbilityExtraDamage()
        {
            Debug.Log("start extra damage");
            abilityExtraDamageCounter++;
            yield return new WaitForSeconds(extraDamageDuration);
            abilityExtraDamageCounter--;
            Debug.Log("end extra damage. Remaining: " + abilityExtraDamageCounter);
        }
    }
}



//Special Ability: Skill Showdown
//For next 6 seconds:
//every 2 stamina points used, you get +5% damage for 8 seconds (stacks up to 25%)
//each attack consumes 20 Health Points 
//you gain +10% attack speed 
//you gain +8% movement speed.