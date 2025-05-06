using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class AttackSpeedPercentage : BaseAttributes
    {
        public float attackSpeedPercentage;

        float attackSpeedValue;
        EntityAttributes myEntityAttributes;

        public override void Enable()
        {
            myEntityAttributes = GameObject.Find("Player").GetComponent<EntityAttributes>();
            attackSpeedValue = myEntityAttributes.attackSpeed * attackSpeedPercentage / 100f;
            myEntityAttributes.attackSpeed += attackSpeedValue;
            Debug.Log("increased attack speed by: " + attackSpeedValue + " to: " + myEntityAttributes.attackSpeed);
        }

        public override void Disable()
        {
            myEntityAttributes.attackSpeed -= attackSpeedValue;
            Debug.Log("decreased attack speed by: " + attackSpeedValue + " to: " + myEntityAttributes.attackSpeed);
        }
    }
}