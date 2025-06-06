using ProjectColombo.GameManagement.Events;
using UnityEngine;
using ProjectColombo.Combat;


namespace ProjectColombo.Objects.Charms
{
    public class HealthPointsForPickUp : BaseAttributes
    {
        public int healthForPickup;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myHealthManager = p.GetComponent<HealthManager>();
            CustomEvents.OnMaskCollected += OnMaskCollected;
            CustomEvents.OnCharmFirstTimeEquipped += OnCharmCollected;
        }

        private void OnCharmCollected()
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            AddHealth();
        }

        private void OnMaskCollected(GameObject obj)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            AddHealth();
        }

        void AddHealth()
        {
            Debug.Log("extra health for pick up: " + healthForPickup);
            myHealthManager.AddHealthPoints(healthForPickup);
        }

        public override void Disable()
        {
            CustomEvents.OnMaskCollected -= OnMaskCollected;
            CustomEvents.OnCharmFirstTimeEquipped -= OnCharmCollected;
        }
    }
}