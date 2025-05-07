using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class AbbatazzuAbility : BaseAbility
    {

        public override void UseAbility()
        {
            myPlayerStateMachine.myHealthManager.ignoreDamage = true;
        }

        public override void EndAbility()
        {
            myPlayerStateMachine.myHealthManager.ignoreDamage = false;
        }
    }
}