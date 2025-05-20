using ProjectColombo.GameManagement.Events;
using ProjectColombo.StateMachine.Player;
using ProjectColombo.Tutorial;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class PotionAbility : BaseAbility
    {
        [Header("Potion")]
        public int healAmount;
        public float healPercentage;

        public override void UseAbility()
        {
            if (myPlayerStateMachine == null)
            {
                myPlayerStateMachine = GameObject.Find("Player").GetComponent<PlayerStateMachine>();
            }

            myPlayerStateMachine.myHealthManager.Heal(healAmount);

            int missingHealth = myPlayerStateMachine.myHealthManager.MaxHealth - myPlayerStateMachine.myHealthManager.currentHealth;
            int value = (int)(missingHealth * healPercentage / 100f);

            myPlayerStateMachine.myHealthManager.Heal(value);

            CustomEvents.PotionUsed();
        }

        public override void EndAbility()
        {
           
        }
    }
}