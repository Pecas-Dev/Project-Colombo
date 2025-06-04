using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class SpeedPercentage : BaseAttributes
    {
        public float speedIncreasePercentage;

        float speedIncreaseValue;
        EntityAttributes myEntityAttributes;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {
            stats.moveSpeedPercentage += speedIncreasePercentage;
        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myEntityAttributes = p.GetComponent<EntityAttributes>();
            speedIncreaseValue = myEntityAttributes.moveSpeed * speedIncreasePercentage / 100f;
            myEntityAttributes.moveSpeed += speedIncreaseValue;
            Debug.Log("changed player speed by " + speedIncreaseValue + " to " + myEntityAttributes.moveSpeed);
        }

        public override void Disable()
        {
            myEntityAttributes.moveSpeed -= speedIncreaseValue;
            Debug.Log("changed player speed by " + -speedIncreaseValue + " to " + myEntityAttributes.moveSpeed);
        }
    }
}