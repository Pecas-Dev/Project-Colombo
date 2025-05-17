using ProjectColombo.Combat;
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
            myStamina = FindFirstObjectByType<Stamina>();
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