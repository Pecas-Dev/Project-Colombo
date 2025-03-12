using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public abstract class MommottiBaseState : State
    {
        protected MommottiStateMachine stateMachine;
        private Vector3 currentVelocity = Vector3.zero;
        public MommottiBaseState(MommottiStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        protected void MoveToTarget(Vector3 targetPosition, float deltaTime, float speed)
        {
            Vector3 direction = targetPosition - stateMachine.transform.position;

            if (direction.magnitude > 0.1f) // Ensure we don’t overshoot or start lerping if already close
            {
                // Use SmoothDamp for smoother, damped movement
                stateMachine.transform.position = Vector3.SmoothDamp(
                    stateMachine.transform.position,  // Current position
                    targetPosition,                    // Target position
                    ref currentVelocity,               // Reference to the current velocity
                    0.3f,                              // Smooth time (higher value = slower movement)
                    speed,                             // Speed of the movement (max speed)
                    deltaTime                          // DeltaTime for frame-rate independence
                );
            }
        }

        protected void RotateTowardsTarget(Vector3 targetPosition, float deltaTime, float rotationSpeed)
        {
            Vector3 direction = (targetPosition - stateMachine.transform.position).normalized;

            if (direction.sqrMagnitude > 0.001f) // Prevent jittering when already aligned
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }
    }
}
