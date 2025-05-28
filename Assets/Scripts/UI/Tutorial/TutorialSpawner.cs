using ProjectColombo;
using ProjectColombo.Enemies.Mommotti;
using ProjectColombo.Enemies.Pathfinding;
using ProjectColombo.StateMachine.Mommotti;
using System.Collections;
using UnityEngine;

public class TutorialSpawner : MonoBehaviour
{
    public GameObject enemyType;
    public float patrolAreaDistance = 0;
    public GridManager myGridManager;

    public GameGlobals.MusicScale scale;

    public void SpawnEnemy()
    {
        StartCoroutine(UpdateGrid());
        Vector3 spawnPosition = new Vector3(transform.position.x, 1, transform.position.z);
        GameObject newEnemy = Instantiate(enemyType, spawnPosition, transform.rotation, this.transform);
        newEnemy.GetComponent<MommottiAttributes>().spawnPointLocation = transform.position;
        newEnemy.GetComponent<MommottiAttributes>().patrolAreaDistance = patrolAreaDistance;
        newEnemy.GetComponent<MommottiAttributes>().myGridManager = myGridManager;

        newEnemy.GetComponent<MommottiStateMachine>().SetScale(scale);
    }

    IEnumerator UpdateGrid()
    {
        yield return new WaitForEndOfFrame();
        myGridManager.CreateGrid();
    }
}
