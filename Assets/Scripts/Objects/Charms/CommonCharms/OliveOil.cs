using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class OliveOil : BaseCharm
    {
        public int movementSpeedIncrease = 8;
        float defaultMoveSpeed;

        public override void Equip()
        {
            defaultMoveSpeed = GetComponentInParent<EntityAttributes>().moveSpeed;
            GetComponentInParent<EntityAttributes>().moveSpeed += defaultMoveSpeed / 100 * movementSpeedIncrease;
        }

        public override void Remove()
        {
            GetComponentInParent<EntityAttributes>().moveSpeed = defaultMoveSpeed;
        }
    }
}