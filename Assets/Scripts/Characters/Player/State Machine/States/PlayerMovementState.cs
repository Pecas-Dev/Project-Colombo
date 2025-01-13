using UnityEngine;

namespace ProjectColombo.StateMachine.Player
{
    public class PlayerMovementState : PlayerBaseState
    {
        Vector2 movementInput;

        public PlayerMovementState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
        }

        public override void Enter()
        {
            Debug.Log("Entered Movement State");
        }

        public override void Tick(float deltaTime)
        {
            if (m_playerStateMachine.GameInput.AttackPressed)
            {
                m_playerStateMachine.GameInput.ResetAttackPressed();
                m_playerStateMachine.SwitchState(new PlayerAttackState(m_playerStateMachine));
                return;
            }

            if (m_playerStateMachine.GameInput.RollPressed)
            {
                m_playerStateMachine.GameInput.ResetRollPressed();
                m_playerStateMachine.SwitchState(new PlayerRollState(m_playerStateMachine));

                return;
            }

            HandleMovement(deltaTime);
            HandleRotation(deltaTime);
            UpdateAnimator();

            if (movementInput.sqrMagnitude < 0.01f)
            {
                StopMovementAndRotation();
                return;
            }
        }

        public override void Exit()
        {
            Debug.Log("Exited Movement State");
        }

        void HandleMovement(float deltaTime)
        {
            movementInput = m_playerStateMachine.GameInput.MovementInput;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y) * m_playerStateMachine.EntityAttributes.moveSpeed * movementInput.magnitude;

                m_playerStateMachine.PlayerRigidbody.MovePosition(m_playerStateMachine.PlayerRigidbody.position + desiredVelocity * deltaTime);
            }
            else
            {
                m_playerStateMachine.PlayerRigidbody.linearVelocity = Vector3.zero;
            }
        }

        void HandleRotation(float deltaTime)
        {
            if (movementInput.sqrMagnitude > 0.01f)
            {
                Vector3 targetDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                m_playerStateMachine.PlayerRigidbody.rotation = Quaternion.RotateTowards(m_playerStateMachine.PlayerRigidbody.rotation, targetRotation, m_playerStateMachine.EntityAttributes.rotationSpeedPlayer * deltaTime);
            }
        }

        void StopMovementAndRotation()
        {
            m_playerStateMachine.PlayerRigidbody.linearVelocity = Vector3.zero;
            m_playerStateMachine.PlayerRigidbody.angularVelocity = Vector3.zero;
        }

        void UpdateAnimator()
        {
            float speed = movementInput.magnitude * m_playerStateMachine.EntityAttributes.moveSpeed;
            bool hasMovementInput = movementInput.sqrMagnitude > 0.01f;

            m_playerStateMachine.PlayerAnimator.UpdateAnimator(speed, false, hasMovementInput);
        }
    }
}
