using ProjectColombo.Combat;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class GoldenArancino : BaseCharm
    {
        public int healthPointsIncrease = 100;

        public override void Equip()
        {
            GetComponentInParent<HealthManager>().AddHealthPoints(healthPointsIncrease);
        }

        public override void Remove()
        {
            GetComponentInParent<HealthManager>().AddHealthPoints(-healthPointsIncrease);
        }
    }
}