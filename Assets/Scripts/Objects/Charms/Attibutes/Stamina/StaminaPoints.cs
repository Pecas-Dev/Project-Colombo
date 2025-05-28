using ProjectColombo.Combat;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class StaminaPoints : BaseAttributes
    {
        public int addedStaminaPoints;

        Stamina myStamina;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            myStamina = GameObject.Find("Player").GetComponent<PlayerStateMachine>().myStamina;
            myStamina.AddStamina(addedStaminaPoints);
            Debug.Log("added " + addedStaminaPoints + " stamina");
        }

        public override void Disable()
        { 
            myStamina.AddStamina(-addedStaminaPoints);
            Debug.Log("removed " + addedStaminaPoints + " stamina");
        }
    }
}