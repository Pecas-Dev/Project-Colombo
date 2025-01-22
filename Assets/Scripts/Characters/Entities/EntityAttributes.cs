using UnityEngine;


public class EntityAttributes : MonoBehaviour
{
     [Header("Movement Settings")]
    [Tooltip("Controls the Movement Speed.")]
    public float moveSpeed = 5f;

    [Tooltip("Controls the Rotation Speed (Degrees per second).")]
    public float rotationSpeedPlayer = 720f;


    [Header("Movement Settings - DEPRECATED")]
    [Tooltip("Controls the Acceleration. - DEPRECATED")]
    public float acceleration = 10f;

    [Tooltip("Controls the Deceleration. - DEPRECATED")]
    public float deceleration = 10f;

    [Tooltip("Controls the Grace Period for maintaining velocity after input stops (Time in seconds). - DEPRECATED")]
    public float graceTime = 0.1f;

    [Tooltip("Delay after rolling before allowing another roll (Time in seconds). - DEPRECATED")]
    public float rollDelay = 0.2f;



    [Header("Julian Variables (To REFACTOR [currentState])")]
    public int maxHealth;
    [HideInInspector] public int health;


    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
        if (health <= 0) // Here can be stuff that is universal for all entities
        {
            //Destroy(this.gameObject);
        }
    }

    public Vector3 GetFacingDirection()
    {
        return transform.forward;
    }
}
