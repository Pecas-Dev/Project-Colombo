using ProjectColombo.Combat;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class HealthPoints : BaseAttributes
    {
        public int extraHealthPoints;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myHealthManager = p.GetComponent<HealthManager>();
            Debug.Log("added extra health of: " + extraHealthPoints);
            myHealthManager.AddHealthPoints(extraHealthPoints);
        }

        public override void Disable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myHealthManager = p.GetComponent<HealthManager>();
            Debug.Log("removed extra health of: " + extraHealthPoints);
            myHealthManager.AddHealthPoints(-extraHealthPoints);
        }
    }
}