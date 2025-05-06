using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Objects.Masks
{
    public class DamageDelt : BaseEchoMissions
    {
        public int damageToDeal;
        int currentCollected = 0;

        public override void Enable()
        {
            CustomEvents.OnDamageDelt += OnDamageDelt;
        }

        private void OnDamageDelt(int value, GameGlobals.MusicScale scale, Combat.HealthManager healthmanager)
        {
            currentCollected += value;

            if (currentCollected >= damageToDeal)
            {
                CompletedMission();
            }
        }


        public override void Disable()
        {
            CustomEvents.OnDamageDelt -= OnDamageDelt;
        }
    }
}