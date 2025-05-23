using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class BossRoomBehavior : MonoBehaviour
    {
        public GameObject endLevelTrigger;

        private void Start()
        {
            CustomEvents.OnEnemyDeath += EndBossFight;
            CustomEvents.ChamberActivated();
        }

        private void EndBossFight(GameGlobals.MusicScale arg1, GameObject arg2)
        {
            CustomEvents.ChamberFinished();
            endLevelTrigger.SetActive(true);
            CustomEvents.OnEnemyDeath -= EndBossFight;
        }
    }
}