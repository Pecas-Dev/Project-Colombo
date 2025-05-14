using ProjectColombo.Enemies;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class BossRoomBehaviou : MonoBehaviour
    {
        TileWorldChamber myChamber;

        private void Start()
        {
            myChamber = GetComponent<TileWorldChamber>();
            myChamber.ActivateChamber();
            GetComponent<EnemyAttackPriority>().Activate();
        }
    }
}