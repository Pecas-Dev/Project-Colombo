using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class MajorKills : BaseEchoMissions
    {
        public int majorKillsToDo;
        int currentCollected = 0;

        public override void Enable()
        {
            if (isEnabled) return;
            isEnabled = true;

            CustomEvents.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(GameGlobals.MusicScale scale, GameObject enemy)
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
            isEnabled = false;
            CustomEvents.OnEnemyDeath -= OnEnemyDeath;
        }

        public override void ResetProgress()
        {
            currentCollected = 0;
        }
    }
}