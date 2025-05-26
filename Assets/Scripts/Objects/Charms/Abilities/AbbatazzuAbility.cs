using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class AbbatazzuAbility : BaseAbility
    {

        public override void UseAbility()
        {
            myPlayerStateMachine.myHealthManager.ignoreDamage = true;
            CustomEvents.AbilityUsed(abilitySoundName);
        }

        public override void EndAbility()
        {
            myPlayerStateMachine.myHealthManager.ignoreDamage = false;
        }
    }
}