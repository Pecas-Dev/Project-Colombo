using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Objects.Masks
{
    public class CollectMaxHealth : BaseEchoMissions
    {
        public int maxHealthToCollect;
        int currentCollected = 0;

        public override void Enable()
        {
            CustomEvents.OnMaxHealthGained += OnMaxHealthGained;
        }

        private void OnMaxHealthGained(int value)
        {
            currentCollected += value;

            if (currentCollected >= maxHealthToCollect)
            {
                CompletedMission();
            }
        }

        public override void Disable()
        {
            CustomEvents.OnMaxHealthGained -= OnMaxHealthGained;
        }

        public override void ResetProgress()
        {
            currentCollected = 0;
        }
    }
}