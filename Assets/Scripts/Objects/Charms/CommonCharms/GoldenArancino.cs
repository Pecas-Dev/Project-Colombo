using ProjectColombo.Combat;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class GoldenArancino : BaseCharm
    {
        public int healthIncrease = 100;

        public override void Equip()
        {
            GetComponentInParent<HealthManager>().AddHealthPoints(healthIncrease);
        }

        public override void Remove()
        {
            GetComponentInParent<HealthManager>().AddHealthPoints(-healthIncrease);
        }
    }
}