using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerMovementState : PlayerBaseState
    {

        public PlayerMovementState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {

        }

        public override void Enter()
        {
            stateMachine.ParryFrameStop();
            stateMachine.ParryPanaltyStop();
            stateMachine.currentComboString = "";
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Movement);
            stateMachine.myPlayerAnimator.PlayMovementAnimation();
            stateMachine.gameInputSO.EnableAllInputs();
        }

        public override void Tick(float deltaTime)
        {
            HandleStateSwitchFromInput();

            HandleMovement(deltaTime);
            HandleRotation(deltaTime);

            UpdateAnimator();
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 moveInput = stateMachine.gameInputSO.GetVector2Input(GameInputSystem.PlayerInputAction.Movement);
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

            float inputMagnitude = moveDirection.magnitude;

            if (inputMagnitude > 0.01f)
            {
                Vector3 isometricDirection = TransformDirectionToIsometric(moveDirection / inputMagnitude);

                float currentYVelocity = stateMachine.myRigidbody.linearVelocity.y;

                Vector3 targetVelocity = isometricDirection * inputMagnitude * stateMachine.myEntityAttributes.moveSpeed;

                targetVelocity.y = currentYVelocity;

                stateMachine.myRigidbody.linearVelocity = targetVelocity;
            }
            else
            {
                stateMachine.myRigidbody.linearVelocity = new Vector3(0, stateMachine.myRigidbody.linearVelocity.y, 0);
            }
        }

        private void HandleRotation(float deltaTime)
        {
            stateMachine.myRigidbody.angularVelocity = Vector3.zero;
            Vector2 moveInput = stateMachine.gameInputSO.GetVector2Input(GameInputSystem.PlayerInputAction.Movement);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

                moveDirection = TransformDirectionToIsometric(moveDirection);

                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                    stateMachine.myRigidbody.MoveRotation(Quaternion.Slerp(stateMachine.myRigidbody.rotation, targetRotation, Time.fixedDeltaTime * stateMachine.myEntityAttributes.rotationSpeedPlayer));
                }
            }
            else
            {
                stateMachine.myRigidbody.angularVelocity = Vector3.zero;
            }
        }

        void UpdateAnimator()
        {
            float animationSpeed = stateMachine.gameInputSO.GetVector2Input(GameInputSystem.PlayerInputAction.Movement).magnitude;
            bool hasMovementInput = animationSpeed > 0.01f;

            stateMachine.myPlayerAnimator.UpdateAnimator(animationSpeed, false, hasMovementInput);
        }
    }
}