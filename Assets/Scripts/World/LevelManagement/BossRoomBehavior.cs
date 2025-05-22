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
        }

        private void EndBossFight(GameGlobals.MusicScale arg1, GameObject arg2)
        {
            endLevelTrigger.SetActive(true);
            CustomEvents.OnEnemyDeath -= EndBossFight;
        }
    }
}