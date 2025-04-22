using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class Cornino : BaseCharm
    {
        public int luckIncrease = 8;

        public override void Equip()
        {
            GetComponentInParent<EntityAttributes>().currentLuck += luckIncrease;
        }

        public override void Remove()
        {
            GetComponentInParent<EntityAttributes>().currentLuck -= luckIncrease;
        }
    }
}