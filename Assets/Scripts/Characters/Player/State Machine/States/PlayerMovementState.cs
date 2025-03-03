using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerMovementState : PlayerBaseState
    {
        Vector2 movementInput;
        Matrix4x4 isometricMatrix;

        public PlayerMovementState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {
            isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, m_playerStateMachine.Angle, 0));
        }

        public override void Enter()
        {
            m_playerStateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Movement);
            m_playerStateMachine.PlayerAnimatorScript.PlayMovementAnimation();
        }

        public override void Tick(float deltaTime)
        {
            if (m_playerStateMachine.GameInputSO.AttackPressed)
            {
                m_playerStateMachine.GameInputSO.ResetAttackPressed();
                m_playerStateMachine.SwitchState(new PlayerAttackState(m_playerStateMachine, 0));
                return;
            }

            if (m_playerStateMachine.GameInputSO.RollPressed)
            {
                m_playerStateMachine.GameInputSO.ResetRollPressed();
                m_playerStateMachine.SwitchState(new PlayerRollState(m_playerStateMachine));
                return;
            }

            if (m_playerStateMachine.GameInputSO.ParryPressed)
            {
                m_playerStateMachine.GameInputSO.ResetParryPressed();
                m_playerStateMachine.SwitchState(new PlayerParryState(m_playerStateMachine));
                return;
            }

            HandleMovement(deltaTime);
            HandleRotation(deltaTime);
            UpdateAnimator();
        }

        public override void Exit()
        {
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 moveInput = m_playerStateMachine.GameInputSO.MovementInput;

            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

            if (moveDirection.magnitude > 0.1f)
            {
                moveDirection = TransformDirectionToIsometric(moveDirection.normalized);

                float currentYVelocity = m_playerStateMachine.PlayerRigidbody.linearVelocity.y;

                Vector3 targetVelocity = moveDirection * m_playerStateMachine.EntityAttributes.moveSpeed;
                targetVelocity.y = currentYVelocity; 

                m_playerStateMachine.PlayerRigidbody.linearVelocity = targetVelocity;
            }
            else
            {
                m_playerStateMachine.PlayerRigidbody.linearVelocity = new Vector3(0, m_playerStateMachine.PlayerRigidbody.linearVelocity.y, 0);
            }
        }

        private void HandleRotation(float deltaTime)
        {
            Vector2 moveInput = m_playerStateMachine.GameInputSO.MovementInput;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

                moveDirection = TransformDirectionToIsometric(moveDirection);

                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                    m_playerStateMachine.PlayerRigidbody.MoveRotation(Quaternion.Slerp( m_playerStateMachine.PlayerRigidbody.rotation,targetRotation,Time.fixedDeltaTime * m_playerStateMachine.EntityAttributes.rotationSpeedPlayer ));
                }
            }
            else
            {
                m_playerStateMachine.PlayerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        void UpdateAnimator()
        {
            float speed = m_playerStateMachine.GameInputSO.MovementInput.magnitude * m_playerStateMachine.EntityAttributes.moveSpeed;
            bool hasMovementInput = m_playerStateMachine.GameInputSO.MovementInput.sqrMagnitude > 0.01f;

            m_playerStateMachine.PlayerAnimatorScript.UpdateAnimator(speed, false, hasMovementInput);
        }

        private Vector3 TransformDirectionToIsometric(Vector3 direction)
        {
            return isometricMatrix.MultiplyVector(direction).normalized;
        }
    }
}
