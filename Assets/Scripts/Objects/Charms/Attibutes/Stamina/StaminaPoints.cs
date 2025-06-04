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

            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myStamina = p.GetComponent<PlayerStateMachine>().myStamina;
            myStamina.AddStamina(addedStaminaPoints);
            Debug.Log("added " + addedStaminaPoints + " stamina");
        }

        public override void Disable()
        {

            GameObject p = GameObject.Find("Player");

            if (p == null) return;

            myStamina.AddStamina(-addedStaminaPoints);
            Debug.Log("removed " + addedStaminaPoints + " stamina");
        }
    }
}