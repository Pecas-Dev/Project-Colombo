using ProjectColombo.Combat;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class HealthPoints : BaseAttributes
    {
        public int extraHealthPoints;

        HealthManager myHealthManager;

        public override void Enable()
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
            Debug.Log("added extra health of: " + extraHealthPoints);
            myHealthManager.AddHealthPoints(extraHealthPoints);
        }

        public override void Disable()
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
            Debug.Log("removed extra health of: " + extraHealthPoints);
            myHealthManager.AddHealthPoints(-extraHealthPoints);
        }
    }
}