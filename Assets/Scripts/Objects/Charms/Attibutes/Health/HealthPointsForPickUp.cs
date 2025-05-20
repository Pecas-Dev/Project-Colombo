using ProjectColombo.GameManagement.Events;
using UnityEngine;
using ProjectColombo.Combat;


namespace ProjectColombo.Objects.Charms
{
    public class HealthPointsForPickUp : BaseAttributes
    {
        public int healthForPickup;

        HealthManager myHealthManager;

        public override void Enable()
        {
            myHealthManager = GameObject.Find("Player").GetComponent<HealthManager>();
            CustomEvents.OnMaskCollected += OnMaskCollected;
            CustomEvents.OnCharmCollected += OnCharmCollected;
        }

        private void OnCharmCollected(GameObject obj)
        {
            AddHealth();
        }

        private void OnMaskCollected(GameObject obj)
        {
            AddHealth();
        }

        void AddHealth()
        {
            Debug.Log("extra health for pick up: " + healthForPickup);
            myHealthManager.Heal(healthForPickup);
        }

        public override void Disable()
        {
            CustomEvents.OnMaskCollected -= OnMaskCollected;
            CustomEvents.OnCharmCollected -= OnCharmCollected;
        }
    }
}