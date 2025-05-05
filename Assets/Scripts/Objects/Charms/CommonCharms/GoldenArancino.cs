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
            myHealthManager.AddHealthPoints(healthPointsIncrease);
            Debug.Log("increased health by: " + healthPointsIncrease + ", to: " + myHealthManager.currentHealth);
        }

        public override void Remove()
        {
            myHealthManager.AddHealthPoints(-healthPointsIncrease);
            Debug.Log("decreased health by: " + healthPointsIncrease + ", to: " + myHealthManager.currentHealth);
        }
    }
}