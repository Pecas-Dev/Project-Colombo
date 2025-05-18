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
            myPlayerStateMachine.myHealthManager.Heal(healAmount);

            int missingHealth = myPlayerStateMachine.myHealthManager.MaxHealth - myPlayerStateMachine.myHealthManager.currentHealth;
            int value = (int)(missingHealth * healPercentage / 100f);

            myPlayerStateMachine.myHealthManager.Heal(value);
        }

        public override void EndAbility()
        {
           
        }
    }
}