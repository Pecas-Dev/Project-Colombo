using UnityEngine;


public class EntityAttributes : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Controls the Movement Speed.")]
    public float moveSpeed = 5f;

    [Tooltip("Controls the Rotation Speed (Degrees per second).")]
    public float rotationSpeedPlayer = 720f;


    [Header("Health Variables")]
    [Tooltip("Max Health of the Entity")]
    public int maxHealth;
    public int health;


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
