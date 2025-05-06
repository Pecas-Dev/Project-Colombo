using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Objects.Masks
{
    public class CollectGold : BaseEchoMissions
    {
        public int goldToUnlockEcho;
        int currentCollected = 0;

        public override void Enable()
        {
            CustomEvents.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnCoinsCollected(int amount)
        {
            currentCollected += amount;

            if (currentCollected >= goldToUnlockEcho)
            {
                CompletedMission();
            }
        }

        public override void Disable()
        {
            CustomEvents.OnCoinsCollected -= OnCoinsCollected;
        }
    }
}