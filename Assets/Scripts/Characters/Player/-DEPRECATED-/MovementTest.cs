using ProjectColombo.GameInputSystem;
using UnityEngine;

namespace ProjectColombo.Control
{
    public class MovementTest : MonoBehaviour
    {
        EntityAttributes myEntityAttributes;
        Rigidbody myRigidbody;
        Animator myAnimator;
        public GameInputSO myInputs;

        private void Start()
        {
            myEntityAttributes = GetComponent<EntityAttributes>();
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            myInputs.Initialize();
            myInputs.EnableAllInputs();

            // Freeze rotation to avoid any rotation from physics forces
            myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            Vector2 moveInput = myInputs.MovementInput;
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

            // Only apply movement if input exists
            if (moveDirection.magnitude > 0.1f)
            {
                // Get the current Y velocity (gravity should handle this)
                float currentYVelocity = myRigidbody.linearVelocity.y;

                // Apply force for movement (preserve Y velocity, handle gravity naturally)
                Vector3 targetVelocity = moveDirection * myEntityAttributes.moveSpeed;
                targetVelocity.y = currentYVelocity; // Maintain the Y velocity (gravity)

                myRigidbody.linearVelocity = targetVelocity;

                // Update animator speed based on movement magnitude
                myAnimator.SetFloat("speed", moveDirection.magnitude * myEntityAttributes.moveSpeed);
            }
            else
            {
                // No input - just apply the current Y velocity and stop horizontal movement
                myAnimator.SetFloat("speed", 0);
                myRigidbody.linearVelocity = new Vector3(0, myRigidbody.linearVelocity.y, 0);
            }
        }

        private void HandleRotation()
        {
            Vector2 moveInput = myInputs.MovementInput;
            if (moveInput.sqrMagnitude > 0.01f)
            {
                // Create a direction vector based on movement input
                Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

                // Calculate the target rotation based on movement
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                // Smoothly rotate to the desired direction
                myRigidbody.MoveRotation(Quaternion.Slerp(
                    myRigidbody.rotation,
                    targetRotation,
                    Time.fixedDeltaTime * myEntityAttributes.rotationSpeedPlayer
                ));
            }
            else
            {
                // If no movement input, zero out angular velocity
                myRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
