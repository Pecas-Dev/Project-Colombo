using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class Cornino : BaseCharm
    {
        public int luckIncrease = 8;
        EntityAttributes myEntityAttributes;

        public override void Equip()
        {
            myEntityAttributes = GameObject.Find("Player").GetComponent<EntityAttributes>();
            myEntityAttributes.currentLuck += luckIncrease;
        }

        public override void Remove()
        {
            myEntityAttributes.currentLuck -= luckIncrease;
        }
    }
}