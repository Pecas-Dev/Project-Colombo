using NUnit.Framework;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class EnemyAttributes : MonoBehaviour
{
    [Header("Player Detection")]
    public float distanceToNoticePlayer;
    public float distanceToSwitchPatrolPoints;
    [HideInInspector] public EntityAttributes myAttributes;
    WeaponAttributes currentWeapon;
    GameObject player;


    //current target
    [HideInInspector] public GameObject currentTarget; //walk to patrol point or player if chasing;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentRotationSpeed;

    //alerted by player
    [Header("Alerted")]
    public float alertedCheckTime; //delay after noticed player once to check again
    float timer;
    float currentDistanceToPlayer;
    [HideInInspector] public bool isChasing; //public because maybe alert other enemies

    //patroling
    [Header("Patroling")]
    public bool onCheckpoint;
    public string pathToFollowName;
    List<GameObject> patrolPath;
    float currentDistanceToPatrolPoint;
    int currentPatrolPoint;

    //field of view detection
    [Header("Field of View Detection")]
    public bool enableFOVDetection;
    public float rangeFOV;
    public float angleFOV;


    //sound detection
    [Header("Sound Detection")]
    public List<EntityAttributes.EntityState> noDetectionState;
    public bool enableSoundDetection;
    public float rangeSD;

    private void Start()
    {
        myAttributes = GetComponent<EntityAttributes>();
        onCheckpoint = false;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("no player found");
        }


        if (pathToFollowName.Length != 0)
        {
            AddPatrolPath();
            currentPatrolPoint = 0;
            currentTarget = patrolPath[currentPatrolPoint];
        }
        else
        {
            currentTarget = this.gameObject;
        }

        currentSpeed = myAttributes.walkSpeed;
        currentRotationSpeed = myAttributes.rotationSpeed;
        currentWeapon = GetComponentInChildren<WeaponAttributes>();
    }

    private void Update()
    {
        currentDistanceToPlayer = (player.transform.position - transform.position).magnitude;

        switch (myAttributes.currentState)
        {
            case EntityAttributes.EntityState.PATROL:
                UpdatePatrol();
                break;
            case EntityAttributes.EntityState.ALERTED:
                UpdateAlerted();
                break;
            case EntityAttributes.EntityState.CHASE:
                UpdateChase();
                break;
            default:
                UpdateIdle();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (patrolPath != null && patrolPath.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPath.Count; i++)
            {
                Vector3 current = patrolPath[i].transform.position;
                Vector3 next = patrolPath[(i + 1) % patrolPath.Count].transform.position;
                Gizmos.DrawLine(current, next);
            }
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTarget.transform.position, 0.5f);
        }

        //draw fov
        if (enableFOVDetection)
        {
            Gizmos.color = Color.yellow;
            //range
            Gizmos.DrawWireSphere(transform.position, rangeFOV);

            //angles
            Vector3 forward = transform.forward;
            float halfFOV = angleFOV / 2;

            Quaternion leftRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRotation = Quaternion.Euler(0, halfFOV, 0);

            Vector3 leftDirection = leftRotation * forward;
            Vector3 rightDirection = rightRotation * forward;

            Gizmos.DrawLine(transform.position, transform.position + leftDirection * rangeFOV);
            Gizmos.DrawLine(transform.position, transform.position + rightDirection * rangeFOV);
        }

        //draw sound detection
        if (enableSoundDetection)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rangeSD);
        }
    }


    private void AddPatrolPath()
    {
        GameObject path = GameObject.Find(pathToFollowName);
        patrolPath = new List<GameObject>();
        patrolPath.Clear();

        foreach (Transform child in path.transform)
        {
            patrolPath.Add(child.gameObject);
        }
    }

    public void NextControllPoint()
    {
        currentPatrolPoint = (currentPatrolPoint + 1) % patrolPath.Count;
    }

    private void SwitchIdle()
    {
        myAttributes.currentState = EntityAttributes.EntityState.IDLE;
        currentTarget = this.gameObject;
        currentSpeed = 0;
    }

    private void UpdateIdle()
    {
        if (enableSoundDetection)
        {
            if (SoundDetectionCheck())
            {
                SwitchAlerted();
            }
        }
        else if (enableFOVDetection)
        {
            if (FieldOfViewCheck())
            {
                SwitchAlerted();
            }
        }
        else if (currentDistanceToPlayer < distanceToNoticePlayer)
        {
            SwitchAlerted();
        }

        if (pathToFollowName.Length != 0)
        {
            SwitchPatrol();
        }
    }

    private void SwitchPatrol()
    {
        myAttributes.currentState = EntityAttributes.EntityState.PATROL;
        currentTarget = patrolPath[currentPatrolPoint];
        onCheckpoint = false;
        currentSpeed = myAttributes.walkSpeed;
    }

    private void UpdatePatrol()
    {
        currentDistanceToPatrolPoint = (currentTarget.transform.position - transform.position).magnitude;

        if (currentDistanceToPatrolPoint < distanceToSwitchPatrolPoints)
        {
            if (!onCheckpoint)
            {
                onCheckpoint = true;
            }
            else
            {
                if (patrolPath[currentPatrolPoint].GetComponent<PathCheckpoints>() != null)
                {
                    patrolPath[currentPatrolPoint].GetComponent<PathCheckpoints>().currentEnemy = this.gameObject;
                    patrolPath[currentPatrolPoint].GetComponent<PathCheckpoints>().TriggerAction();
                }
                else
                {
                    Debug.Log("no path checkpoint script");
                    onCheckpoint = false;
                    NextControllPoint();
                }
            }
        }

        currentTarget = patrolPath[currentPatrolPoint];

        if (enableSoundDetection)
        {
            if (SoundDetectionCheck())
            {
                onCheckpoint = false;
                SwitchAlerted();
            }
        }
        
        if (enableFOVDetection)
        {
            if (FieldOfViewCheck())
            {
                onCheckpoint = false;
                SwitchAlerted();
            }
        }
        else if (currentDistanceToPlayer < distanceToNoticePlayer)
        {
            onCheckpoint = false;
            SwitchAlerted();
        }
    }

    private void SwitchAlerted()
    {
        myAttributes.currentState = EntityAttributes.EntityState.ALERTED;
        currentTarget = player;
        timer = 0;
        currentSpeed = 0;
    }

    private void UpdateAlerted()
    {
        timer += Time.deltaTime;

        if (timer > alertedCheckTime)
        {
            if (enableFOVDetection && !enableSoundDetection)
            {
                if (FieldOfViewCheck())
                {
                    SwitchChase();
                }
            }
            else if (currentDistanceToPlayer < distanceToNoticePlayer)
            {
                SwitchChase();
            }
            else
            {
                SwitchIdle();
            }
        }
    }

    private void SwitchChase()
    {
        myAttributes.currentState = EntityAttributes.EntityState.CHASE;
        currentTarget = player;
        currentSpeed = myAttributes.sprintSpeed;
    }

    private void UpdateChase()
    {
        if (player == null)
        {
            SwitchIdle();
        }

        if (currentDistanceToPlayer > distanceToNoticePlayer)
        {
            SwitchAlerted();
        }

        currentSpeed = myAttributes.sprintSpeed;
        currentTarget = player;

        if (currentDistanceToPlayer < currentWeapon.reach && !currentWeapon.attack)
        {
            currentWeapon.attack = true;
        }
    }

    private bool FieldOfViewCheck()
    {
        if (currentDistanceToPlayer < rangeFOV)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float anglePlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (Mathf.Abs(anglePlayer) < angleFOV / 2)
            {
                //check player is behind obstical
                if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, rangeFOV))
                {
                    if (hit.transform == player.transform)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool SoundDetectionCheck()
    {
        if (currentDistanceToPlayer < rangeSD)
        {
            foreach (EntityAttributes.EntityState state in noDetectionState)
            {
                if (player.GetComponent<EntityAttributes>().currentState == state)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}
