using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class OliveOil : BaseCharm
    {
        public float movementSpeedIncrease = 8;
        float value;
        EntityAttributes myEntityAttributes;
        public override void Equip()
        {
            myEntityAttributes = GameObject.Find("Player").GetComponent<EntityAttributes>();
            value = myEntityAttributes.moveSpeed / 100 * movementSpeedIncrease;
            myEntityAttributes.moveSpeed += value;
        }

        public override void Remove()
        {
            myEntityAttributes.moveSpeed -= value;
        }
    }
}