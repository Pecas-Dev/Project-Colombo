using ProjectColombo.Combat;
using ProjectColombo.GameManagement.Events;
using UnityEngine;


namespace ProjectColombo.Objects.Charms
{
    public class HealthPointsForKill : BaseAttributes
    {
        public int healthPointsForKill;

        HealthManager myHealthManager;

        public override void UpdateStatSheed(AttributesStatSheet stats)
        {

        }

        public override void Enable()
        {
            GameObject p = GameObject.Find("Player");

            if (p == null) return;
            myHealthManager = p.GetComponent<HealthManager>();
            CustomEvents.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale obj, GameObject enemy)
        {
            if (eventHandled) return;
            eventHandled = true;
            StartCoroutine(ResetEventHandled());

            CustomEvents.MaxHealthIncreased(healthPointsForKill);
            myHealthManager.AddHealthPoints(healthPointsForKill);
        }

        public override void Disable()
        {
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
        }
    }
}