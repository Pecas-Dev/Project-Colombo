using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class OliveOil : BaseCharm
    {
        public int movementSpeedIncrease = 8;
        float value;

        public override void Equip()
        {
            value = GetComponentInParent<EntityAttributes>().moveSpeed / 100 * movementSpeedIncrease;
            GetComponentInParent<EntityAttributes>().moveSpeed += value;
        }

        public override void Remove()
        {
            GetComponentInParent<EntityAttributes>().moveSpeed -= value;
        }
    }
}