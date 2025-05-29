using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.Inventory;
using System.Collections;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class BossRoomBehavior : MonoBehaviour
    {
        public GameObject endLevelTrigger;

        private void Start()
        {
            CustomEvents.OnEnemyDeath += EndBossFight;
            StartCoroutine(StartDelay());
        }

        IEnumerator StartDelay()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            GameManager.Instance.GetComponent<PlayerInventory>().DeactivateCharms();
            GameManager.Instance.GetComponent<PlayerInventory>().DeactivateMask();
            GameManager.Instance.GetComponent<PlayerInventory>().ActivateCharms();
            GameManager.Instance.GetComponent<PlayerInventory>().ActivateMask();
            CustomEvents.ChamberActivated();
        }

        private void EndBossFight(GameGlobals.MusicScale arg1, GameObject arg2)
        {
            CustomEvents.ChamberFinished();
            endLevelTrigger.SetActive(true);
            CustomEvents.OnEnemyDeath -= EndBossFight;
        }

        public void StartBossFight()
        {
            CustomEvents.StartBossfight();
        }
    }
}