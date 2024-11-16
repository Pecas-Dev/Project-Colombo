using NUnit.Framework;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using System.Collections.Generic;

public class EnemyAttributes : MonoBehaviour
{
    [HideInInspector] public EntityAttributes myAttributes;
    public float distanceToNoticePlayer;
    public float distanceToSwitchPatrolPoints;
    WeaponAttributes currentWeapon;

    //current target
    [HideInInspector] public GameObject currentTarget; //walk to patrol point or player if chasing;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentRotationSpeed;

    //alerted by player
    GameObject player;
    float currentDistanceToPlayer;
    [HideInInspector] public bool isChasing; //public because maybe alert other enemies

    //patroling
    public string pathToFollowName;
    List<GameObject> patrolPath;
    float currentDistanceToPatrolPoint;
    int currentPatrolPoint;

    private void Start()
    {
        myAttributes = GetComponent<EntityAttributes>();
        player = GameObject.FindGameObjectWithTag("Player");
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
        //check if player is close enough to chase
        currentDistanceToPlayer = (player.transform.position - transform.position).magnitude;

        isChasing = currentDistanceToPlayer < distanceToNoticePlayer;

        if (isChasing)
        {
            currentSpeed = myAttributes.sprintSpeed;
            currentTarget = player;

            if (currentDistanceToPlayer < currentWeapon.reach && !currentWeapon.attack)
            {
                currentWeapon.attack = true;
            }
        }
        else
        {
            //when not chasing follow the patrolpath
            currentSpeed = myAttributes.walkSpeed;

            if (pathToFollowName.Length != 0)
            {
                currentDistanceToPatrolPoint = (currentTarget.transform.position - transform.position).magnitude;

                if (currentDistanceToPatrolPoint < distanceToSwitchPatrolPoints)
                {
                    currentPatrolPoint = (currentPatrolPoint + 1) % patrolPath.Count;
                }

                currentTarget = patrolPath[currentPatrolPoint];
            }
            else
            {
                currentTarget = this.gameObject;
            }
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
}
