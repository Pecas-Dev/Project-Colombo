using ProjectColombo.Combat;
using ProjectColombo.StateMachine.Player;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class GoldenArancino : BaseCharm
    {
        public int healthPointsIncrease = 100;
        HealthManager myHealthManager;

        public override void Equip()
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
            Debug.Log("increased health from: " + myHealthManager.MaxHealth + ", by: " + healthPointsIncrease);
            myHealthManager.AddHealthPoints(healthPointsIncrease);
        }

        public override void Remove()
        {
            myHealthManager.AddHealthPoints(-healthPointsIncrease);
        }
    }
}