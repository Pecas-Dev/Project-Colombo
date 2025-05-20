using ProjectColombo.GameInputSystem;
using System.Collections;
using UnityEngine;


namespace ProjectColombo.StateMachine.Player
{
    public class PlayerBlockState : PlayerBaseState
    {
        public PlayerBlockState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
        {

        }

        public override void Enter()
        {
            stateMachine.SetCurrentState(PlayerStateMachine.PlayerState.Block);
            stateMachine.myPlayerAnimator.TriggerBlock();
            Debug.Log("Player entered Blocking State");

            stateMachine.isBlocking = true;
            stateMachine.gameInputSO.DisableAllInputsExcept(
                InputActionType.Movement,
                InputActionType.Block,
                InputActionType.Roll,
                InputActionType.MajorAttack,
                InputActionType.MinorAttack,
                InputActionType.MajorParry,
                InputActionType.MinorParry,
                InputActionType.Pause);

            stateMachine.StartCoroutine(StopMovement());
        }

        IEnumerator StopMovement()
        {
            yield return new WaitForFixedUpdate();

            //reset velocities
            stateMachine.myRigidbody.linearVelocity = Vector3.zero;
            stateMachine.myRigidbody.angularVelocity = Vector3.zero;
        }

        public override void Tick(float deltaTime)
        {
            if (stateMachine.gameInputSO.MovementInput.magnitude > 0.01f)
            {
                Vector2 moveInput = stateMachine.gameInputSO.MovementInput;
                Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

                // Convert input to isometric space
                Vector3 isometricDirection = TransformDirectionToIsometric(moveDirection);

                if (isometricDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(isometricDirection);
                    stateMachine.myRigidbody.MoveRotation(Quaternion.Slerp(stateMachine.myRigidbody.rotation, targetRotation, Time.fixedDeltaTime * stateMachine.myEntityAttributes.rotationSpeedPlayer));
                }
            }


            HandleStateSwitchFromInput();

            if (!stateMachine.gameInputSO.BlockPressed())
            {
                stateMachine.SwitchState(new PlayerMovementState(stateMachine));
                return;
            }
        }

        public override void Exit()
        {
            stateMachine.gameInputSO.EnableAllInputs();
            stateMachine.isBlocking = false;
        }
    }
}