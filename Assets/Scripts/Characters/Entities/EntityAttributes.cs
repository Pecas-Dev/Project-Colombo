using UnityEngine;



public class EntityAttributes : MonoBehaviour
{
    //this will be the states for all entites
    public enum EntityState {IDLE, WALK, SPRINT, SNEAK, ATTACK, PATROL, ALERTED, CHASE};


    [Header("Movement Settings")]
    [Tooltip("Controls the Movement Speed.")]
    public float moveSpeed = 5f;

    [Tooltip("Controls the Rotation Speed (Degrees per second).")]
    public float rotationSpeedPlayer = 720f;

    [Tooltip("Controls the Acceleration.")]
    public float acceleration = 10f;

    [Tooltip("Controls the Deceleration.")]
    public float deceleration = 10f;

    [Tooltip("Controls the Grace Period for maintaining velocity after input stops (Time in seconds).")]
    public float graceTime = 0.1f;



    [Header("Julien Variables (To Adapt)")]
    public EntityState currentState;
    public int health;
    public float walkSpeed; // For enemies' patrol speed
    public float sprintSpeed; // For enemies' chase speed
    public float rotationSpeed;
    [HideInInspector] public bool grounded;
    public float jumpForce;
    [HideInInspector] public Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        currentState = EntityState.IDLE;
    }

    void Update()
    {
        if (health <= 0) // Here can be stuff that is universal for all entities
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }
}
