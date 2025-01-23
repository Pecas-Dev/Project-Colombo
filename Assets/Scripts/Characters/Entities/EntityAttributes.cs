using UnityEngine;


public class EntityAttributes : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Controls the Movement Speed.")]
    public float moveSpeed = 5f;

    [Tooltip("Controls the Rotation Speed (Degrees per second).")]
    public float rotationSpeedPlayer = 720f;

    public Vector3 GetFacingDirection()
    {
        return transform.forward;
    }
}
