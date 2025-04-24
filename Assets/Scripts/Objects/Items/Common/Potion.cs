using ProjectColombo.Combat;
using UnityEngine;

namespace ProjectColombo.Objects.Items
{
    public class Potion : BaseItem
    {
        public int healPoints = 100;
        public float healPercentage = 15;

        public override void Activate()
        {
            HealthManager playerHealth = GameObject.Find("Player").GetComponent<HealthManager>();

            playerHealth.Heal(healPoints);

            float value = (playerHealth.MaxHealth - playerHealth.currentHealth) / 100f * healPercentage;
            Debug.Log("healed by: " + healPoints+ ", and " + value);
            playerHealth.Heal((int)(value));
        }
    }
}