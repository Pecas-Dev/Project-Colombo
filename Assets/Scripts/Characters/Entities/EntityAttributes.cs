using UnityEngine;

namespace ProjectColombo
{
    public class EntityAttributes : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Controls the Movement Speed.")]
        public float moveSpeed = 5f;

        [Tooltip("Controls the Rotation Speed (Degrees per second).")]
        public float rotationSpeedPlayer = 720f;


        [Header("Attack Settings")]
        [Tooltip("Controls the Attack Impulse Forward.")]
        public float attackImpulseForce = 2.5f;
        [Tooltip("Stagger duration for now. We have to decide how to control it")]
        public float stunnedTime = 1f;

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}
