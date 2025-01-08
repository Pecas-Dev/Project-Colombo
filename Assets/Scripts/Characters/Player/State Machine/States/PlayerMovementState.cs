using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerMovementState : PlayerBaseState
    {
        private Vector2 movementInput;
        private Vector3 currentVelocity = Vector3.zero;

        public PlayerMovementState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            Debug.Log("Entered Movement State");
        }

        public override void Tick(float deltaTime)
        {
            HandleMovement(deltaTime);
            HandleRotation(deltaTime);
            UpdateAnimator();
        }

        public override void Exit()
        {
            Debug.Log("Exited Movement State");
        }

        private void HandleMovement(float deltaTime)
        {
            movementInput = m_playerStateMachine.GameInput.MovementInput;

            Vector3 desiredVelocity = Vector3.zero;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y).normalized * m_playerStateMachine.EntityAttributes.moveSpeed;
            }

            float accelerationRate = m_playerStateMachine.EntityAttributes.acceleration * (movementInput.sqrMagnitude > 0.01f ? 1.5f : 1.0f);
            float decelerationRate = m_playerStateMachine.EntityAttributes.deceleration * 1.8f;

            float maxSpeedChange = (movementInput.sqrMagnitude > 0.01f ? accelerationRate : decelerationRate) * deltaTime;
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, maxSpeedChange);

            m_playerStateMachine.PlayerRigidbody.MovePosition(m_playerStateMachine.PlayerRigidbody.position + currentVelocity * deltaTime);
        }

        private void HandleRotation(float deltaTime)
        {
            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 targetDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                m_playerStateMachine.PlayerRigidbody.rotation = Quaternion.RotateTowards(m_playerStateMachine.PlayerRigidbody.rotation, targetRotation, m_playerStateMachine.EntityAttributes.rotationSpeedPlayer * deltaTime * 2f);
            }
        }

        private void UpdateAnimator()
        {
            float speed = currentVelocity.magnitude;
            bool hasMovementInput = movementInput.sqrMagnitude > 0.01f;

            if (!hasMovementInput && speed < 0.01f)
            {
                speed = 0; 
            }

            m_playerStateMachine.PlayerAnimator.UpdateAnimator(speed, false, hasMovementInput);
        }
    }
}

