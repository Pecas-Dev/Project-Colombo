using ProjectColombo.Control;
using UnityEngine;
using UnityEngine.InputSystem;

public class temp_StateManager : MonoBehaviour
{
    EntityAttributes myEntityAttributes;
    Rigidbody myRigidbody;
    public GameInput myInputs;

    private void Start()
    {
        myEntityAttributes = GetComponent<EntityAttributes>();
        myRigidbody = GetComponent<Rigidbody>();
        myEntityAttributes.currentState = EntityAttributes.EntityState.IDLE;
    }

    private void Update()
    {
        if (myInputs.CrouchPressed)
        {
            myEntityAttributes.currentState = EntityAttributes.EntityState.SNEAK;
        }
        else if (myInputs.AttackPressed)
        {
            myEntityAttributes.currentState = EntityAttributes.EntityState.ATTACK;
        }
        else if (myInputs.MovementInput.magnitude > 0.2f)
        {
            myEntityAttributes.currentState = EntityAttributes.EntityState.WALK;
        }
        else
        {
            myEntityAttributes.currentState = EntityAttributes.EntityState.IDLE;
        }
    }
}