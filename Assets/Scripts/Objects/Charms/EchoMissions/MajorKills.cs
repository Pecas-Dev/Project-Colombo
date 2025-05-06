using ProjectColombo.GameManagement.Events;

namespace ProjectColombo.Objects.Masks
{
    public class MajorKills : BaseEchoMissions
    {
        public int majorKillsToDo;
        int currentCollected = 0;

        public override void Enable()
        {
            CustomEvents.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                currentCollected ++;

                if (currentCollected >= majorKillsToDo)
                {
                    CompletedMission();
                }
            }
        }


        public override void Disable()
        {
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
        }
    }
}