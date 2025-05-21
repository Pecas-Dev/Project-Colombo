using ProjectColombo.Combat;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class StaminaRegenPercentage : BaseAttributes
    {
        public float staminaRegenTimePercentage;

        float staminaRegenTimeValue;
        Stamina myStamina;

        public override void Enable()
        {
            myStamina = GameObject.Find("Player").GetComponent<PlayerStateMachine>().myStamina;
            staminaRegenTimeValue = myStamina.regenTime * staminaRegenTimePercentage / 100f;
            myStamina.regenTime += staminaRegenTimeValue;
            Debug.Log("changed stamina regen time by: " + staminaRegenTimeValue + " to " + myStamina.regenTime);
        }

        public override void Disable()
        {
            myStamina.regenTime -= staminaRegenTimeValue;
            Debug.Log("changed stamina regen time by: " + -staminaRegenTimeValue + " to " + myStamina.regenTime);
        }
    }
}