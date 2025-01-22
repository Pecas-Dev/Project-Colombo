using UnityEngine;
using System.Collections.Generic;
using System.Xml.XPath;

public class EnemySpawnPoint : MonoBehaviour
{
    [HideInInspector] public bool cleared = false;
    public GameObject enemyType;
    public int minAmountEnemies;
    public int maxAmountEnemies;

    //[HideInInspector]
    public List<GameObject> spawnGroup;
    public string pathName;

    private void Start()
    {
        int amountOfEnemies = Random.Range(minAmountEnemies, maxAmountEnemies+1);

        if (enemyType == null || amountOfEnemies == 0)
        {
            cleared = true;
            return;
        }

        float spaceX = transform.position.x - amountOfEnemies / 2;

        for (int i = 0; i < amountOfEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(spaceX + i, transform.position.y, transform.position.z);
            GameObject newEnemy = Instantiate(enemyType, spawnPosition, transform.rotation);
            spawnGroup.Add(newEnemy);
            newEnemy.GetComponent<EnemyAttributes>().pathToFollowName = pathName;
            newEnemy.GetComponent<EnemyAttributes>().patrolMode = PatrolMode.RANDOM;
        }
    }

    private void Update()
    {
        if (spawnGroup.Count <= 0)
        {
            cleared = true;
        }
    }
}
