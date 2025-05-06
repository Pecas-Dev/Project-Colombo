using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public class SpeedPercentage : BaseAttributes
    {
        public float speedIncreasePercentage;

        float speedIncreaseValue;
        EntityAttributes myEntityAttributes;

        public override void Enable()
        {
            myEntityAttributes = GameObject.Find("Player").GetComponent<EntityAttributes>();
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