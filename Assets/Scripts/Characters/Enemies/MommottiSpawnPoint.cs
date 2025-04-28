using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.LevelManagement;
using ProjectColombo.StateMachine.Mommotti;
using UnityEngine;


namespace ProjectColombo.Enemies.Mommotti
{
    public class MommottiSpawnPoint : MonoBehaviour
    {
        public GameObject enemyType;
        public int minAmountEnemies;
        public int maxAmountEnemies;
        public GameGlobals.MusicScale scale;
        public float patrolAreaDistance;

        [HideInInspector] public GridManager myGridManager;

        private void Start()
        {
            TileWorldChamber myChamber = GetComponentInParent<TileWorldChamber>();
            myGridManager = myChamber.GetComponentInChildren<GridManager>();

            if (myGridManager == null)
            {
                Debug.Log("Mommotti Spawn Point grid == null");
            }

            int amountOfEnemies = Random.Range(minAmountEnemies, maxAmountEnemies + 1);

            if (enemyType == null || amountOfEnemies == 0)
            {
                return;
            }

            float spaceX = transform.position.x - amountOfEnemies / 2;

            for (int i = 0; i < amountOfEnemies; i++)
            {
                Vector3 spawnPosition = new Vector3(spaceX + i, 1, transform.position.z);
                GameObject newEnemy = Instantiate(enemyType, spawnPosition, transform.rotation);
                newEnemy.GetComponent<MommottiAttributes>().spawnPointLocation = transform.position;
                newEnemy.GetComponent<MommottiAttributes>().patrolAreaDistance = patrolAreaDistance;
                newEnemy.GetComponent<MommottiAttributes>().myGridManager = myGridManager;

                newEnemy.GetComponent<MommottiStateMachine>().SetScale(scale);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            if (scale == GameGlobals.MusicScale.MINOR)
            {
                Gizmos.color = Color.red;
            }
            else if (scale == GameGlobals.MusicScale.MAJOR)
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawWireSphere(transform.position, patrolAreaDistance);
        }
    }
}
