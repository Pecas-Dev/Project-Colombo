using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnPoint : MonoBehaviour
{
    [HideInInspector] public bool cleared = false;
    public GameObject enemyType;
    public int amountOfEnemies;
    //[HideInInspector]
    public List<GameObject> spawnGroup;

    private void Start()
    {
        if (enemyType == null || amountOfEnemies == 0)
        {
            cleared = true;
            return;
        }

        for (int i = 0; i < amountOfEnemies; i++)
        {
            GameObject newEnemy = Instantiate(enemyType, transform.position, transform.rotation);
            spawnGroup.Add(newEnemy);
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
