using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class PotionAbility : BaseAbility
    {
        [Header("Potion")]
        public int healAmount;

        public override void UseAbility()
        {
            myPlayerStateMachine.myHealthManager.Heal(healAmount);
        }

        public override void EndAbility()
        {
           
        }
    }
}