using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class AttackSpeedPercentage : BaseAttributes
    {
        public float attackSpeedPercentage;

        float attackSpeedValue;
        EntityAttributes myEntityAttributes;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.attackSpeedPercentage += attackSpeedPercentage;
        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            
            myEntityAttributes = p.GetComponent<EntityAttributes>();
            attackSpeedValue = myEntityAttributes.attackSpeed * attackSpeedPercentage / 100f;
            myEntityAttributes.attackSpeed += attackSpeedValue;
            Debug.Log("increased attack speed by: " + attackSpeedValue + " to: " + myEntityAttributes.attackSpeed);
        }

        public override void Disable()
        {

            GameObject p = GameObject.Find("Player");

            if (p == null) return;

            myEntityAttributes.attackSpeed -= attackSpeedValue;
            Debug.Log("decreased attack speed by: " + attackSpeedValue + " to: " + myEntityAttributes.attackSpeed);
        }
    }
}