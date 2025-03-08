using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;


namespace ProjectColombo.Enemies.Mommotti
{
    public class MommottiSpawnPoint : MonoBehaviour
    {
        public GameObject enemyType;
        public int minAmountEnemies;
        public int maxAmountEnemies;

        public float patrolAreaDistance;

        [HideInInspector] public GridManager myGridManager;

        private void Start()
        {
            myGridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
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
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(transform.position, patrolAreaDistance);
        }
    }
}
