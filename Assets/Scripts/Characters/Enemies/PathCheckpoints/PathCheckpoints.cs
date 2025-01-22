using UnityEngine;

public class PathCheckpoints : MonoBehaviour
{
    public enum CheckpointAction
    {
        None,
        LookAround,
        Wait
    }

    public CheckpointAction selectedAction;
    public float actionDuration;
    public GameObject currentEnemy;
    float currentTimer;

    void EndAction()
    {
        currentTimer = 0;
        currentEnemy.GetComponent<EnemyAttributes>().onCheckpoint = false;
        //currentEnemy.GetComponent<EnemyAttributes>().NextControllPoint();
    }

    public void None()
    {
        EndAction();
    }
    public void LookAround()
    {
        int direction = 1; //rotate to right
        currentTimer += Time.deltaTime;

        if (currentTimer < actionDuration / 2)
        {
            direction = -1; //rotate to left in first half
        }

        currentEnemy.transform.Rotate(Vector3.up, direction + 20f * Time.deltaTime);

        if (currentTimer > actionDuration)
        {
            EndAction();
        }
    }

    public void Wait()
    {
        currentTimer += Time.deltaTime;

        if (currentTimer > actionDuration)
        {
            EndAction();
        }
    }

    public void TriggerAction()
    {
        switch (selectedAction)
        {
            case CheckpointAction.None:
                Debug.Log("selected action: none");
                None();
                break;
            case CheckpointAction.LookAround:
                Debug.Log("selected action: look around");
                LookAround();
                break;
            case CheckpointAction.Wait:
                Debug.Log("selected action: wait");
                Wait();
                break;
            default:
                Debug.Log("no action selected");
                break;
        }
    }
}
