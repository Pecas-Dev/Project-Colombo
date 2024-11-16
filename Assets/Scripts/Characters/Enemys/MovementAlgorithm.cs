using System.Xml.Serialization;
using UnityEngine;

public class MovementAlgorithm : MonoBehaviour
{
    EnemyAttributes myEnemyAttributes;
    Rigidbody myRigidbody;
    GameObject currentTarget;

    Vector3 movingDirection;

    private void Start()
    {
        myEnemyAttributes = GetComponent<EnemyAttributes>();
        myRigidbody = GetComponent<Rigidbody>();
        currentTarget = myEnemyAttributes.currentTarget;
    }

    private void Update()
    {
        currentTarget = myEnemyAttributes.currentTarget;


        //to do: implement moving direction calculations with algorithm
        movingDirection = currentTarget.transform.position - transform.position;
        movingDirection.y = 0;

    }

    private void FixedUpdate()
    {
        if (Vector3.Angle(transform.forward, movingDirection) > 1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movingDirection);
            myRigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, myEnemyAttributes.currentRotationSpeed * Time.fixedDeltaTime));
        }
        
        if (Vector3.Angle(transform.forward, movingDirection) < 90f && movingDirection.magnitude > myEnemyAttributes.distanceToSwitchPatrolPoints / 2)
        {
            myRigidbody.MovePosition(transform.position + transform.forward * myEnemyAttributes.currentSpeed * Time.fixedDeltaTime);
        }
    }
}
